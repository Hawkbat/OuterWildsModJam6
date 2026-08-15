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

    // Rotating doors use a momentary activation slot, so the orb has to go home; airlocks and gateways park it in whichever slot matches the state
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

        // Sliding the orb in works the mechanism through the same slot event the Nomai controls use, so audio, panel locking and orb suspension all follow
        orb = FindOrb();
        orbBody = orb != null ? orb.GetAttachedOWRigidbody() : null;
        idleSlot = orb != null ? orb.GetComponentInParent<NomaiInterfaceSlot>() : null;
        var slot = orb != null ? FindTargetSlot() : null;
        if (orbBody == null || slot == null)
        {
            DriveMechanism();
            return;
        }

        StartTravel(slot, false);
    }

    void DriveMechanism()
    {
        if (door != null)
        {
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
        // The orb stays locked to its panel while the door swings, so re-anchor the trip home to wherever that carries it
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

        if (!IsMechanismCycling())
        {
            DriveMechanism();
        }

        if (ReturnsHome)
        {
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

        // Slots are named in pairs (IdleSlot_Front / ActivateSlot_Front); the broken variant has one unsuffixed pair
        if (idleSlot == null) return null;
        var slotName = idleSlot.name.Replace("IdleSlot", "ActivateSlot");
        return door._cycleSwitches.FirstOrDefault(slot => slot != null && slot.name == slotName);
    }
}
