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

    public void CreateGhostStatue(string persistentCondition, string parentPath, Vector3 localPosition, Vector3 localRotation, bool hasPedestal, bool canTurn)
    {
        var parent = GameObject.Find(parentPath).transform;
        var statue = Instantiate(headPrefab, parent, false);
        statue.name = persistentCondition;
        statue.transform.localPosition = localPosition;
        statue.transform.localEulerAngles = localRotation;
        if (!hasPedestal)
        {
            statue.transform.Find("Structure_NOM_Column_Base_Square").gameObject.SetActive(false);
        }
        var ghostController = statue.AddComponent<StatueGhostController>();
        ghostController.persistentCondition = persistentCondition;
        ghostController.canTurn = canTurn;

        var interactable = new GameObject("InteractReceiver");
        interactable.transform.SetParent(statue.transform, false);
        interactable.transform.localPosition = Vector3.up * 3f;
        interactable.layer = LayerMask.NameToLayer("Interactible");
        var col = interactable.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;
        var receiver = interactable.AddComponent<StatuePromptReceiver>();
        receiver.SetInteractRange(4f);

        statue.SetActive(true);
    }

    public void PlaceInitialStatues()
    {
        // Gabbro's statue is batched, but no other important props are in the batch, so we disable the whole thing
        GameObject.Find("StatueIsland_Body/Sector_StatueIsland/Props_StatueIsland/BatchedGroup").SetActive(false);
        CreateGhostStatue(STATUE_GABBRO, "StatueIsland_Body", new(-2.9152f, 10.6522f, 7.9023f), new(56.1298f, 90.7213f, 76.5443f), false, false);
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