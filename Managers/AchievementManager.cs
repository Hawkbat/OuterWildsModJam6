using GhostInTheMachine.Controllers;
using GhostInTheMachine.Patches;
using OWML.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Managers;

public class AchievementManager : ManagerBase<AchievementManager>
{
    public const string GHOST_IN_THE_MACHINE = "GITM_GHOST_IN_THE_MACHINE";
    public const string TERRIBLE_FATE = "GITM_TERRIBLE_FATE";
    public const string THEY_COOKED = "GITM_THEY_COOKED";
    public const string VERTICALLY_CHALLENGED = "GITM_VERTICALLY_CHALLENGED";
    public const string ECO_FRIENDLY = "GITM_ECO_FRIENDLY";
    public const string TIME_BUDDY_AMNESIA = "GITM_TIME_BUDDY_AMNESIA";
    public const string AN_ORB_TOO_LARGE = "GITM_AN_ORB_TOO_LARGE";
    public const string CONSTRUCTION_KIT = "GITM_CONSTRUCTION_KIT";
    public const string FRESH_START = "GITM_FRESH_START";
    public const string TOURIST = "GITM_TOURIST";

    const string STOREROOM_PATH = "StatueIsland_Body/Sector_StatueIsland/StoreroomGuantlet";
    const string STORAGE_DOOR_NAME = "GITM_STORAGE_DOOR";
    const string STORAGE_BEAM_NAME = "GITM_STORAGE_BEAM";
    const string STATUE_ISLAND_ROOT = "StatueIsland_Body";

    const float STOREROOM_DOOR_RANGE = 2.5f;

    const float STATUE_VISIT_RANGE = 30f;

    IAchievements api;

    bool usedSunStationWarp;
    bool forfeitedClimb;
    readonly HashSet<string> visitedStatues = [];
    readonly HashSet<string> earnedThisLoop = [];

    NomaiWarpReceiver sunStationWarp;
    CharacterDialogueTree gabbroDialogue;
    PlayerTriggerVolume storageDoorVolume;

    protected override void Awake()
    {
        base.Awake();

        api = GhostInTheMachine.Instance.ModHelper.Interaction.TryGetModApi<IAchievements>("xen.AchievementTracker");
        if (api == null)
        {
            // Achievements+ is an optional dependency, so quietly do nothing without it
            return;
        }

        var storeroom = GameObject.Find(STOREROOM_PATH);
        var storageDoor = storeroom != null ? storeroom.transform.Find(STORAGE_DOOR_NAME) : null;
        if (storageDoor != null)
        {
            storageDoorVolume = PlayerTriggerVolume.Create("GITM_STOREROOM_DOOR_VOLUME", storageDoor, storageDoor.position, STOREROOM_DOOR_RANGE);
            storageDoorVolume.OnPlayerEnter.AddListener(OnReachStorageDoor);
        }

        sunStationWarp = Locator.GetWarpReceiver(NomaiWarpPlatform.Frequency.SunStation);
        if (sunStationWarp != null)
        {
            sunStationWarp.OnReceiveWarpedBody += OnReceiveWarpedBody;
        }

        // Gabbro's conversation zone starts switched off, so go through the swapper that owns it
        gabbroDialogue = Resources.FindObjectsOfTypeAll<GabbroDialogueSwapper>().FirstOrDefault()?._dialogueTree;
        if (gabbroDialogue != null)
        {
            gabbroDialogue.OnStartConversation += OnStartGabbroConversation;
        }

        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        if (api == null) return;

        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
        if (sunStationWarp != null)
        {
            sunStationWarp.OnReceiveWarpedBody -= OnReceiveWarpedBody;
        }
        if (gabbroDialogue != null)
        {
            gabbroDialogue.OnStartConversation -= OnStartGabbroConversation;
        }
        if (storageDoorVolume != null)
        {
            storageDoorVolume.OnPlayerEnter.RemoveListener(OnReachStorageDoor);
        }
    }

    public void OnBeamCycled(GhostBeamController beam)
    {
        if (beam.name == STORAGE_BEAM_NAME)
        {
            // Riding the beam up is the intended route, so touching it forfeits the climb for this loop
            forfeitedClimb = true;
        }
    }

    public void OnWallSpawned(string parentPath, int activeWallCount)
    {
        if (parentPath == STATUE_ISLAND_ROOT)
        {
            forfeitedClimb = true;
        }
        if (activeWallCount >= StaffManager.MAX_ACTIVE_WALLS)
        {
            Earn(CONSTRUCTION_KIT);
        }
    }

    public void PlaceStatueVolumes()
    {
        if (api == null) return;

        foreach (var statue in FindObjectsOfType<StatueGhostController>())
        {
            var condition = statue.persistentCondition;
            // Parented to the site rather than the statue so the statue's scale can't stretch the trigger
            PlayerTriggerVolume.Create($"{condition}_VISIT_VOLUME", statue.transform.parent, statue.transform.position, STATUE_VISIT_RANGE)
                .OnPlayerEnter.AddListener(() => OnStatueSiteEntered(condition));
        }
    }

    public void OnDeathSequenceFinished()
    {
        var conditions = DialogueConditionManager.SharedInstance;

        if (conditions.GetConditionState(Constants.DialogueConditions.GameOver))
        {
            Earn(GHOST_IN_THE_MACHINE);
        }
        // The rock ending kills the player as well, so don't double up
        if (conditions.GetConditionState(Constants.DialogueConditions.TerribleFate)
            && !conditions.GetConditionState(Constants.DialogueConditions.ErnestoRockDeath))
        {
            Earn(TERRIBLE_FATE);
        }
    }

    void OnStatueSiteEntered(string persistentCondition)
    {
        if (!visitedStatues.Add(persistentCondition)) return;

        GhostInTheMachine.Instance.ModHelper.Console.WriteLine($"Visited memory statue {persistentCondition} ({visitedStatues.Count}/{ALL_STATUE_CONDITIONS.Length} this loop)", MessageType.Info);

        if (visitedStatues.Count == ALL_STATUE_CONDITIONS.Length)
        {
            Earn(TOURIST);
        }
    }

    public void OnComputerTranslated(NomaiComputer computer)
    {
        if (ProbeTrackingManager.IsModuleOffline && ProbeTrackingManager.Instance.IsOfflineDisplay(computer))
        {
            Earn(FRESH_START);
        }
    }

    void OnReachStorageDoor()
    {
        if (!forfeitedClimb)
        {
            Earn(VERTICALLY_CHALLENGED);
        }
        if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == NomaiOrbItem.ItemType)
        {
            Earn(AN_ORB_TOO_LARGE);
        }
    }

    void OnReceiveWarpedBody(OWRigidbody warpedBody, NomaiWarpPlatform startPlatform, NomaiWarpPlatform targetPlatform)
    {
        if (warpedBody.CompareTag("Player"))
        {
            usedSunStationWarp = true;
        }
    }

    void OnStartGabbroConversation()
    {
        if (GabbroDialogueSwapperPatches.ForgotTheLoop)
        {
            Earn(TIME_BUDDY_AMNESIA);
        }
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (!state) return;

        if (!usedSunStationWarp && (condition == STATUE_SS_UPPER || condition == STATUE_SS_LOWER))
        {
            Earn(THEY_COOKED);
        }
        if (ALL_STATUE_CONDITIONS.All(PlayerData.GetPersistentCondition))
        {
            Earn(ECO_FRIENDLY);
        }
    }

    void Earn(string achievementID)
    {
        if (api == null || !earnedThisLoop.Add(achievementID)) return;

        api.EarnAchievement(achievementID);
    }
}
