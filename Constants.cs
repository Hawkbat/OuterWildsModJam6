
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

        // Rumors an act gate in systems/SolarSystem.json owns. Reading the card they point back to must not
        // hand them over early, which it otherwise would for any rumor hanging off one of Lami's entries or
        // one of the player's replies. Keep this in sync with the conditionalChecks that reveal rumor facts
        // Solanum has to have reworked the mask before it will do anything in the Ash Twin Project
        public const string SolanumAnswer = "GITM_SOLANUM_ANSWER";

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
}
