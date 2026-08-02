using UnityEngine;

namespace GhostInTheMachine.Managers;

public class TornadoManager : ManagerBase<TornadoManager>
{

    protected override void Awake()
    {
        base.Awake();

        var tornadoFluid = GameObject.Find("GabbroIsland_Body/Sector_GabbroIsland/GITM_TORNADO/MockDownTornado_FluidCenter").GetComponent<TornadoFluidVolume>();

        // Vanilla is -300f for downward tornados but NH defaults to -100f
        tornadoFluid._verticalSpeed = -500f;

        var tractorBeam = GameObject.Find("GabbroIsland_Body/Sector_GabbroIsland/GITM_TORNADO_LAUNCHER").GetComponent<TractorBeamController>();

        tractorBeam._fluid._reverseSpeed = tractorBeam._fluid._verticalSpeed = -64f;
        tractorBeam.SetActivation(true);
    }

}
