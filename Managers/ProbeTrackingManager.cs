using GhostInTheMachine.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Managers;

public class ProbeTrackingManager : ManagerBase<ProbeTrackingManager>
{
    const string MODULE_PATH = "GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Interactables_Module_Sunken";
    const string HOLOGRAM_PROJECTOR_PATH = MODULE_PATH + "/HologramProjector";

    // Each computer paired with the lines it shows once the link to the Ash Twin Project is gone
    static readonly (string path, string[] offlineText)[] OFFLINE_DISPLAYS =
    [
        (MODULE_PATH + "/Computers/ComputerPivot (1)/Props_NOM_Computer (1)", ["GITM_OPC_LAUNCHES_1", "GITM_OPC_LAUNCHES_2", "GITM_OPC_LAUNCHES_3"]),
        (MODULE_PATH + "/Computers/ComputerPivot (2)/Props_NOM_Computer (2)", ["GITM_OPC_COORDINATES_1", "GITM_OPC_COORDINATES_2"]),
    ];

    static readonly string[] OFFLINE_HOLOGRAMS = ["Hologram_AllProbeTrajectories", "Hologram_EyeCoordinates"];

    public static bool IsModuleOffline => PlayerData.GetPersistentCondition(STATUE_PROBE);

    readonly List<Display> displays = [];

    OrbitalCannonHologramProjector projector;
    bool appliedOffline;

    protected override void Awake()
    {
        base.Awake();

        foreach (var (path, offlineText) in OFFLINE_DISPLAYS)
        {
            displays.Add(new Display(GameObject.Find(path).GetComponent<NomaiComputer>(), offlineText));
        }
        projector = GameObject.Find(HOLOGRAM_PROJECTOR_PATH).GetComponent<OrbitalCannonHologramProjector>();

        RegisterOfflineText();
        TextTranslation.Get().OnLanguageChanged += RegisterOfflineText;

        SetOffline(IsModuleOffline);
        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
        if (TextTranslation.Get() != null)
        {
            TextTranslation.Get().OnLanguageChanged -= RegisterOfflineText;
        }
    }

    // Handle slot activation for the orb track driving the holograms/computer when the module is offline
    public bool HandleSlotActivated(OrbitalCannonHologramProjector candidate, int index)
    {
        if (!IsModuleOffline || candidate != projector) return false;
        if (index < 0 || index >= projector._holograms.Length) return false;
        if (!OFFLINE_HOLOGRAMS.Contains(projector._holograms[index].name)) return false;

        projector._activeIndex = index;
        if (projector._energyCables.Length > index)
        {
            projector._energyCables[index].SetPowered(true, false);
        }
        if (projector._computers.Length > index)
        {
            projector._computers[index].DisplayAllEntries();
        }
        return true;
    }

    void RegisterOfflineText()
    {
        foreach (var display in displays)
        {
            display.RegisterTranslations();
        }
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (condition != STATUE_PROBE) return;

        SetOffline(state);
    }

    void SetOffline(bool offline)
    {
        if (appliedOffline == offline) return;

        appliedOffline = offline;
        foreach (var display in displays)
        {
            display.SetOffline(offline);
        }
    }

    class Display
    {
        readonly NomaiText text;
        readonly NomaiTextSwapper textSwapper;
        readonly List<NomaiText.NomaiTextConditionData> onlineConditions;

        public Display(NomaiText text, string[] offlineKeys)
        {
            this.text = text;
            textSwapper = new NomaiTextSwapper(text, offlineKeys);
            onlineConditions = [.. text._listDBConditions];
        }

        public void RegisterTranslations() => textSwapper.RegisterTranslations();

        public void SetOffline(bool offline)
        {
            textSwapper.SetReplaced(offline);
            // Prevent vanilla fact reveals if offline
            text._listDBConditions.Clear();
            if (!offline)
            {
                text._listDBConditions.AddRange(onlineConditions);
            }
        }
    }
}
