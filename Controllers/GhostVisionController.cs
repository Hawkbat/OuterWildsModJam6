using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class GhostVisionController : MonoBehaviour
{
    bool solid;

    public bool IsSolid() => solid;

    public void Start()
    {
        CustomAssetsManager.Instance.ApplyGhostMaterial(gameObject);
        ToggleCollision(false);
        solid = false;
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
        ToggleCollision(!solid);
    }

    void ToggleCollision(bool enable)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.isTrigger = !enable;
        }
    }
}
