using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class GhostDoorController : MonoBehaviour
{
    const float ORB_TRAVEL_DURATION = 0.75f;
    const float UNLOCK_TRIGGER_RADIUS = 20f;

    NomaiMultiPartDoor door;
    OWTriggerVolume unlockTrigger;

    NomaiInterfaceOrb orb;
    OWRigidbody orbBody;
    NomaiInterfaceSlot idleSlot;
    NomaiInterfaceSlot targetSlot;
    Vector3 travelStart;
    float progress;
    bool returning;

    public bool IsOpen => door.IsOpen() || door.IsOpening();
    public bool IsCycling => enabled || door.IsCycling();

    public void Init(NomaiMultiPartDoor door)
    {
        this.door = door;
        enabled = false;

        // Only broken doors (single working switch) trigger the unlock memory
        if (door._cycleSwitches.Length < 2)
        {
            CreateUnlockTrigger();
        }
    }

    protected void OnDestroy()
    {
        if (unlockTrigger != null)
        {
            unlockTrigger.OnEntry -= HandleUnlockTriggerEntry;
        }
    }

    void CreateUnlockTrigger()
    {
        var volume = new GameObject("DoorUnlockTrigger");
        volume.transform.SetParent(transform, false);
        volume.layer = LayerMask.NameToLayer("BasicEffectVolume");
        var col = volume.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = UNLOCK_TRIGGER_RADIUS;
        unlockTrigger = volume.AddComponent<OWTriggerVolume>();
        unlockTrigger.OnEntry += HandleUnlockTriggerEntry;
    }

    void HandleUnlockTriggerEntry(GameObject hitObj)
    {
        // Similar to vanilla reveal volume but gated on the unlock hint fact so it doesn't trigger on statue island at the start
        if (!hitObj.CompareTag("PlayerDetector")) return;

        var shipLogManager = Locator.GetShipLogManager();
        if (!shipLogManager.IsFactRevealed(Constants.ShipLogFacts.DoorToolHint)) return;
        if (shipLogManager.IsFactRevealed(Constants.ShipLogFacts.DoorToolUnlock)) return;

        shipLogManager.RevealFact(Constants.ShipLogFacts.DoorToolUnlock);
        unlockTrigger.OnEntry -= HandleUnlockTriggerEntry;
    }

    public void Cycle()
    {
        if (IsCycling) return;

        // Sliding an orb into its activation slot works the door through the same slot event the Nomai controls use, so the audio, the panel locking and the orb suspension all behave as they should
        orb = FindNearestOrb();
        orbBody = orb != null ? orb.GetAttachedOWRigidbody() : null;
        idleSlot = orb != null ? orb.GetComponentInParent<NomaiInterfaceSlot>() : null;
        var activationSlot = idleSlot != null ? FindActivationSlot(idleSlot) : null;
        if (orbBody == null || activationSlot == null)
        {
            // Nothing we can move, so drive the door directly instead. Vanilla passes a null slot here too
            door.Cycle(null);
            return;
        }

        StartTravel(activationSlot, false);
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
        }
        else
        {
            // Landing in the slot should have set the door off, but make sure it actually moves
            if (!door.IsCycling())
            {
                door.Cycle(null);
            }
            // Park the orb back where it started so the controls are ready to be used again
            StartTravel(idleSlot, true);
        }
    }

    NomaiInterfaceOrb FindNearestOrb()
    {
        // Intact doors have an orb on either face, so use whichever one is on the player's side
        var position = Locator.GetPlayerTransform().position;
        return door._listInterfaceOrb
            .Where(orb => orb != null && orb.gameObject.activeInHierarchy && !orb.IsBeingDragged())
            .OrderBy(orb => Vector3.SqrMagnitude(orb.transform.position - position))
            .FirstOrDefault();
    }

    NomaiInterfaceSlot FindActivationSlot(NomaiInterfaceSlot idleSlot)
    {
        // Both variants of the door prefab name their slots in pairs, so IdleSlot_Front belongs with
        // ActivateSlot_Front, while the broken variant just has the one unsuffixed IdleSlot/ActivateSlot
        var slotName = idleSlot.name.Replace("IdleSlot", "ActivateSlot");
        return door._cycleSwitches.FirstOrDefault(slot => slot != null && slot.name == slotName);
    }
}
