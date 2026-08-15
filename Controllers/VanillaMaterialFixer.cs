using OWML.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class VanillaMaterialFixer : MonoBehaviour
{
    const string DEFAULT_SOURCE_ROOT = "StatueIsland_Body";

    // Only loads materials available under the specified object
    [SerializeField] string sourceRootPath = DEFAULT_SOURCE_ROOT;

    bool applied;

    protected void Awake()
    {
        FixMaterials();
    }

    protected void Start()
    {
        // Try again in case the first attempt failed due to unloaded objects
        if (!FixMaterials())
        {
            GhostInTheMachine.Instance.ModHelper.Console.WriteLine($"{nameof(VanillaMaterialFixer)} on {name} couldn't find '{SourceRootPath}' to take materials from", MessageType.Warning);
        }
        Destroy(this);
    }

    string SourceRootPath => string.IsNullOrEmpty(sourceRootPath) ? DEFAULT_SOURCE_ROOT : sourceRootPath;

    public bool FixMaterials()
    {
        if (applied) return true;

        var sourceRoot = GameObject.Find(SourceRootPath);
        if (sourceRoot == null) return false;
        applied = true;

        var vanillaMaterials = CollectMaterials(sourceRoot);
        var unmatched = new HashSet<string>();
        var swapped = 0;

        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            var changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null) continue;

                var materialName = CleanName(materials[i].name);
                if (!vanillaMaterials.TryGetValue(materialName, out var vanillaMaterial))
                {
                    unmatched.Add(materialName);
                    continue;
                }
                if (materials[i] == vanillaMaterial) continue;

                materials[i] = vanillaMaterial;
                changed = true;
                swapped++;
            }
            if (changed)
            {
                renderer.sharedMaterials = materials;
            }
        }

        if (unmatched.Count > 0)
        {
            GhostInTheMachine.Instance.ModHelper.Console.WriteLine($"{nameof(VanillaMaterialFixer)} on {name} found no match in '{SourceRootPath}' for: {string.Join(", ", unmatched.OrderBy(materialName => materialName))}", MessageType.Warning);
        }
        if (GhostInTheMachine.Instance.DebugModeEnabled)
        {
            GhostInTheMachine.Instance.ModHelper.Console.WriteLine($"{nameof(VanillaMaterialFixer)} on {name} swapped {swapped} material slot(s) from '{SourceRootPath}'");
        }
        return true;
    }

    static Dictionary<string, Material> CollectMaterials(GameObject sourceRoot)
    {
        // Sectors are usually streamed out at this point, so this has to reach inactive renderers too
        var materials = new Dictionary<string, Material>();
        foreach (var renderer in sourceRoot.GetComponentsInChildren<Renderer>(true))
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material == null) continue;

                var materialName = CleanName(material.name);
                if (!materials.ContainsKey(materialName))
                {
                    materials[materialName] = material;
                }
            }
        }
        return materials;
    }

    static string CleanName(string materialName)
    {
        // Unity tacks this on the moment anything touches Renderer.material instead of sharedMaterial
        return materialName.Replace(" (Instance)", "").Trim();
    }
}
