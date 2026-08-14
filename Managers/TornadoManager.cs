using GhostInTheMachine.Controllers;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class TornadoManager : ManagerBase<TornadoManager>
{
    const string TORNADO_FLUID_PATH = "StatueIsland_Body/Sector_StatueIsland/GITM_TORNADO/MockDownTornado_FluidCenter";
    const string TORNADO_LAUNCHER_PATH = "StatueIsland_Body/Sector_StatueIsland/GITM_TORNADO_LAUNCHER";
    const string STORAGE_ROOM_BEAM_PATH = "StatueIsland_Body/Sector_StatueIsland/GITM_STORAGE_BEAM";

    const string ORB_VISUALS_PATH = "BrittleHollow_Body/Sector_BH/Sector_SouthHemisphere/Sector_SouthPole/Sector_Observatory/Interactables_Observatory/HologramProjector/Prefab_NOM_OrbTrack_Vertical/Prefab_NOM_InterfaceOrb/Props_NOM_Orb";

    const float ORB_VISUALS_SCALE = 0.6f;
    const float ORB_COLLIDER_RADIUS = 0.5f;

    const string ORB_PARENT_PATH = "StatueIsland_Body";
    static readonly Vector3 ORB_POSITION = new(-9.728863f, 34.0171f, 86.11772f);
    static readonly Vector3 ORB_ROTATION = Vector3.zero;

    const string ORB_SOCKET_PARENT_PATH = "StatueIsland_Body/Sector_StatueIsland/Tornado Lab";
    static readonly Vector3 ORB_SOCKET_POSITION = new(24.27f, 4.9844f, -110.7641f);
    static readonly Vector3 ORB_SOCKET_ROTATION = Vector3.zero;

    ArtificialTornadoController tornado;
    GameObject orbPrefab;

    protected override void Awake()
    {
        base.Awake();

        var tornadoFluid = GameObject.Find(TORNADO_FLUID_PATH).GetComponent<TornadoFluidVolume>();
        // Vanilla is -300f for downward tornados but NH defaults to -100f
        tornadoFluid._verticalSpeed = -200f;

        var tornadoController = tornadoFluid.GetComponentInParent<TornadoController>();
        // Make the tornado skinnier
        tornadoController.transform.localScale = Vector3.Scale(tornadoController.transform.localScale, new(0.6f, 1f, 0.6f));
        // Take over tornado controller with our own implementation
        tornado = tornadoController.gameObject.AddComponent<ArtificialTornadoController>();
        tornado.Init(tornadoController);
        tornado.SetActivatedImmediate(DialogueConditionManager.SharedInstance.GetConditionState(Constants.DialogueConditions.TornadoActivated));

        var tractorBeam = GameObject.Find(TORNADO_LAUNCHER_PATH).GetComponent<TractorBeamController>();

        // Speed is applied along -up, so the forward direction pushes down into the island and only the reversed direction throws the player up into the tornado. TractorBeamManager starts it switched off, so the staff has to both power it up and flip it before it's any use
        tractorBeam._fluid._verticalSpeed = 64f;
        tractorBeam._fluid._reverseSpeed = -64f;
        tractorBeam.SetActivation(false, true);

        orbPrefab = CreateOrbPrefab();

        GlobalMessenger<string, bool>.AddListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("DialogueConditionChanged", OnDialogueConditionChanged);
    }

    public void PlaceOrbAndSocket()
    {
        PlaceOrb(ORB_PARENT_PATH, ORB_POSITION, ORB_ROTATION);
        PlaceOrbSocket(ORB_SOCKET_PARENT_PATH, ORB_SOCKET_POSITION, ORB_SOCKET_ROTATION);
    }

    public void SetOrbInstalled(bool installed)
    {
        DialogueConditionManager.SharedInstance.SetConditionState(Constants.DialogueConditions.TornadoActivated, installed);
    }

    public NomaiOrbItem GivePlayerOrb()
    {
        var orb = Instantiate(orbPrefab).GetComponent<NomaiOrbItem>();
        orb.gameObject.SetActive(true);
        Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(orb);
        return orb;
    }

    GameObject CreateOrbPrefab()
    {
        var prefab = new GameObject("GITM_ORB");
        prefab.layer = LayerMask.NameToLayer("Interactible");

        var visuals = GhostInTheMachine.CloneVanillaProp(ORB_VISUALS_PATH);
        visuals.transform.SetParent(prefab.transform, false);
        visuals.transform.localPosition = Vector3.zero;
        visuals.transform.localScale = Vector3.one * ORB_VISUALS_SCALE;

        var collider = prefab.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = ORB_COLLIDER_RADIUS;

        prefab.AddComponent<NomaiOrbItem>();
        prefab.SetActive(false);
        return prefab;
    }

    public NomaiOrbItem PlaceOrb(string parentPath, Vector3 localPosition, Vector3 localRotation)
    {
        var parent = GameObject.Find(parentPath);
        var orb = Instantiate(orbPrefab, parent.transform, false);
        orb.transform.localPosition = localPosition;
        orb.transform.localEulerAngles = localRotation;
        orb.SetActive(true);
        return orb.GetComponent<NomaiOrbItem>();
    }

    public NomaiOrbSocket PlaceOrbSocket(string parentPath, Vector3 localPosition, Vector3 localRotation)
    {
        var parent = GameObject.Find(parentPath);
        var socketObject = new GameObject("GITM_ORB_SOCKET");
        socketObject.SetActive(false);
        socketObject.transform.SetParent(parent.transform, false);
        socketObject.transform.localPosition = localPosition;
        socketObject.transform.localEulerAngles = localRotation;
        socketObject.layer = LayerMask.NameToLayer("Interactible");

        var collider = socketObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = ORB_COLLIDER_RADIUS;

        var socket = socketObject.AddComponent<NomaiOrbSocket>();
        socket._socketTransform = socketObject.transform;
        socket._interactRange = 3f;

        socketObject.SetActive(true);
        return socket;
    }

    void OnDialogueConditionChanged(string conditionName, bool state)
    {
        if (conditionName != Constants.DialogueConditions.TornadoActivated) return;

        tornado.SetActivated(state);
    }
}
