using UnityEngine;

namespace GhostInTheMachine.Managers;

public class FastForwardManager : ManagerBase<FastForwardManager>
{
    const float MAX_FAST_FORWARD_MULTIPLIER = 10f;

    float startTime = 0f;
    float endTime = 0f;
    float fastForwardMultiplier = 1f;
    bool isFastForwarding = false;
    float displayStartTime = 0f;
    float displayEndTime = 0f;

    public bool IsFastForwarding() => isFastForwarding;
    public float GetStartTime() => startTime;
    public float GetEndTime() => endTime;
    public float GetProgress() => Mathf.InverseLerp(startTime, endTime, TimeLoop.GetSecondsElapsed());
    public float GetDisplayStartTime() => displayStartTime;
    public float GetDisplayEndTime() => displayEndTime;

    protected override void Awake()
    {
        base.Awake();
        enabled = false;
    }

    protected void Update()
    {
        if (!OWTime.IsPaused())
        {
            fastForwardMultiplier = Mathf.MoveTowards(fastForwardMultiplier, MAX_FAST_FORWARD_MULTIPLIER, 2f * Time.unscaledDeltaTime);
            OWTime.SetTimeScale(fastForwardMultiplier);
        }
        if (TimeLoop.GetSecondsElapsed() >= endTime)
        {
            StopFastForwarding();
        }
    }

    public void SetDisplayTimes(float startTime, float endTime)
    {
        displayStartTime = startTime;
        displayEndTime = endTime;
    }

    public void SetTargetTime(float targetTime)
    {
        startTime = TimeLoop.GetSecondsElapsed();
        endTime = targetTime;
        if (startTime < targetTime)
        {
            StartFastForwarding();
        }
        else
        {
            StopFastForwarding();
        }
    }

    void StartFastForwarding()
    {
        if (isFastForwarding) return;

        isFastForwarding = true;
        fastForwardMultiplier = 1f;
        Locator.GetPlayerCamera().enabled = false;
        OWTime.SetMaxDeltaTime(0.033333335f);
        Locator.GetAudioMixer().MixSleepAtCampfire(0f);
        OWInput.ChangeInputMode(InputMode.None);
        GlobalMessenger.FireEvent("StartFastForward");
        enabled = true;
    }

    void StopFastForwarding()
    {
        if (!isFastForwarding) return;

        isFastForwarding = false;
        fastForwardMultiplier = 1f;
        OWTime.SetTimeScale(1f);
        Locator.GetPlayerCamera().enabled = true;
        OWTime.SetTimeScale(1f);
        OWTime.SetMaxDeltaTime(0.06666667f);
        Locator.GetAudioMixer().UnmixSleepAtCampfire(0f);
        Locator.GetPlayerAudioController()._oneShotSource.PlayOneShot(AudioType.PlayerGasp_Medium);
        OWInput.ChangeInputMode(InputMode.Character);
        GlobalMessenger.FireEvent("EndFastForward");
        enabled = false;
    }
}
