using UnityEngine;

namespace GhostInTheMachine.Controllers;

// Vanilla's forming and collapsing can't drive one of New Horizons' tornados. It builds them from the Brittle Hollow observatory mock, where _tornadoRoot is the same GameObject that carries the controller, and then puts the configured height on that transform's scale. So vanilla forming lerps the scale to Vector3.one and throws the height away, while vanilla collapsing switches off the object running the update loop and the tornado can never come back. It also drives the transition off a start timestamp, so a formation can only ever run to completion. This drives the whole thing off a single reversible progress value instead, and leaves the vanilla component in place for its bone animation.
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

        // New Horizons puts the configured height on this transform, which is also what vanilla treats as _tornadoRoot, so every scale we apply has to be measured against it rather than against Vector3.one
        formedScale = transform.localScale;

        // Vanilla's FixedUpdate would run its own formation timer alongside ours and undo our scale; we call the parts we still want ourselves
        tornado.enabled = false;

        // The bones hang off the one child that holds every renderer, so switching it off leaves nothing of a collapsed tornado behind
        effectsRoot = tornado._topBone != null && tornado._topBone.parent != null ? tornado._topBone.parent.gameObject : null;

        // Vanilla reads the cutoff off its own block, but that's only built in an Awake we may not have reached yet
        matPropBlock = new MaterialPropertyBlock();
        propID_CutoffFade = Shader.PropertyToID("_CutoffFade");

        // New Horizons hands _audioSource an observatory audio rail that it instantiated inactive and never switched back on, so every vanilla fade against it throws. The rail it does switch on is the one it parents in here, and pointing at that one also stops the two of them playing over each other
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

    // The tornado can still be streamed out when Init runs, in which case vanilla hasn't hooked any of this up yet.
    // FixedUpdate can't run before the object is awake, so this is the first moment it's safe to unhook
    void Detach()
    {
        detached = true;

        // Flying the ship through a Nomai machine shouldn't dispel it
        if (tornado._collapseTrigger != null)
        {
            tornado._collapseTrigger.OnEntry -= tornado.OnEnterCollapseTrigger;
        }

        // These fire regardless of the component being disabled, and they fade the audio out from under us on their own schedule. Nothing else we kept reads _isSectorOccupied
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

        // Both of these depend on an Awake that may not have happened when Init ran, and both are cheap enough to keep nudging until they take
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

        // Only does anything when the tornado snaps its bones to the planet's sphere, which the mock prefab doesn't, but it costs nothing to keep the elevations honest
        tornado._midElevation = Mathf.Lerp(tornado._topElevation, tornado._midStartElevation, t);
        tornado._bottomElevation = Mathf.Lerp(tornado._topElevation, tornado._bottomStartElevation, t);

        // Vanilla fades these three at different rates while forming, but collapses them all at the body's rate. Running the formation curves in both directions instead keeps a reversed transition retracing its own steps
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
            // The down tornado's bottom fluid ships switched off and without the trigger volume SetVolumeActivation needs, so it would throw if we treated it like the center one. A volume in a sector that hasn't streamed in yet looks the same, hence only recording the state once something actually took it
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
