using GhostInTheMachine.Controllers;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class DebugManager : ManagerBase<DebugManager>
{
    PlayerResources playerResources;

    protected override void Awake()
    {
        base.Awake();

        playerResources = FindObjectOfType<PlayerResources>();
    }

    protected void OnGUI()
    {
        if (!GhostInTheMachine.Instance.DebugModeEnabled) return;

        if (OWTime.IsPaused())
        {
            if (PlayerState.IsWearingSuit() && GUILayout.Button("Remove Suit"))
            {
                Locator.GetPlayerSuit().RemoveSuit(true);
            }
            if (!PlayerState.IsWearingSuit() && GUILayout.Button("Suit Up"))
            {
                Locator.GetPlayerSuit().SuitUp(false, true);
            }
            if (GUILayout.Button($"{(playerResources._invincible ? "Disable" : "Enable")} Invincibility"))
            {
                playerResources.ToggleInvincibility();
            }
            if (GUILayout.Button("Refill Resources"))
            {
                playerResources.DebugRefillResources();
            }
            if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem() != null)
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Give Staff"))
            {
                StaffManager.Instance.GivePlayerStaff();
            }
            if (GUILayout.Button("Give Mask"))
            {
                MaskManager.Instance.GivePlayerMask();
            }
            if (GUILayout.Button("Give Orb"))
            {
                TornadoManager.Instance.GivePlayerOrb();
            }
            GUI.enabled = true;
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
            if (GUILayout.Button("Reset Dialogue Conditions"))
            {
                foreach (var condition in typeof(Constants.DialogueConditions).GetFields().Where(f => f.FieldType == typeof(string)))
                {
                    var conditionName = (string)condition.GetValue(null);
                    DialogueConditionManager.SharedInstance.SetConditionState(conditionName, false);
                }
            }
            foreach (var condition in typeof(Constants.DialogueConditions).GetFields().Where(f => f.FieldType == typeof(string)))
            {
                var conditionName = (string)condition.GetValue(null);
                if (GUILayout.Button($"{conditionName}: {DialogueConditionManager.SharedInstance.GetConditionState(conditionName)}"))
                {
                    DialogueConditionManager.SharedInstance.SetConditionState(conditionName, !DialogueConditionManager.SharedInstance.GetConditionState(conditionName));
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Reset Mod Ship Logs (Reloads Scene)"))
            {
                var saves = PlayerData._currentGameSave.shipLogFactSaves.Where(s => s.Value.id.StartsWith("GITM_")).Select(s => s.Value).ToList();
                foreach (var save in saves)
                {
                    PlayerData._currentGameSave.shipLogFactSaves.Remove(save.id);
                }
                PlayerData.SaveCurrentGame();
                Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
            }
            foreach (var fact in typeof(Constants.ShipLogFacts).GetFields().Where(f => f.FieldType == typeof(string)))
            {
                var factID = (string)fact.GetValue(null);
                GUI.enabled = !Locator.GetShipLogManager().IsFactRevealed(factID);
                if (GUILayout.Button($"Reveal {factID}"))
                {
                    Locator.GetShipLogManager().RevealFact(factID);
                }
            }
            GUI.enabled = true;
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
        else
        {
            var staff = NomaiStaffItem.GetHeldStaff();
            if (staff != null)
            {
                GUILayout.Label($"Surface Type: {staff.TargetSurfaceType}");
                GUILayout.Label($"Wall Target: {staff.WallTarget}");

            }
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
