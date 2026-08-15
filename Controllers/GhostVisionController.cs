using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class GhostVisionController : MonoBehaviour
{
    bool solid;

    public bool IsSolid() => solid;

    public void Start()
    {
        SetIsSolid(false);
    }

    public void SetIsSolid(bool solid)
    {
        this.solid = solid;
        if (this.solid)
        {
            CustomAssetsManager.Instance.ApplySandstoneMaterial(gameObject);
        }
        else
        {
            CustomAssetsManager.Instance.ApplyGhostMaterial(gameObject);
        }
        ToggleCollision(solid);
    }

    void ToggleCollision(bool enable)
    {
        var colliders = GetComponentsInChildren<OWCollider>(true);
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<InteractReceiver>()) continue;
            collider.SetActivation(enable);
        }
    }
}
