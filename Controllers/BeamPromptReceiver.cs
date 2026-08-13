
namespace GhostInTheMachine.Controllers;

public class BeamPromptReceiver : InteractReceiver
{
    GhostBeamController beam;
    GhostBeamController.BeamState promptState;

    public override void Awake()
    {
        base.Awake();
        beam = GetComponentInParent<GhostBeamController>();
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
            beam.Cycle();
        }
    }

    bool CanInteract()
    {
        var staff = NomaiStaffItem.GetHeldStaff();
        return staff != null && staff.IsBeamToolUnlocked();
    }

    public override void UpdatePromptVisibility()
    {
        if (promptState != beam.State)
        {
            UpdatePrompt();
        }
        _screenPrompt.SetVisibility(_focused && OWInput.IsInputMode(InputMode.Character) && CanInteract());
    }

    void UpdatePrompt()
    {
        // The prompt names what the next press does, so it walks the same cycle the beam does
        promptState = beam.State;
        var promptKey = promptState switch
        {
            GhostBeamController.BeamState.Off => "ActivateBeamPrompt",
            GhostBeamController.BeamState.Forward => "ReverseBeamPrompt",
            _ => "DeactivateBeamPrompt"
        };
        ChangePrompt(GhostInTheMachine.NewHorizons.GetTranslationForUI(promptKey));
    }
}
