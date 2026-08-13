using GhostInTheMachine.Controllers;
using OWML.Common;
using System.Linq;
using UnityEngine;
using static GhostInTheMachine.Constants;

namespace GhostInTheMachine.Managers;

public class ErnestoManager : ManagerBase<ErnestoManager>
{
    const string ANGLERFISH_PATH = "Anglerfish_Body/Beast_Anglerfish";
    const string DIALOGUE_PATH = "planets/Ghost/dialogue/Ernesto.xml";
    const string ROCK_DEATH_VOLUME_NAME = "GITM_ERNESTO_ROCK_DEATH";

    static readonly Vector3 ERNESTO_POSITION = new(2f, -0.25f, 0f);
    static readonly Vector3 ERNESTO_ROTATION = new(0f, 0f, 0f);
    const float ERNESTO_SCALE = 0.02f;

    StatueGhostController playerStatue;
    GameObject ernesto;
    CharacterDialogueTree dialogue;
    bool statueDeactivated;

    protected override void Awake()
    {
        base.Awake();

        statueDeactivated = PlayerData.GetPersistentCondition(PersistentConditions.STATUE_PLAYER);
        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    public void Attach(StatueGhostController playerStatue)
    {
        this.playerStatue = playerStatue;

        ernesto = GhostInTheMachine.CloneVanillaProp(ANGLERFISH_PATH);
        ernesto.name = "GITM_ERNESTO";
        ernesto.transform.SetParent(playerStatue.transform, false);
        ernesto.transform.localPosition = ERNESTO_POSITION;
        ernesto.transform.localEulerAngles = ERNESTO_ROTATION;
        ernesto.transform.localScale = Vector3.one * ERNESTO_SCALE;
        ernesto.AddComponent<GhostVisionController>();

        // Radius of zero leaves the conversation zone with its collider and interact receiver switched off, so only manual triggering will start the conversation
        (dialogue, _) = GhostInTheMachine.NewHorizons.SpawnDialogue(GhostInTheMachine.Instance, ernesto, DIALOGUE_PATH, 0f, 0f);
        dialogue.OnEndConversation += HandleEndConversation;

        ernesto.SetActive(false);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
        if (dialogue != null)
        {
            dialogue.OnEndConversation -= HandleEndConversation;
        }
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (condition != PersistentConditions.STATUE_PLAYER || state == statueDeactivated) return;

        statueDeactivated = state;
        if (state)
        {
            OnPlayerStatueDeactivated();
        }
    }

    void OnPlayerStatueDeactivated()
    {
        var conditions = DialogueConditionManager.SharedInstance;
        if (conditions.GetConditionState(DialogueConditions.ErnestoWarnedTwice))
        {
            // Twice was already twice more than he wanted to explain it
            TriggerRockDeath();
            return;
        }

        if (ernesto == null || dialogue == null || PlayerState.InConversation()) return;

        ernesto.SetActive(true);
        dialogue.StartConversation();
    }

    void HandleEndConversation()
    {
        if (ernesto != null)
        {
            ernesto.SetActive(false);
        }

        var conditions = DialogueConditionManager.SharedInstance;
        if (!conditions.GetConditionState(DialogueConditions.ErnestoUndo)) return;

        // Reactivating clears the persistent condition, which walks back the terrible fate check in SolarSystem.json
        conditions.SetConditionState(DialogueConditions.ErnestoUndo, false);
        playerStatue.Activate();
    }

    void TriggerRockDeath()
    {
        var volume = FindRockDeathVolume();

        // Add player to volume directly, bypassing physical overlap check
        DialogueConditionManager.SharedInstance.SetConditionState(DialogueConditions.ErnestoRockDeath, true);
        volume.AddObjectToVolume(Locator.GetPlayerDetector().gameObject);
    }

    OWTriggerVolume FindRockDeathVolume()
    {
        return Resources.FindObjectsOfTypeAll<OWTriggerVolume>()
            .FirstOrDefault(volume => volume.name == ROCK_DEATH_VOLUME_NAME);
    }
}
