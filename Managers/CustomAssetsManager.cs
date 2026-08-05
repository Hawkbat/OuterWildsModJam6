using UnityEngine;

namespace GhostInTheMachine.Managers;

public class CustomAssetsManager : ManagerBase<CustomAssetsManager>
{
    [SerializeField] Material ghostMaterial;
    [SerializeField] Material sandstoneMaterial;
    [SerializeField] GameObject spawnedWallPrefab;

    public Material GhostMaterial => ghostMaterial;
    public Material SandstoneMaterial => sandstoneMaterial;
    public GameObject SpawnedWallPrefab => spawnedWallPrefab;

    protected override void Awake()
    {
        base.Awake();
        transform.parent = null;
    }

    public void ApplyGhostMaterial(GameObject obj)
    {
        if (ghostMaterial == null) return;
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = ghostMaterial;
        }
    }

    public void ApplySandstoneMaterial(GameObject obj)
    {
        if (sandstoneMaterial == null) return;
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = sandstoneMaterial;
        }
    }
}
