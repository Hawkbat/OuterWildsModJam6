using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GhostInTheMachine.Managers;

public class DebugManager : ManagerBase<DebugManager>
{

    protected void OnGUI()
    {
        if (!OWTime.IsPaused() || !GhostInTheMachine.Instance.DebugModeEnabled) return;
        if (PlayerState.IsWearingSuit() && GUILayout.Button("Remove Suit"))
        {
            Locator.GetPlayerSuit().RemoveSuit(true);
        }
        if (!PlayerState.IsWearingSuit() && GUILayout.Button("Suit Up"))
        {
            Locator.GetPlayerSuit().SuitUp(false, true);
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Reset Persistent Conditions"))
        {
            foreach (var condition in typeof(Constants.PersistentConditions).GetFields().Where(f => f.FieldType == typeof(string)))
            {
                var conditionName = (string)condition.GetValue(null);
                PlayerData.SetPersistentCondition(conditionName, false);
            }
        }
        foreach (var condition in typeof(Constants.PersistentConditions).GetFields().Where(f => f.FieldType == typeof(string)))
        {
            var conditionName = (string)condition.GetValue(null);
            if (GUILayout.Button($"{conditionName}: {PlayerData.GetPersistentCondition(conditionName)}"))
            {
                PlayerData.SetPersistentCondition(conditionName, !(PlayerData.PersistentConditionExists(conditionName) && PlayerData.GetPersistentCondition(conditionName)));
            }
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Reset Mod Ship Log"))
        {
            var saves = PlayerData._currentGameSave.shipLogFactSaves.Where(s => s.Value.id.StartsWith("GITM_")).Select(s => s.Value).ToList();
            foreach (var save in saves)
            {
                PlayerData._currentGameSave.shipLogFactSaves.Remove(save.id);
            }
            PlayerData.SaveCurrentGame();
            Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
        }
        GUILayout.Space(20);
        if (GUILayout.Button("Default Spawn")) WarpToSpawnPoint("Spawn_TH");
        if (GUILayout.Button("Spawn at Statue Island")) WarpToSpawnPoint("Spawn_StatueIsland_Beach");
        if (GUILayout.Button("Spawn inside ATP")) WarpToSpawnPoint("Spawn_TimeLoopDevice");
        if (GUILayout.Button("Spawn at Vessel")) WarpToSpawnPoint("Spawn_Vessel");
        if (GUILayout.Button("Spawn at Solanum"))
        {
            Locator.GetQuantumMoon()._collapseToIndex = 5;
            Locator.GetQuantumMoon().Collapse(true);
            WarpToSpawnPoint("Spawn_NorthPole");
        }
    }

    void WarpToSpawnPoint(string spawnPointName)
    {
        var spawner = Locator.GetPlayerBody().GetComponent<PlayerSpawner>();
        var spawnPoint = spawner._spawnList.FirstOrDefault(s => s.name == spawnPointName);
        if (!spawnPoint)
        {
            var spawnPoints = GameObject.FindObjectsOfType<SpawnPoint>();
            spawnPoint = spawnPoints.First(s => s.name == spawnPointName);
        }
        if (spawnPoint is EyeSpawnPoint eyeSpawn)
        {
            Locator.GetEyeStateManager().SetState(eyeSpawn.GetEyeState());
        }
        spawner.DebugWarp(spawnPoint);
    }
}
