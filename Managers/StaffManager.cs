using GhostInTheMachine.Controllers;
using OWML.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class StaffManager : ManagerBase<StaffManager>
{
    static readonly string[] VANILLA_STAFF_PATHS = [
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_NorthPoleSurface/Props_NorthPoleSurface/OtherComponentsGroup/LowBuilding/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge/Prefab_NOM_Staff (2)",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District1/Props_HangingCity_District1/OtherComponentsGroup/Props_HangingCity_SchoolBuilding/Prefab_NOM_Staff (1)",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District3/Props_HangingCity_District3/OtherComponentsGroup/EyeShrine_Buildings/Building_6_Props/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District3/Props_HangingCity_District3/OtherComponentsGroup/EyeShrine_Buildings/Building_1_Props/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District3/Props_HangingCity_District3/OtherComponentsGroup/EyeShrine_Buildings/Building_2_Props/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District3/Props_HangingCity_District3/OtherComponentsGroup/EyeShrine_Buildings/EyeShrine_Props/Prefab_NOM_Staff",
        "CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_SouthUnderground/Sector_City/Sector_Forum/Props_Forum/OtherComponentsGroup/Interior_Forum/Prefab_NOM_Staff",
        "CaveTwin_Body/Sector_CaveTwin/Sector_SouthHemisphere/Sector_SouthUnderground/Sector_City/Sector_EyeDistrict/Props_EyeDistrict/OtherComponentsGroup/Props_SideBuilding/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_OldSettlement/Fragment OldSettlement 0/Core_OldSettlement0/Props_Core_OldSettlement0/Prefab_NOM_Staff",
        "BrittleHollow_Body/Sector_BH/Sector_GravityCannon/Props_GravityCannon/OtherComponentsGroup/Prefab_NOM_Staff",
    ];

    static readonly string[] VANILLA_STAFF_PARENT_PATHS = [
        "SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup/ARTPASS_Props_ControlINT",
    ];

    static readonly CustomStaffPlacement[] CUSTOM_STAFFS = [
        new("StatueIsland_Body/Sector_StatueIsland/Tornado Lab", new(35.45065f, 17.36366f, -47.93872f), new(0f, 330f, 0f)),
        new("StatueIsland_Body/Sector_StatueIsland", new(-8.466702f, 33.74698f, 84.35114f), new(355f, 204f, 351f)),
        new("GiantsDeep_Body/Sector_GD", new(-35.05562f, -72.41138f, -46.42143f), new(333.6703f, 53.55551f, 80.81622f)),
        new("CaveTwin_Body/Sector_CaveTwin", new(-99f, -109f, -24f), new(345f, 180f, 221f)),
        new("CaveTwin_Body/Sector_CaveTwin", new(14.67846f, -106.6276f, 47.13413f), new(19f,204f, 184f)),
        new("TimberHearth_Body/Sector_TH", new(11.85f, -44.5f, 185f), new(3f, 94f, 103f), Constants.PersistentConditions.MASK_INSTALLED),
    ];

    public const float WALL_LIFETIME = 60f;
    public const int MAX_ACTIVE_WALLS = 30;

    static ItemType itemType;
    static string itemName;

    public static ItemType ItemType => itemType;
    public static string ItemName => itemName;

    GameObject staffPrefab;
    NomaiStaffItem staffToGive;

    List<SpawnedWallData> activeWalls = [];
    Stack<SpawnedWallController> wallPool = [];

    protected override void Awake()
    {
        base.Awake();

        if (itemType == ItemType.Invalid)
        {
            itemType = EnumUtils.Create<ItemType>(nameof(NomaiStaffItem));
        }
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = GhostInTheMachine.NewHorizons.GetTranslationForUI(nameof(NomaiStaffItem));
        }

        staffPrefab = GhostInTheMachine.CloneVanillaProp("BrittleHollow_Body/Sector_BH/Sector_OldSettlement/Fragment OldSettlement 0/Core_OldSettlement0/Props_Core_OldSettlement0/Prefab_NOM_Staff");
        staffPrefab.AddComponent<NomaiStaffItem>();
        staffPrefab.SetActive(false);

        staffToGive = Instantiate(staffPrefab).GetComponent<NomaiStaffItem>();
    }

    protected void Update()
    {
        List<SpawnedWallData> wallsToRemove = null;
        if (activeWalls.Count > MAX_ACTIVE_WALLS)
        {
            var excessWalls = activeWalls.Count - MAX_ACTIVE_WALLS;
            for (int i = 0; i < excessWalls; i++)
            {
                var data = activeWalls[i];
                if (data.wall != null && data.wall.IsIdle)
                {
                    data.wall.Shrink();
                }
            }
        }
        foreach (var data in activeWalls)
        {
            if (data.wall == null)
            {
                wallsToRemove ??= [];
                wallsToRemove.Add(data);
            }
            else if (data.wall.IsShrunk)
            {
                data.wall.gameObject.SetActive(false);
                wallPool.Push(data.wall);
                wallsToRemove ??= [];
                wallsToRemove.Add(data);
            }
            else if (Time.time - data.spawnTime > WALL_LIFETIME && !data.wall.IsShrinking)
            {
                data.wall.Shrink();
            }
        }
        if (wallsToRemove != null)
        {
            foreach (var wall in wallsToRemove)
            {
                activeWalls.Remove(wall);
            }
        }
    }

    public NomaiStaffItem GivePlayerStaff()
    {
        staffToGive.gameObject.SetActive(true);
        Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(staffToGive);
        return staffToGive;
    }

    public void PlaceInitialStaffs()
    {
        var staffsToReplace = new List<Transform>();
        staffsToReplace.AddRange(VANILLA_STAFF_PATHS.Select(path => GameObject.Find(path).transform));
        staffsToReplace.AddRange(VANILLA_STAFF_PARENT_PATHS.SelectMany(path => GameObject.Find(path).transform.Cast<Transform>().Where(child => child.name.StartsWith("Prefab_NOM_Staff"))));
        foreach (var oldStaff in staffsToReplace)
        {
            var newStaff = Instantiate(staffPrefab);
            newStaff.transform.SetParent(oldStaff.transform.parent, false);
            newStaff.transform.localPosition = oldStaff.localPosition;
            newStaff.transform.localEulerAngles = oldStaff.localEulerAngles;
            newStaff.transform.localScale = oldStaff.localScale * 1.02f; // Slightly larger to give bigger hitbox for player to pick up
            newStaff.SetActive(true);
            oldStaff.gameObject.SetActive(false);
        }

        foreach (var loc in CUSTOM_STAFFS)
        {
            if (!string.IsNullOrEmpty(loc.requiredPersistentCondition))
            {
                if (!PlayerData.GetPersistentCondition(loc.requiredPersistentCondition))
                {
                    continue;
                }
            }
            var parent = GameObject.Find(loc.parentPath).transform;
            var staff = Instantiate(staffPrefab);
            staff.transform.SetParent(parent, false);
            staff.transform.position = parent.root.TransformPoint(loc.bodyPos);
            staff.transform.rotation = parent.root.rotation * Quaternion.Euler(loc.bodyRot);
            staff.SetActive(true);
        }
    }

    public SpawnedWallController SpawnWall(string parentPath, Vector3 localPosition, Vector3 localRotation)
    {
        var parent = GameObject.Find(parentPath).transform;

        var wall = wallPool.Count > 0 ? wallPool.Pop().gameObject : Instantiate(CustomAssetsManager.Instance.SpawnedWallPrefab, parent, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localEulerAngles = localRotation;
        wall.SetActive(true);
        var controller = wall.GetAddComponent<SpawnedWallController>();

        activeWalls.Add(new()
        {
            wall = controller,
            spawnTime = Time.time
        });

        AchievementManager.Instance.OnWallSpawned(parentPath, activeWalls.Count);

        return controller;
    }

    class SpawnedWallData
    {
        public SpawnedWallController wall;
        public float spawnTime;
    }

    class CustomStaffPlacement(string parentPath, Vector3 pos, Vector3 rot, string requiredPersistentCondition = null)
    {
        public string parentPath = parentPath;
        public Vector3 bodyPos = pos;
        public Vector3 bodyRot = rot;
        public string requiredPersistentCondition = requiredPersistentCondition;
    }
}
