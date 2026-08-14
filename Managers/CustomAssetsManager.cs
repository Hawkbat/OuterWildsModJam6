using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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
            renderer.sharedMaterials = [.. renderer.sharedMaterials.Select(m => ghostMaterial)];
        }
    }

    public void ApplySandstoneMaterial(GameObject obj)
    {
        if (sandstoneMaterial == null) return;
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.sharedMaterials = [.. renderer.sharedMaterials.Select(m =>  sandstoneMaterial)];
        }
    }

    public static void AnalyzeShader(Shader shader) => AnalyzeShader(shader, null);
    public static void AnalyzeShader(Material material) => AnalyzeShader(material.shader, material);
    public static void AnalyzeShader(Shader shader, Material material)
    {
        if (!shader)
        {
            Log("No shader provided.");
            return;
        }
        Log($"Analyzing shader: {shader.name}");
        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            var propName = shader.GetPropertyName(i);
            var propType = shader.GetPropertyType(i);
            var propDesc = shader.GetPropertyDescription(i);
            switch (propType)
            {
                case ShaderPropertyType.Color:
                    Log($"Property {i}: {propName} (Color) - {propDesc}");
                    if (material != null)
                    {
                        var colorValue = material.GetColor(propName);
                        Log($"  Value: {colorValue}");
                    }
                    break;
                case ShaderPropertyType.Vector:
                    Log($"Property {i}: {propName} (Vector) - {propDesc}");
                    if (material != null)
                    {
                        var vectorValue = material.GetVector(propName);
                        Log($"  Value: {vectorValue}");
                    }
                    break;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    Log($"Property {i}: {propName} (Float/Range) - {propDesc}");
                    if (material != null)
                    {
                        var floatValue = material.GetFloat(propName);
                        Log($"  Value: {floatValue}");
                    }
                    break;
                case ShaderPropertyType.Texture:
                    Log($"Property {i}: {propName} (Texture) - {propDesc}");
                    if (material != null)
                    {
                        var textureValue = material.GetTexture(propName);
                        Log($"  Value: {(textureValue != null ? textureValue.name : "null")}");
                    }
                    break;
                default:
                    Log($"Property {i}: {propName} (Unknown Type) - {propDesc}");
                    break;
            }
        }
    }

    static void Log(string msg)
    {
        GhostInTheMachine.Instance.ModHelper.Console.WriteLine(msg);
    }
}
