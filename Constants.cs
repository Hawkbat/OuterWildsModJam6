
namespace GhostInTheMachine;

public static class Constants
{
    public static class ShipLogFacts
    {
        public const string WallToolUnlock = "GITM_WALL_TOOL_UNLOCK";
        public const string DoorToolUnlock = "GITM_DOOR_TOOL_UNLOCK";
        public const string BeamToolUnlock = "GITM_BEAM_TOOL_UNLOCK";
        // Revealed by picking up Lami's mask in the storeroom, which is what opens the Solanum thread
        public const string MaskAcquired = "GITM_FIND_MASK_REVEAL";

        // Solanum has to rework the mask before it does anything in the ATP
        public const string SolanumAnswer = "GITM_SOLANUM_ANSWER";

        // Rumors owned by an act gate in systems/SolarSystem.json; keep in sync with the conditionalChecks that reveal them
        public static readonly string[] GATED_RUMORS =
        [
            "GITM_GHOST_CLIMB_HINT_RUMOR",
            "GITM_CHOICE_MORE_RUMOR",
            "GITM_GHOST_LAEVI_RUMOR",
            "GITM_GHOST_AUNT_PYE_RUMOR",
            "GITM_GHOST_MASK_RUMOR",
            "GITM_GHOST_SOLANUM_RUMOR",
            "GITM_VISION_GABBRO_LISTED",
            "GITM_VISION_WORKSHOP_LISTED",
            "GITM_VISION_PROBE_LISTED",
            "GITM_VISION_FORGE_LISTED",
            "GITM_VISION_ATP_LISTED",
            "GITM_VISION_SS_UPPER_LISTED",
            "GITM_VISION_SS_LOWER_LISTED"
        ];
    }

    public static class DialogueConditions
    {
        public const string StatueInstalledThisLoop = "GITM_StatueInstalledThisLoop";
        public const string GameOver = "GITM_GameOver";
        public const string TerribleFate = "GITM_TerribleFate";
        public const string ErnestoWarned = "GITM_ErnestoWarned";
        public const string ErnestoWarnedTwice = "GITM_ErnestoWarnedTwice";
        public const string ErnestoUndo = "GITM_ErnestoUndo";
        public const string ErnestoRockDeath = "GITM_ErnestoRockDeath";
        public const string TornadoActivated = "GITM_TornadoActivated";
    }

    public static class PersistentConditions
    {
        public const string STATUE_GABBRO = "GITM_STATUE_GABBRO";
        public const string STATUE_WORKSHOP = "GITM_STATUE_WORKSHOP";
        public const string STATUE_PROBE = "GITM_STATUE_PROBE";
        public const string STATUE_FORGE = "GITM_STATUE_FORGE";
        public const string STATUE_ATP = "GITM_STATUE_ATP";
        public const string STATUE_SS_UPPER = "GITM_STATUE_SS_UPPER";
        public const string STATUE_SS_LOWER = "GITM_STATUE_SS_LOWER";
        public const string STATUE_PLAYER = "GITM_STATUE_PLAYER";
        public const string MASK_INSTALLED = "GITM_MASK_INSTALLED";
        public const string DEV_THANKS_SEEN = "GITM_DEV_THANKS_SEEN";
        public const string WALL_PLACED = "GITM_WALL_PLACED";

        public static readonly string[] ALL_STATUE_CONDITIONS =
        [
            STATUE_GABBRO,
            STATUE_WORKSHOP,
            STATUE_PROBE,
            STATUE_FORGE,
            STATUE_ATP,
            STATUE_SS_UPPER,
            STATUE_SS_LOWER,
            STATUE_PLAYER
        ];
    }

    // Vanilla conditions a fresh save profile lacks; the mod opens partway through a run the player is meant to have already been on
    public static class VanillaConditions
    {
        public const string LAUNCH_CODES_GIVEN = "LAUNCH_CODES_GIVEN";

        // Things the player character is supposed to have done before the mod's story begins
        public static readonly string[] PROGRESSED_SAVE_CONDITIONS =
        [
            LAUNCH_CODES_GIVEN,
            "HAS_SEEN_SUN_EXPLODE",
            "KILLED_BY_SUPERNOVA_AND_KNOWS_IT",
            "TALKED_TO_GABBRO",
            "GABBRO_MERGE_TRIGGERED",
            "KNOWS_MEDITATION",
            "COMPLETED_SHIPLOG_TUTORIAL",
            "HAS_USED_SHIPLOG",
            "PREFLIGHT_CHECKLIST_UNLOCKED",
            "HAS_USED_PREFLIGHT_CHECKLIST",
            "HAS_USED_JETPACK",
            "SUIT_BOOSTER_FIRED",
            "HAS_USED_MAP_SUIT",
            "HAS_USED_MAP_SHIP",
            "HAS_PLAYER_LOCKED_ON",
            "HAS_PLAYER_LOCKED_ON_MAP",
            "HAS_AIMED_TRANSLATOR",
            "HAS_USED_TRANSLATOR",
            "MARK_ON_HUD_TUTORIAL_COMPLETE"
        ];

        public static bool IsFreshSave() => PlayerData.LoadLoopCount() == 1 && !PlayerData.KnowsLaunchCodes();
    }
}
