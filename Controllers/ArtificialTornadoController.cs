using UnityEngine;

namespace GhostInTheMachine.Controllers;

// Reversible forming and collapsing off a single progress value, for NH tornados where the configured height lives on _tornadoRoot's own scale. Vanilla component stays for its bone animation
public class ArtificialTornadoController : MonoBehaviour
{
    public float formationDuration = 5f;
    public float collapseDuration = 3f;

    TornadoController tornado;
    GameObject effectsRoot;
    OWAudioSource audioSource;
    MaterialPropertyBlock matPropBlock;
    int propID_CutoffFade;

    Vector3 formedScale;

    bool activated;
    float progress;
    bool? fluidsActive;
    bool detached;

    public bool IsActivated => activated;
    public bool IsFormed => progress >= 1f;
    public bool IsCollapsed => progress <= 0f;

    public void Init(TornadoController tornado)
    {
        this.tornado = tornado;

        formedScale = transform.localScale;

        // Disable vanilla controller; we call what we need from it instead of letting it tick itself
        tornado.enabled = false;

        effectsRoot = tornado._topBone != null && tornado._topBone.parent != null ? tornado._topBone.parent.gameObject : null;

        matPropBlock = new MaterialPropertyBlock();
        propID_CutoffFade = Shader.PropertyToID("_CutoffFade");

        // NH mistakenly sets _audioSource to the wrong, inactive AudioRail instead of the one it's supposed to use
        var audioRail = transform.Find("AudioRail");
        audioSource = audioRail != null ? audioRail.GetComponentInChildren<OWAudioSource>(true) : null;
        if (audioSource != null)
        {
            tornado._audioSource = audioSource;
        }
    }

    public void SetActivated(bool activated)
    {
        this.activated = activated;
    }

    public void SetActivatedImmediate(bool activated)
    {
        this.activated = activated;
        progress = activated ? 1f : 0f;
        ApplyProgress();
    }

    // First safe moment to unhook, since the tornado can still be streamed out during Init
    void Detach()
    {
        detached = true;

        // Flying the ship through a Nomai machine shouldn't dispel it
        if (tornado._collapseTrigger != null)
        {
            tornado._collapseTrigger.OnEntry -= tornado.OnEnterCollapseTrigger;
        }

        // Sector events fire even while disabled and fade the audio out from under us
        tornado.SetSector(null);
    }

    protected void FixedUpdate()
    {
        if (!detached)
        {
            Detach();
        }

        var target = activated ? 1f : 0f;
        if (progress != target)
        {
            var duration = activated ? formationDuration : collapseDuration;
            progress = duration > 0f ? Mathf.MoveTowards(progress, target, Time.fixedDeltaTime / duration) : target;
            ApplyProgress();
        }

        SetFluidsActive(progress > 0f);
        if (audioSource != null && audioSource.gameObject.activeInHierarchy)
        {
            audioSource.SetLocalVolume(Mathf.SmoothStep(0f, 1f, progress));
        }

        if (progress > 0f)
        {
            tornado.UpdateAnimation();
        }
    }

    void ApplyProgress()
    {
        var t = Mathf.SmoothStep(0f, 1f, progress);

        transform.localScale = Vector3.Scale(formedScale, new Vector3(t, 1f, t));

        // Only matters if the bones snap to the planet's sphere, which the mock prefab doesn't, but keeps the elevations honest
        tornado._midElevation = Mathf.Lerp(tornado._topElevation, tornado._midStartElevation, t);
        tornado._bottomElevation = Mathf.Lerp(tornado._topElevation, tornado._bottomStartElevation, t);

        SetCutoffFade(tornado._topBlendRenderers, 1f - t / tornado._topFadeTime);
        SetCutoffFade(tornado._bodyRenderers, (1f - t) / tornado._bodyFadeTime);
        SetCutoffFade(tornado._bottomBlendRenderers, (1f - t) / tornado._bottomFadeTime);

        if (effectsRoot != null && effectsRoot.activeSelf != progress > 0f)
        {
            effectsRoot.SetActive(progress > 0f);
        }
    }

    void SetCutoffFade(Renderer[] renderers, float fade)
    {
        if (renderers == null) return;
        matPropBlock.SetFloat(propID_CutoffFade, Mathf.Clamp01(fade));
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.SetPropertyBlock(matPropBlock);
            }
        }
    }

    void SetFluidsActive(bool active)
    {
        if (fluidsActive == active) return;
        var applied = false;
        foreach (var fluid in tornado._fluids)
        {
            // The down tornado's bottom fluid has no trigger volume
            if (fluid == null || fluid.GetOWTriggerVolume() == null) continue;
            fluid.SetVolumeActivation(active);
            applied = true;
        }
        if (applied)
        {
            fluidsActive = active;
        }
    }
}
