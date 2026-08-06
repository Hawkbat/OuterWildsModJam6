using System.Collections.Generic;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class ShipLogDialogueManager : ManagerBase<ShipLogDialogueManager>
{
    ShipLogDetectiveMode detectiveMode;

    protected override void Awake()
    {
        base.Awake();
        detectiveMode = FindObjectOfType<ShipLogDetectiveMode>();
    }

    protected void Update()
    {
        var time = Time.unscaledTime;
        var scale = 1f / detectiveMode._scaleRoot.localScale.x;
        var pan = detectiveMode._panRoot.localPosition;
        Shader.SetGlobalFloat("_DataGhostUIUnscaledTime", time);
        Shader.SetGlobalFloat("_DataGhostUIEffectScale", scale);
        Shader.SetGlobalVector("_DataGhostUIEffectOffset", new Vector4(pan.x, pan.y, pan.z, 0f));
    }

    public bool CanActivateEntry(string entryID)
    {
        var entry = detectiveMode._manager.GetEntry(entryID);
        if (entry.GetState() == ShipLogEntry.State.Explored)
        {
            return false;
        }
        // TODO: Check if entry is a choice and if the choice has already been made, etc.
        return entryID.StartsWith("GITM_");
    }

    public void OnActivateEntry(string entryID)
    {
        // TODO: Unlock dependent entries/facts and restart reveal queue if necessary
    }

    void RestartRevealQueue(List<ShipLogFact> revealQueue)
    {
        detectiveMode._factRevealQueue.Clear();
        detectiveMode._factRevealQueue = revealQueue;
        detectiveMode._updateRevealAnim = true;
        detectiveMode._updateFrameAll = false;
        detectiveMode._targetCard = null;
        detectiveMode._animWaitSeconds = 0.5f;
        detectiveMode._panDuration = 0.7f;
        detectiveMode._queueIndex = 0;
        detectiveMode._startScale = detectiveMode._scaleRoot.localScale;
        detectiveMode.PrepareRevealAnimations();
    }
}
