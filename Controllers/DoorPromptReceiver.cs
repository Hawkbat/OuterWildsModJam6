
namespace GhostInTheMachine.Controllers;

public class DoorPromptReceiver : InteractReceiver
{
    GhostDoorController door;
    bool promptShowsOpen;

    public override void Awake()
    {
        base.Awake();
        door = GetComponentInParent<GhostDoorController>();
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
            door.Cycle();
        }
    }

    bool CanInteract()
    {
        var staff = NomaiStaffItem.GetHeldStaff();
        return !door.IsCycling && staff != null && staff.IsDoorToolUnlocked();
    }

    public override void UpdatePromptVisibility()
    {
        // The door takes a moment to finish animating, so the prompt only flips once it has settled either way
        if (promptShowsOpen != door.IsOpen)
        {
            UpdatePrompt();
        }
        _screenPrompt.SetVisibility(_focused && OWInput.IsInputMode(InputMode.Character) && CanInteract());
    }

    void UpdatePrompt()
    {
        promptShowsOpen = door.IsOpen;
        var promptText = GhostInTheMachine.NewHorizons.GetTranslationForUI(promptShowsOpen ? "CloseDoorPrompt" : "OpenDoorPrompt");
        ChangePrompt(promptText);
    }
}
