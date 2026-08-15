using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Managers;

public class LoopPulseManager : ManagerBase<LoopPulseManager>
{
    static readonly Color EXTENDED_TINT = new(0.7470587f, 0.717647f, 1.5f);
    static readonly Color SHORTENED_TINT = new(1.5f, 0.529f, 0.576f);

    // How far the mask closes
    const float PEAK = 0.7f;

    // Matches New Horizons detail deactivationCondition timing
    const float SWAP_TIME = 0.7f;
    const float DURATION = 1.7f;

    const float MASK_CURVE_START = 0.3f;

    PlayerCameraEffectController cameraEffects;
    OWCamera playerCamera;
    Coroutine pulse;

    readonly Dictionary<string, bool> statueStates = [];

    protected override void Awake()
    {
        base.Awake();

        cameraEffects = FindObjectOfType<PlayerCameraEffectController>();
        playerCamera = Locator.GetPlayerCamera();

        foreach (var condition in ALL_STATUE_CONDITIONS)
        {
            statueStates[condition] = PlayerData.GetPersistentCondition(condition);
        }
        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (condition == STATUE_PLAYER) return;
        if (!statueStates.TryGetValue(condition, out var wasDeactivated) || wasDeactivated == state) return;

        statueStates[condition] = state;
        Play(state);
    }

    public void Play(bool timeLoopExtended)
    {
        if (pulse != null) StopCoroutine(pulse);
        pulse = StartCoroutine(PulseCoroutine(timeLoopExtended));
        Locator.GetPlayerAudioController().PlayOneShotInternal(AudioType.MemoryUplink_Start);
    }

    IEnumerator PulseCoroutine(bool extended)
    {
        var settings = playerCamera.postProcessingSettings;

        settings.eyeMask.edgeColorMode = true;
        settings.eyeMask.eyeMask = cameraEffects._timeLoopEyeMask;
        settings.eyeMask.linesEnabled = true;
        settings.eyeMask.edgeColor = extended ? EXTENDED_TINT : SHORTENED_TINT;

        var startTime = Time.time;
        float x;
        while ((x = (Time.time - startTime) / DURATION) < 1f)
        {
            // Bail if an actual death occurs so we don't scramble it
            if (cameraEffects._isDying)
            {
                pulse = null;
                yield break;
            }
            Apply(PEAK * OutAndBack(x, SWAP_TIME / DURATION));
            yield return null;
        }

        Apply(0f);
        yield return null;

        Restore();
    }

    static float OutAndBack(float x, float peakAt)
    {
        var t = Mathf.Clamp01(x < peakAt ? x / peakAt : (1f - x) / (1f - peakAt));
        return Mathf.Lerp(t, Mathf.SmoothStep(0f, 1f, t), 0.5f);
    }

    void Apply(float amount)
    {
        var settings = playerCamera.postProcessingSettings;

        settings.eyeMaskEnabled = true;
        settings.eyeMask.openness = 1f - cameraEffects._timeLoopEyeMaskCurve.Evaluate(Mathf.Lerp(MASK_CURVE_START, 1f, amount));
        settings.eyeMask.linesProgress = cameraEffects._timeLoopLinesProgressionCurve.Evaluate(amount) * 1.5f;

        settings.eyeMask.blendWidth = Mathf.Lerp(settings.eyeMaskDefault.blendWidth, cameraEffects._timeLoopBlendWidth, amount / PEAK);
    }

    void Restore()
    {
        var settings = playerCamera.postProcessingSettings;
        settings.eyeMask = settings.eyeMaskDefault;
        settings.eyeMaskEnabled = false;
        pulse = null;
    }
}
