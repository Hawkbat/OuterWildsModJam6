using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class GhostDoorController : MonoBehaviour
{
    const float ORB_TRAVEL_DURATION = 0.75f;

    NomaiMultiPartDoor door;
    NomaiGateway gateway;

    NomaiInterfaceOrb orb;
    OWRigidbody orbBody;
    NomaiInterfaceSlot idleSlot;
    NomaiInterfaceSlot targetSlot;
    Vector3 travelStart;
    float progress;
    bool returning;

    // A rotating door's activation slot is a momentary button, so its orb has to go home again before the controls will work a second time. Airlocks and gateways instead have the orb rest in whichever of a pair of slots matches the state, so it stays put and doubles as the readout of which way they are
    bool ReturnsHome => door != null && door is not NomaiAirlock;

    public bool IsOpen => door != null ? door.IsOpen() || door.IsOpening() : gateway._open;
    public bool IsCycling => enabled || (door != null ? door.IsCycling() : gateway.enabled);

    public void Init(NomaiMultiPartDoor door)
    {
        this.door = door;
        enabled = false;
    }

    public void Init(NomaiGateway gateway)
    {
        this.gateway = gateway;
        enabled = false;

        // Fix gateway grabbing orb audio source instead of its own
        var gatewayAudio = gateway.GetComponentsInChildren<OWAudioSource>(true)
            .FirstOrDefault(source => gateway._orb == null || !source.transform.IsChildOf(gateway._orb.transform));
        if (gatewayAudio != null)
        {
            gateway._audioSource = gatewayAudio;
        }
    }

    public void Cycle()
    {
        if (IsCycling) return;

        // Sliding an orb into its slot works the mechanism through the same slot event the Nomai controls use, so the audio, the panel locking and the orb suspension all behave as they should
        orb = FindOrb();
        orbBody = orb != null ? orb.GetAttachedOWRigidbody() : null;
        idleSlot = orb != null ? orb.GetComponentInParent<NomaiInterfaceSlot>() : null;
        var slot = orb != null ? FindTargetSlot() : null;
        if (orbBody == null || slot == null)
        {
            // Nothing we can move, so drive the mechanism directly instead
            DriveMechanism();
            return;
        }

        StartTravel(slot, false);
    }

    void DriveMechanism()
    {
        if (door != null)
        {
            // Vanilla passes a null slot here too, and airlocks override Cycle to swap their air over with it
            door.Cycle(null);
        }
        else if (IsOpen)
        {
            gateway.CloseGate(null);
        }
        else
        {
            gateway.OpenGate(null);
        }
    }

    void StartTravel(NomaiInterfaceSlot slot, bool returning)
    {
        targetSlot = slot;
        travelStart = orb.transform.position;
        progress = 0f;
        this.returning = returning;
        enabled = true;
    }

    protected void Update()
    {
        // The orb is locked to the panel it activated for as long as the door is swinging, so wait that out and keep the trip home anchored to wherever the panel has carried it
        if (returning && (door.IsCycling() || orbBody.IsSuspended()))
        {
            travelStart = orb.transform.position;
            return;
        }

        progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime / ORB_TRAVEL_DURATION);
        var t = Mathf.SmoothStep(0f, 1f, progress);
        orb.SetOrbPosition(Vector3.Lerp(travelStart, targetSlot.transform.position, t));

        if (progress < 1f) return;

        if (returning)
        {
            enabled = false;
            return;
        }

        // Landing in the slot should have set the mechanism off, but make sure it actually moves
        if (!IsMechanismCycling())
        {
            DriveMechanism();
        }

        if (ReturnsHome)
        {
            // Park the orb back where it started so the controls are ready to be used again
            StartTravel(idleSlot, true);
        }
        else
        {
            enabled = false;
        }
    }

    bool IsMechanismCycling()
    {
        return door != null ? door.IsCycling() : gateway.enabled;
    }

    NomaiInterfaceOrb FindOrb()
    {
        if (gateway != null) return gateway._orb;

        // Intact doors have an orb on either face, so use whichever one is on the player's side
        var position = Locator.GetPlayerTransform().position;
        return door._listInterfaceOrb
            .Where(orb => orb != null && orb.gameObject.activeInHierarchy && !orb.IsBeingDragged())
            .OrderBy(orb => Vector3.SqrMagnitude(orb.transform.position - position))
            .FirstOrDefault();
    }

    NomaiInterfaceSlot FindTargetSlot()
    {
        if (gateway != null)
        {
            return IsOpen ? gateway._closeSlot : gateway._openSlot;
        }
        if (door is NomaiAirlock)
        {
            var slots = IsOpen ? door._closeSwitches : door._openSwitches;
            return slots.FirstOrDefault(slot => slot != null);
        }

        // Both variants of the door prefab name their slots in pairs, so IdleSlot_Front belongs with
        // ActivateSlot_Front, while the broken variant just has the one unsuffixed IdleSlot/ActivateSlot
        if (idleSlot == null) return null;
        var slotName = idleSlot.name.Replace("IdleSlot", "ActivateSlot");
        return door._cycleSwitches.FirstOrDefault(slot => slot != null && slot.name == slotName);
    }
}
