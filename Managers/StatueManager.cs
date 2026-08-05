using GhostInTheMachine.Controllers;
using System.Linq;
using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Managers;

public class StatueManager : ManagerBase<StatueManager>
{
    GameObject headPrefab;

    protected override void Awake()
    {
        base.Awake();

        headPrefab = new GameObject("GhostStatue");

        var head = GhostInTheMachine.CloneVanillaProp("TimberHearth_Body/Sector_TH/Sector_Village/Sector_Observatory/Interactables_Observatory/NomaiStatueExhibit/NomaiHeadStatue");
        head.transform.SetParent(headPrefab.transform, false);
        head.transform.localPosition = Vector3.up * 1f;

        var headProp = head.transform.Find("Props_NOM_StatueHead");
        headProp.transform.localPosition = Vector3.zero;
        headProp.transform.localEulerAngles = Vector3.zero;

        var audio = GhostInTheMachine.CloneVanillaProp("TimberHearth_Body/Sector_TH/Sector_Village/Sector_Observatory/Interactables_Observatory/NomaiStatueExhibit/NomaiStatue_Audio");
        audio.transform.SetParent(headPrefab.transform, false);
        audio.transform.localPosition = Vector3.up * 1.5f;

        var mount = GhostInTheMachine.CloneVanillaProp("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken/Structure_NOM_Column_Base_Square");
        mount.transform.SetParent(headPrefab.transform, false);
        mount.transform.localPosition = Vector3.zero;

        var eyeLidAnimators = head.GetComponentsInChildren<TransformAnimator>();
        var lowerLidAnimators = eyeLidAnimators.Where(anim => anim.name.Contains("eyelid_bot")).ToArray();
        var upperLidAnimators = eyeLidAnimators.Where(anim => anim.name.Contains("eyelid_top")).ToArray();

        var eyeRenderer = head.transform.Find("Props_NOM_StatueHead/Statue_Eyes").GetComponent<OWRenderer>();

        var turnTransformAnimator = head.GetComponent<TransformAnimator>();
        var turnAudioSource = audio.GetComponent<OWAudioSource>();

        var visuals = headPrefab.AddComponent<StatueVisualsController>();
        visuals.lowerLidAnimators = lowerLidAnimators;
        visuals.upperLidAnimators = upperLidAnimators;
        visuals.eyeRenderer = eyeRenderer;
        visuals.turnTransformAnimator = turnTransformAnimator;
        visuals.turnAudioSource = turnAudioSource;

        headPrefab.SetActive(false);
    }

    public void CreateGhostStatue(string persistentCondition, string parentPath, Vector3 localPosition, Vector3 localRotation, float localScale, bool hasPedestal, bool canTurn)
    {
        var parent = GameObject.Find(parentPath).transform;
        var statue = Instantiate(headPrefab, parent, false);
        statue.name = persistentCondition;
        statue.transform.localPosition = localPosition;
        statue.transform.localEulerAngles = localRotation;
        statue.transform.localScale = Vector3.one * localScale;
        if (!hasPedestal)
        {
            statue.transform.Find("Structure_NOM_Column_Base_Square").gameObject.SetActive(false);
        }
        var ghostController = statue.AddComponent<StatueGhostController>();
        ghostController.persistentCondition = persistentCondition;
        ghostController.canTurn = canTurn;

        var interactable = new GameObject("InteractReceiver");
        interactable.transform.SetParent(statue.transform, false);
        interactable.transform.localPosition = Vector3.up * 3f * localScale;
        interactable.layer = LayerMask.NameToLayer("Interactible");
        var col = interactable.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f * localScale;
        var receiver = interactable.AddComponent<StatuePromptReceiver>();
        receiver.SetInteractRange(4f * localScale);

        statue.SetActive(true);
    }

