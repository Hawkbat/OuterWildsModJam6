
namespace GhostInTheMachine.Controllers;

public class StatuePromptReceiver : InteractReceiver
{
    StatueGhostController statue;
    bool promptHasCommand = true;

    public override void Awake()
    {
        base.Awake();
        statue = GetComponentInParent<StatueGhostController>();
        OnPressInteract += HandleOnPressInteract;
    }

    public override void Start()
    {
        base.Start();
        UpdatePrompt();
    }

    protected void OnDestroy()
    {
        OnPressInteract -= HandleOnPressInteract;
    }

    private void HandleOnPressInteract()
    {
        if (CanInteract())
        {
            if (statue.IsActivated)
            {
                statue.Deactivate();
            }
            else
            {
                statue.Activate();
            }
        }
        UpdatePrompt();
    }

    bool CanInteract()
    {
        return Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item) && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() is NomaiStaffItem staff && staff.IsStatueToolUnlocked();
    }

    public override void GainFocus()
    {
        base.GainFocus();
    }

    public override void LoseFocus()
    {
        base.LoseFocus();
    }

    public override void UpdatePromptVisibility()
    {
        if (CanInteract() != promptHasCommand)
        {
            UpdatePrompt();
        }
        base.UpdatePromptVisibility();
    }

    void UpdatePrompt()
    {
        promptHasCommand = CanInteract();
        _usingPromptWithCommand = promptHasCommand;
        var promptText = GhostInTheMachine.NewHorizons.GetTranslationForUI(!promptHasCommand ? "NeedStaffStatuePrompt" : statue.IsActivated ? "DeactivateStatuePrompt" : "ActivateStatuePrompt");
        ChangePrompt(promptText);
    }
}
