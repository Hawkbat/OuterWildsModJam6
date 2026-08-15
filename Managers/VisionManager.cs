using GhostInTheMachine.Controllers;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class VisionManager : ManagerBase<VisionManager>
{
    const string GABBRO_VISION_PATH = "StatueIsland_Body/Sector_StatueIsland/GITM_VISION_GABBRO";

    protected override void Awake()
    {
        base.Awake();

        // Cloned from vanilla prop, so doesn't have the vision controller like the other prefabs
        GameObject.Find(GABBRO_VISION_PATH).AddComponent<GhostVisionController>();
    }
}
