using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class GhostBeamController : MonoBehaviour
{
    TractorBeamController beam;
    SafetyTractorBeamController safetyBeam;

    public BeamState State
    {
        get
        {
            if (!beam.IsActive()) return BeamState.Off;
            return beam._fluid.IsFluidReversed() ? BeamState.Reversed : BeamState.Forward;
        }
    }

    public void Init(TractorBeamController beam)
    {
        this.beam = beam;
        safetyBeam = beam.GetComponent<SafetyTractorBeamController>();
    }

    public void Cycle()
    {
        AchievementManager.Instance.OnBeamCycled(this);

        // Cycles between off, forward, reversed
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

    public enum BeamState
    {
        Off,
        Forward,
        Reversed
    }
}
