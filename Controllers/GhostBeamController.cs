using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class GhostBeamController : MonoBehaviour
{
    const float UNLOCK_TRIGGER_RADIUS = 20f;

    TractorBeamController beam;
    SafetyTractorBeamController safetyBeam;
    OWTriggerVolume unlockTrigger;

    public BeamState State
    {
        get
        {
            if (!beam.IsActive()) return BeamState.Off;
            return beam._fluid.IsFluidReversed() ? BeamState.Reversed : BeamState.Forward;
        }
    }

    public void Init(TractorBeamController beam, bool triggersUnlock)
    {
        this.beam = beam;
        safetyBeam = beam.GetComponent<SafetyTractorBeamController>();

        // Vanilla beams keep whatever state their location set up, so nothing is switched here
        if (triggersUnlock)
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

    public void Cycle()
    {
        // Off, pushing along the beam, then pulling back down it. The same three states a
        // TractorBeamSwitch offers, for beams that never got a switch to offer them with
        switch (State)
        {
            case BeamState.Off:
                SetState(true, false);
                break;
            case BeamState.Forward:
                SetState(true, true);
                break;
            default:
                SetState(false, false);
                break;
        }
    }

    void SetState(bool active, bool reversed)
    {
        beam.SetReversed(reversed);
        if (safetyBeam != null)
        {
            // Safety beams have an alignment volume and an emissive fade to drive alongside the beam itself
            safetyBeam.SetActivation(active);
        }
        else
        {
            beam.SetActivation(active, false);
        }
    }

    void CreateUnlockTrigger()
    {
        var volume = new GameObject("BeamUnlockTrigger");
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
        // Similar to vanilla reveal volume but gated on the unlock hint fact
        if (!hitObj.CompareTag("PlayerDetector")) return;

        var shipLogManager = Locator.GetShipLogManager();
        if (!shipLogManager.IsFactRevealed(Constants.ShipLogFacts.BeamToolHint)) return;
        if (shipLogManager.IsFactRevealed(Constants.ShipLogFacts.BeamToolUnlock)) return;

        shipLogManager.RevealFact(Constants.ShipLogFacts.BeamToolUnlock);
        unlockTrigger.OnEntry -= HandleUnlockTriggerEntry;
    }

    public enum BeamState
    {
        Off,
        Forward,
        Reversed
    }
}
