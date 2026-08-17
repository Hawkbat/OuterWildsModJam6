using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Controllers;

public class StatueGhostController : MonoBehaviour
{
    static readonly Color DEFAULT_EYE_GLOW_COLOR = new(0.529f, 0.576f, 1.5f, 1f);
    static readonly Color ERROR_EYE_GLOW_COLOR = new(1.5f, 0f, 0.25f, 1f);

    public string persistentCondition;
    public bool canTurn;

    StatueVisualsController visuals;
    bool isActivated;

    public bool IsActivated => isActivated;

    protected void Awake()
    {
        visuals = GetComponent<StatueVisualsController>();

        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void Start()
    {
        UpdateEyeGlowColor();
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
    }

    public void Deactivate(bool initial = false)
    {
        isActivated = false;
        PlayerData.SetPersistentCondition(persistentCondition, true);
        visuals.SetEyesOpen(false);
        visuals.SetEyesGlowing(false, initial);
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (condition != MASK_INSTALLED) return;

        UpdateEyeGlowColor();
    }

    void UpdateEyeGlowColor()
    {
        visuals.SetEyeGlowColor(PlayerData.GetPersistentCondition(MASK_INSTALLED) ? DEFAULT_EYE_GLOW_COLOR : ERROR_EYE_GLOW_COLOR);
    }
}
