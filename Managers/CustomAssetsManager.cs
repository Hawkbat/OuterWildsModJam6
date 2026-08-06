using UnityEngine;

namespace GhostInTheMachine.Managers;

public class CustomAssetsManager : ManagerBase<CustomAssetsManager>
{
    [SerializeField] Material ghostMaterial;
    [SerializeField] Material ghostUIMaterial;
    [SerializeField] Material sandstoneMaterial;
    [SerializeField] GameObject spawnedWallPrefab;

    public Material GhostMaterial => ghostMaterial;
    public Material GhostUIMaterial => ghostUIMaterial;
    public Material SandstoneMaterial => sandstoneMaterial;
    public GameObject SpawnedWallPrefab => spawnedWallPrefab;

    SurfaceManager surfaceManager;

    protected override void Awake()
    {
        base.Awake();
        transform.parent = null;
    }

    protected void Start()
    {
        surfaceManager = FindObjectOfType<SurfaceManager>();
        surfaceManager._lookupTable.Add(sandstoneMaterial, SurfaceType.Stone);
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