    public void PlaceInitialStatues()
    {
        // Gabbro's statue is batched, but no other important props are in the batch, so we disable the whole thing
        GameObject.Find("StatueIsland_Body/Sector_StatueIsland/Props_StatueIsland/BatchedGroup").SetActive(false);
        CreateGhostStatue(STATUE_GABBRO, "StatueIsland_Body", new(-2.9152f, 10.6522f, 7.9023f), new(56.1298f, 90.7213f, 76.5443f), 1f, false, false);

        // Workshop statue is partially batched with other props (batch 4). Batches 2 and 6 can be disabled entirely, while the batch 4 replacement is handled via NH config
        GameObject.Find("StatueIsland_Body/Sector_StatueIsland/Sector_StatueIslandInterior/Props_StatueIslandInterior/BatchedGroup/BatchedMeshRenderers_2").SetActive(false);
        GameObject.Find("StatueIsland_Body/Sector_StatueIsland/Sector_StatueIslandInterior/Props_StatueIslandInterior/BatchedGroup/BatchedMeshRenderers_4").SetActive(false);
        GameObject.Find("StatueIsland_Body/Sector_StatueIsland/Sector_StatueIslandInterior/Props_StatueIslandInterior/BatchedGroup/BatchedMeshRenderers_6").SetActive(false);
        CreateGhostStatue(STATUE_WORKSHOP, "StatueIsland_Body", new(-19.3869f, 3.5316f, 73.9378f), new(359.6512f, 255.228f, 351.7169f), 1.7f, true, true);

        // Probe Tracking Module statue is unbatched, can just disable the original head and pedestal
        GameObject.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken/Structure_NOM_Column_Base_Square").SetActive(false);
        GameObject.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken/Prefab_NOM_StatueHead").SetActive(false);
        CreateGhostStatue(STATUE_PROBE, "GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken", new(10.35f, 8.7401f, 0f), new(0f, 0f, 180f), 1f, true, true);

        // Forge statue is unbatched, can just disable the original head (no pedestal)
        GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge/StatueHead").SetActive(false);
        CreateGhostStatue(STATUE_FORGE, "BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge", new(0.08f, 63.7f -3.75f * 2f), new(0f, 90f, 90f), 1.7f, false, false);

        // Ash Twin Project statue has unbatched renderer but batched collider, just disable the head and pedestal
        GameObject.Find("TimeLoopRing_Body/Props_TimeLoopRing/OtherComponentsGroup/Props_NOM_StatueHead").SetActive(false);
        GameObject.Find("TimeLoopRing_Body/Props_TimeLoopRing/OtherComponentsGroup/Structure_NOM_TallColumn_Base_Square").SetActive(false);
        CreateGhostStatue(STATUE_ATP, "TimeLoopRing_Body/Props_TimeLoopRing/OtherComponentsGroup", new(-0.7f, 12.2047f, 1.4328f), new(0f, 71.593f, 85.5304f), 1f, true, true);

        // Sun Station (upper) Statue has unbatched renderer but batched collider, head and pedestal are one object, so just disable the whole thing
        GameObject.Find("SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup/StatueHead").SetActive(false);
        CreateGhostStatue(STATUE_SS_UPPER, "SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup", new(17.39f, 13.3296f, 28.73f), new(0f, 90f, 0f), 1.6308f, true, true);

        // Sun Station (lower) Statue has unbatched renderer but batched collider, just disable the head and pedestal
        GameObject.Find("SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup/Prefab_NOM_StatueHead").SetActive(false);
        GameObject.Find("SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup/Structure_NOM_ShortColumnBridge").SetActive(false);
        CreateGhostStatue(STATUE_SS_LOWER, "SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup", new(12.968f, -34.5851f, 7.8967f), new(43.8725f, 0f, 270f), 1f, true, true);

    }
}

// Sunken Module Statue Head model
// 10.35 7.7651 0.0001
// 0 0 180

// Sunken Module Statue Mount
// 10.35 8.7401 0
// 0 0 180

// Observatory Statue Head model offset from main transform
// 0.03 -1.357 -0.086
// 0 270.0001 349.9947

/*
Black Hole Forge
1 inside: BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/Props_BlackHoleForge/StatueHead/OtherComponentsGroup/Prefab_NOM_StatueHead (collider not batched, renderers batched together but not with rest of location)
Sun Station
1 in ‘upper floor’: SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup/StatueHead/Prefab_NOM_StatueHead (renderers not batched, collider batched)
1 in ‘lower floor’: SunStation_Body/Sector_SunStation/Sector_ControlModule/Props/OtherComponentsGroup/Prefab_NOM_StatueHead (renderers not batched, collider batched)
Ash Twin Project
1 inside: TimeLoopRing_Body/Props_TimeLoopRing/OtherComponentsGroup/Props_NOM_StatueHead (renderers not batched, collider batched)
Statue Island
2 partial inside, 1 complete inside: StatueIsland_Body/Sector_StatueIsland/Sector_StatueIslandInterior/Props_StatueIslandInterior/BatchedGroup (all batched together with other props, not recoverable)
1 completed outside (active, Gabbro’s): StatueIsland_Body/Sector_StatueIsland/Props_StatueIsland/BatchedGroup (batched, but no other critical props in same batch)
Timber Hearth Observatory
1 inside (active, Hatchling’s): TimberHearth_Body/Sector_TH/Sector_Village/Sector_Observatory/Interactables_Observatory/NomaiStatueExhibit/NomaiHeadStatue/Props_NOM_StatueHead (completely unbatched)
Giant’s Deep core
1 inside (active, Probe Tracking Module): GiantsDeep_Body/Sector_GD/Sector_GDInterior/Sector_GDCore/Sector_Module_Sunken/Props_Module_Sunken/Prefab_NOM_StatueHead (completely unbatched)
*/
