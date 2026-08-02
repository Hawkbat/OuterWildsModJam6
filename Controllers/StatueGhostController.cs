using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class StatueGhostController : MonoBehaviour
{
    public string persistentCondition;
    public bool canTurn;

    StatueVisualsController visuals;
    bool isActivated;

    public bool IsActivated => isActivated;

    protected void Awake()
    {
        visuals = GetComponent<StatueVisualsController>();
    }

    protected void Start()
    {
        if (PlayerData.PersistentConditionExists(persistentCondition) && PlayerData.GetPersistentCondition(persistentCondition))
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
        enabled = false;
    }

    public void Activate()
    {
        isActivated = true;
        if (PlayerData.PersistentConditionExists(persistentCondition))
        {
            PlayerData.SetPersistentCondition(persistentCondition, false);
        }
        visuals.SetEyesOpen(true);
        visuals.SetEyesGlowing(true);
        if (canTurn)
        {
            visuals.StartTurning(Locator.GetPlayerTransform().position);
        }
        visuals.turnAudioSource.PlayOneShot(AudioType.NomaiComputerRingActivate);
    }

    public void Deactivate()
    {
        isActivated = false;
        PlayerData.SetPersistentCondition(persistentCondition, true);
        visuals.SetEyesOpen(false);
        visuals.SetEyesGlowing(false);
        visuals.turnAudioSource.PlayOneShot(AudioType.NomaiComputerRingDeactivate);
    }
}
