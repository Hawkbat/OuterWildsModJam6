using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiStaffItem : OWItem
{
    string translatedName;
    SurfaceType targetSurfaceType;
    WallToolTarget wallTarget;

    public SurfaceType TargetSurfaceType => targetSurfaceType;
    public WallToolTarget WallTarget => wallTarget;

    ScreenPrompt firePrompt;
    ScreenPrompt altFirePrompt;
    ScreenPrompt cancelPrompt;

    public bool IsStatueToolUnlocked() => true;

    public bool IsWallToolUnlocked() => Locator.GetShipLogManager().IsFactRevealed(Constants.ShipLogFacts.WallToolUnlock);

    public bool IsDoorToolUnlocked() => Locator.GetShipLogManager().IsFactRevealed(Constants.ShipLogFacts.DoorToolUnlock);

    public bool IsBeamToolUnlocked() => Locator.GetShipLogManager().IsFactRevealed(Constants.ShipLogFacts.BeamToolUnlock);

    public static NomaiStaffItem GetHeldStaff()
    {
        var toolModeSwapper = Locator.GetToolModeSwapper();
        return toolModeSwapper.IsInToolMode(ToolMode.Item) ? toolModeSwapper.GetItemCarryTool().GetHeldItem() as NomaiStaffItem : null;
    }

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
        UpdatePromptVisibility();
        enabled = true;
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        _localDropNormal = Vector3.Lerp(Vector3.up, Random.onUnitSphere, 0.1f).normalized;
        base.DropItem(position, normal, parent, sector, customDropTarget);
        Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.ToolItemWarpCoreDrop);
        UpdatePromptVisibility();
        enabled = false;
    }

    protected void Update()
    {
        var inToolMode = GetHeldStaff() == this;
        var fireInput = inToolMode && (OWInput.IsNewlyPressed(InputLibrary.lockOn, InputMode.Character) || OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.Character));
        var altFireInput = inToolMode && OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.Character);
        var cancelInput = inToolMode && OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.Character);

        var player = Locator.GetPlayerTransform();
        var cam = Locator.GetPlayerCamera().transform;

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 75f, OWLayerMask.blockableInteractMask))
        {
            targetSurfaceType = Locator.GetSurfaceManager().GetHitSurfaceType(hit);
            var targetWall = hit.collider.GetComponentInParent<SpawnedWallController>();
            
            wallTarget = targetWall != null ? WallToolTarget.Removable
                : (targetSurfaceType == SurfaceType.Ceramic || targetSurfaceType == SurfaceType.Stone) ? WallToolTarget.Placeable
                : WallToolTarget.None;

            if (IsWallToolUnlocked() && (fireInput || altFireInput))
            {
                if (fireInput)
                {
                    if (wallTarget == WallToolTarget.Placeable)
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
        var inToolMode = GetHeldStaff() == this;
        var promptsVisible = inToolMode && !OWTime.IsPaused() && IsWallToolUnlocked();

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

    public enum WallToolTarget
    {
        None,
        Placeable,
        Removable
    }
}
