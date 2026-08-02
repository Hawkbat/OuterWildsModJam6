using System.Reflection;
using HarmonyLib;
using GhostInTheMachine.Managers;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;

namespace GhostInTheMachine;

public class GhostInTheMachine : ModBehaviour
{
    public static GhostInTheMachine Instance;
    public static INewHorizons NewHorizons;

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

        OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen);
        LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
    }

    public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
    {
        if (newScene != OWScene.SolarSystem) return;
        ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);


        ModHelper.Events.Unity.FireInNUpdates(() =>
        {
            InvincibilityManager.EnsureInstance();
            FastForwardManager.EnsureInstance();
            ShipLogDialogueManager.EnsureInstance();
            StatueManager.EnsureInstance();
            SpawnManager.EnsureInstance();
            TornadoManager.EnsureInstance();

            StatueManager.Instance.PlaceInitialStatues();
            SpawnManager.Instance.DoInitialSpawn();
        }, 1);
    }
}
