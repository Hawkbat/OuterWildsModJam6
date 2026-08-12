
namespace GhostInTheMachine;

public static class Constants
{
    public static class ShipLogFacts
    {
        public const string WallToolUnlock = "GITM_WALL_TOOL_UNLOCK";
    }

    public static class DialogueConditions
    {
        public const string StatueInstalledThisLoop = "GITM_StatueInstalledThisLoop";
        public const string GameOver = "GITM_GameOver";
        public const string TerribleFate = "GITM_TerribleFate";
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
        public const string SOLANUM_MASK_FIX = "GITM_SOLANUM_MASK_FIX";
        public const string MASK_INSTALLED = "GITM_MASK_INSTALLED";

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
