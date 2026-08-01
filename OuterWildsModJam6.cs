using System.Reflection;
using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;

namespace OuterWildsModJam6
{
    public class OuterWildsModJam6 : ModBehaviour
    {
        public static OuterWildsModJam6 Instance;
        public INewHorizons NewHorizons;

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
        }
    }

}
