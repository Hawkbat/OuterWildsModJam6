using GhostInTheMachine.Managers;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Reflection;
using UnityEngine;

namespace GhostInTheMachine;

public class GhostInTheMachine : ModBehaviour
{
    public static GhostInTheMachine Instance;
    public static INewHorizons NewHorizons;

    public bool DebugModeEnabled => debugModeEnabled;

    bool debugModeEnabled;

    public static GameObject CloneVanillaProp(string path)
    {
        return NewHorizons.SpawnObject(Instance, null, null, path, Vector3.zero, Vector3.zero, 1f, false);
    }

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
        NewHorizons.LoadConfigs(this);

        new Harmony(ModHelper.Manifest.UniqueName).PatchAll(Assembly.GetExecutingAssembly());

        debugModeEnabled = ModHelper.Config.GetSettingsValue<bool>("debugMode");

        OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
        LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
    }

    public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
    {
        if (newScene != OWScene.SolarSystem) return;
        ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);

        ModHelper.Events.Unity.FireInNUpdates(() =>
        {
            // Reset the bad ending condition so the player doesn't get repeated game-overs
            if (PlayerData.GetPersistentCondition(Constants.PersistentConditions.STATUE_PLAYER))
            {
                PlayerData.SetPersistentCondition(Constants.PersistentConditions.STATUE_PLAYER, false);
            }

            ModCompatManager.Initialize();
            InvincibilityManager.Initialize();
            FastForwardManager.Initialize();
            ShipLogDialogueManager.Initialize();
            ErnestoManager.Initialize();
            StatueManager.Initialize();
            LoopPulseManager.Initialize();
            MaskManager.Initialize();
            VisionManager.Initialize();
            DoorManager.Initialize();
            SpawnManager.Initialize();
            TornadoManager.Initialize();
            TractorBeamManager.Initialize();
            SolanumManager.Initialize();
            ProbeTrackingManager.Initialize();
            AchievementManager.Initialize();

            if (debugModeEnabled) DebugManager.Initialize();

            StatueManager.Instance.PlaceInitialStatues();
            AchievementManager.Instance.PlaceStatueVolumes();
            StaffManager.Instance.PlaceInitialStaffs();
            DoorManager.Instance.PlaceDoorInteractions();
            TractorBeamManager.Instance.PlaceBeamInteractions();
            TornadoManager.Instance.PlaceOrbAndSocket();
            SpawnManager.Instance.DoInitialSpawn();
        }, 1);
    }

    public override void Configure(IModConfig config)
    {
        debugModeEnabled = config.GetSettingsValue<bool>("debugMode");
        if (debugModeEnabled && LoadManager.GetCurrentScene() == OWScene.SolarSystem)
        {
            DebugManager.Initialize();
        }
    }
}
