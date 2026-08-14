using GhostInTheMachine.Controllers;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class VisionManager : ManagerBase<VisionManager>
{
    const string GABBRO_VISION_PATH = "StatueIsland_Body/Sector_StatueIsland/GITM_VISION_GABBRO";

    protected override void Awake()
    {
        base.Awake();

        // These two are clones of vanilla props rather than our own prefabs, so they don't come with the controller
        GameObject.Find(GABBRO_VISION_PATH).AddComponent<GhostVisionController>();
    }
}
