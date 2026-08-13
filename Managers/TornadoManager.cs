using UnityEngine;

namespace GhostInTheMachine.Managers;

public class TornadoManager : ManagerBase<TornadoManager>
{

    protected override void Awake()
    {
        base.Awake();

        var tornadoFluid = GameObject.Find("StatueIsland_Body/Sector_StatueIsland/GITM_TORNADO/MockDownTornado_FluidCenter").GetComponent<TornadoFluidVolume>();

        // Vanilla is -300f for downward tornados but NH defaults to -100f
        tornadoFluid._verticalSpeed = -500f;

        var tractorBeam = GameObject.Find("StatueIsland_Body/Sector_StatueIsland/GITM_TORNADO_LAUNCHER").GetComponent<TractorBeamController>();

        // Speed is applied along -up, so the forward direction pushes down into the island and only the reversed direction throws the player up into the tornado. TractorBeamManager starts it switched off, so the staff has to both power it up and flip it before it's any use
        tractorBeam._fluid._verticalSpeed = 64f;
        tractorBeam._fluid._reverseSpeed = -64f;
        tractorBeam.SetActivation(false, true);
    }

}
