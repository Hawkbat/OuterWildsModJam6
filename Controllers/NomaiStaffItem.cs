using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiStaffItem : OWItem
{
    string translatedName;
    SurfaceType targetSurfaceType;

    ScreenPrompt firePrompt;
    ScreenPrompt altFirePrompt;
    ScreenPrompt cancelPrompt;

    public override string GetDisplayName() => translatedName;

    public override bool CheckIsDroppable() => true;

    public override void Awake()
    {
        base.Awake();

        _type = StaffManager.ItemType;
        translatedName = StaffManager.ItemName;
        _interactable = true;
        _interactRange = 2f;
        _localDropOffset = new Vector3(0f, -0.1f, 0f);
        _localDropNormal = Vector3.up;
    }

    protected void OnEnable()
    {
        UpdatePromptVisibility();
    }

    protected void OnDisable()
    {
        UpdatePromptVisibility();
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCorePickUp);
        transform.localPosition = new Vector3(0.25f, -1.25f, 0.25f);
        transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        enabled = true;
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        _localDropNormal = Vector3.Lerp(Vector3.up, Random.onUnitSphere, 0.1f).normalized;
        base.DropItem(position, normal, parent, sector, customDropTarget);
        Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCoreDrop);
        enabled = false;
    }

    protected void Update()
    {
        var inToolMode = Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item) && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() == this;
        var fireInput = inToolMode && (OWInput.IsNewlyPressed(InputLibrary.lockOn, InputMode.Character) || OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.Character));
        var altFireInput = inToolMode && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.Character);
        var cancelInput = inToolMode && OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Character);

        var player = Locator.GetPlayerTransform();
        var cam = Locator.GetPlayerCamera().transform;

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 75f, OWLayerMask.blockableInteractMask))
        {
            targetSurfaceType = Locator.GetSurfaceManager().GetHitSurfaceType(hit);
            if (fireInput || altFireInput)
            {
                var targetWall = hit.collider.GetComponentInParent<SpawnedWallController>();

                if (fireInput)
                {
                    if (!targetWall && (targetSurfaceType == SurfaceType.Ceramic || targetSurfaceType == SurfaceType.Stone))
                    {
                        var wallParent = hit.transform.root;
                        var wallWorldPos = hit.point;

                        // If placed on a 'wall', 'up' is perpendicular to the wall and 'forward' is the player's up direction, otherwise 'up' is the player's up direction and 'forward' is the camera's forward direction projected onto the floor plane.

                        var isVerticalSurface = Mathf.Abs(Vector3.Dot(hit.normal, player.up)) < 0.5f;
                        var wallUp = isVerticalSurface ? hit.normal : player.up;
                        var wallForward = isVerticalSurface ? player.up : Vector3.ProjectOnPlane(cam.forward, hit.normal).normalized;
                        var wallWorldRot = Quaternion.LookRotation(wallForward, wallUp);

                        var wallLocalPos = wallParent.InverseTransformPoint(wallWorldPos);
                        var wallLocalRot = wallParent.InverseTransformRotation(wallWorldRot);

                        var wall = StaffManager.Instance.SpawnWall(wallParent.name, wallLocalPos, wallLocalRot.eulerAngles);
                        wall.Grow();
                    }
                }
                else if (altFireInput)
                {
                    if (targetWall != null)
                    {
                        targetWall.Shrink();
                    }
                }
            }
        }
        else
        {
            targetSurfaceType = SurfaceType.None;
        }

        if (cancelInput && PlayerState.IsWearingSuit())
        {
            InputLibrary.cancel.ConsumeInput();
            Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Probe);
        }

        UpdatePromptVisibility();
    }

    void UpdatePromptVisibility()
    {
        var inToolMode = Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item) && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() == this;
        var promptsVisible = inToolMode && !OWTime.IsPaused();

        if (firePrompt == null)
        {
            firePrompt = new ScreenPrompt(InputLibrary.lockOn, InputLibrary.toolActionPrimary, GhostInTheMachine.NewHorizons.GetTranslationForUI($"{nameof(NomaiStaffItem)}_PlaceWall") + "   <CMD>", ScreenPrompt.MultiCommandType.NONE);
            Locator.GetPromptManager().AddScreenPrompt(firePrompt, PromptPosition.UpperRight);
        }
        if (altFirePrompt == null)
        {
            altFirePrompt = new ScreenPrompt(InputLibrary.toolActionSecondary, GhostInTheMachine.NewHorizons.GetTranslationForUI($"{nameof(NomaiStaffItem)}_RemoveWall") + "   <CMD>");
            Locator.GetPromptManager().AddScreenPrompt(altFirePrompt, PromptPosition.UpperRight);
        }
        if (cancelPrompt == null)
        {
            cancelPrompt = new ScreenPrompt(InputLibrary.cancel, UITextLibrary.GetString(UITextType.ScoutModePrompt) + "   <CMD>");
            Locator.GetPromptManager().AddScreenPrompt(cancelPrompt, PromptPosition.UpperRight);
        }

        firePrompt.SetVisibility(promptsVisible);
        altFirePrompt.SetVisibility(promptsVisible);
        cancelPrompt.SetVisibility(promptsVisible && PlayerState.IsWearingSuit());
    }

    protected void OnGUI()
    {
        if (OWTime.IsPaused() || !GhostInTheMachine.Instance.DebugModeEnabled) return;
        if (!Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item) || Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() != this) return;
        GUILayout.Label($"Surface Type: {targetSurfaceType}");
    }
}
