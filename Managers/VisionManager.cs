using GhostInTheMachine.Controllers;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class VisionManager : ManagerBase<VisionManager>
{
    const string GABBRO_VISION_PATH = "StatueIsland_Body/Sector_StatueIsland/GITM_VISION_GABBRO";

    protected override void Awake()
    {
        base.Awake();

        // Gabbro's vision is a clone of a vanilla prop rather than one of our own prefabs, so it doesn't come with the controller
        GameObject.Find(GABBRO_VISION_PATH).AddComponent<GhostVisionController>();
    }
}
