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
        visuals.SetEyeGlowColor(new Color(1.5f, 0f, 0.25f, 1f));
        if (PlayerData.PersistentConditionExists(persistentCondition) && PlayerData.GetPersistentCondition(persistentCondition))
        {
            Deactivate(true);
        }
        else
        {
            Activate(true);
        }
        enabled = false;
    }

    public void Activate(bool initial = false)
    {
        isActivated = true;
        if (PlayerData.PersistentConditionExists(persistentCondition))
        {
            PlayerData.SetPersistentCondition(persistentCondition, false);
        }
        visuals.SetEyesOpen(true);
        visuals.SetEyesGlowing(true);
        if (canTurn && !initial)
        {
            visuals.StartTurning(Locator.GetPlayerTransform().position);
        }
        // This breaks turning audio, do something else if we need to
        //visuals.turnAudioSource.PlayOneShot(AudioType.NomaiTractorBeamActivate);
    }

    public void Deactivate(bool initial = false)
    {
        isActivated = false;
        PlayerData.SetPersistentCondition(persistentCondition, true);
        visuals.SetEyesOpen(false);
        visuals.SetEyesGlowing(false, initial);
        //visuals.turnAudioSource.PlayOneShot(AudioType.NomaiTractorBeamDeactivate);
    }
}
