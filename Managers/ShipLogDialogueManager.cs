using UnityEngine;

namespace GhostInTheMachine.Managers;

public class ShipLogDialogueManager : ManagerBase<ShipLogDialogueManager>
{
    ShipLogMapMode mapMode;
    ShipLogDetectiveMode detectiveMode;

    protected override void Awake()
    {
        base.Awake();
        mapMode = FindObjectOfType<ShipLogMapMode>();
        detectiveMode = FindObjectOfType<ShipLogDetectiveMode>();
    }

    public void OnActivateEntry(string entryID)
    {

    }
}
