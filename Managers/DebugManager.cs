using UnityEngine;

namespace GhostInTheMachine.Managers;

public class DebugManager : ManagerBase<DebugManager>
{

    protected void OnGUI()
    {
        if (!OWTime.IsPaused() || !GhostInTheMachine.Instance.DebugModeEnabled) return;
        if (GUILayout.Button("Reset Persistent Conditions"))
        {
            foreach (var condition in typeof(Constants.PersistentConditions).GetFields())
            {
                var conditionName = (string)condition.GetValue(null);
                PlayerData.SetPersistentCondition(conditionName, false);
            }
        }
        foreach (var condition in typeof(Constants.PersistentConditions).GetFields())
        {
            var conditionName = (string)condition.GetValue(null);
            if (GUILayout.Button($"{conditionName}: {PlayerData.GetPersistentCondition(conditionName)}"))
            {
                PlayerData.SetPersistentCondition(conditionName, !(PlayerData.PersistentConditionExists(conditionName) && PlayerData.GetPersistentCondition(conditionName)));
            }
        }
    }
}
