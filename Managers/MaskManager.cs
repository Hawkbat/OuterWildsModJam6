using GhostInTheMachine.Controllers;
using UnityEngine;
using static GhostInTheMachine.Constants;

namespace GhostInTheMachine.Managers;

public class MaskManager : ManagerBase<MaskManager>
{
    static AudioClip finaleAudioClip;
    
    OWAudioSource finaleAudioSrc;


    protected override void Awake()
    {
        base.Awake();

        GameObject.Find("TimeLoopRing_Body/Interactibles_TimeLoopRing_Hidden/GITM_MASK_COMPUTER").AddComponent<MaskComputerController>();

        if (!finaleAudioClip)
        {
            finaleAudioClip = GhostInTheMachine.Instance.ModHelper.Assets.GetAudio("planets/TimeLoopRing/OW_EndTimes_Reversed.mp3");
        }

        finaleAudioSrc = gameObject.AddComponent<OWAudioSource>();
        finaleAudioSrc.SetTrack(OWAudioMixer.TrackName.Menu);
        finaleAudioSrc.spatialBlend = 0f;
        finaleAudioSrc.SetLocalVolume(0.5f);
        finaleAudioSrc.clip = finaleAudioClip;
        finaleAudioSrc.playOnAwake = false;
        finaleAudioSrc.Stop();
    }

    public NomaiMaskItem GivePlayerMask()
    {
        var mask = FindObjectOfType<NomaiMaskItem>();
        Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(mask);
        return mask;
    }

    public void OnMaskInstalled()
    {
        DialogueConditionManager.SharedInstance.SetConditionState(DialogueConditions.StatueInstalledThisLoop, true);
        PlayerData.SetPersistentCondition(PersistentConditions.MASK_INSTALLED, true);
        FastForwardManager.Instance.SetDisplayTimes(TimeLoop.GetSecondsElapsed(), TimeLoop._loopDuration);
        var actualTimeOffset = 10f;
        TimeLoop.SetSecondsRemaining(60f + actualTimeOffset);
        FastForwardManager.Instance.SetTargetTime(TimeLoop._loopDuration - actualTimeOffset);
        if (Locator.GetGlobalMusicController()._playingEndTimes)
        {
            Locator.GetGlobalMusicController()._playingEndTimes = false;
            Locator.GetGlobalMusicController()._endTimesSource.Stop();
        }
        else
        {
            Locator.GetAudioMixer().MixEndTimes(1f);
        }
        Locator.GetGlobalMusicController().enabled = false;
        finaleAudioSrc.FadeIn(0.5f);
    }

    public void OnMaskRemoved()
    {
        DialogueConditionManager.SharedInstance.SetConditionState(DialogueConditions.StatueInstalledThisLoop, false);
        PlayerData.SetPersistentCondition(PersistentConditions.MASK_INSTALLED, false);
        if (finaleAudioSrc.isPlaying)
        {
            finaleAudioSrc.FadeOut(0.5f);
        }
    }
}
