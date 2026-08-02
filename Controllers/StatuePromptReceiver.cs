
namespace GhostInTheMachine.Controllers;

public class StatuePromptReceiver : InteractReceiver
{
    StatueGhostController statue;

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
        return Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item) && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() is NomaiStaffItem;
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
        _screenPrompt.SetVisibility(_focused && OWInput.IsInputMode(InputMode.Character) && CanInteract());
    }

    void UpdatePrompt()
    {
        var promptText = GhostInTheMachine.NewHorizons.GetTranslationForUI(statue.IsActivated ? "DeactivateStatuePrompt" : "ActivateStatuePrompt");
        ChangePrompt(promptText);
    }
}
