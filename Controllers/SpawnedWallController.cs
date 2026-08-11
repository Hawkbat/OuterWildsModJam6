using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class SpawnedWallController : MonoBehaviour
{
    const float SCALE_MIN = 0.01f;
    const float SCALE_MAX = 1f;
    const float SCALING_DURATION = 1.5f;

    bool growing = false;
    bool shrinking = false;
    float progress = 0f;
    OWAudioSource audioSource;

    public bool IsGrowing => growing;
    public bool IsGrown => IsIdle && progress >= 1f;
    public bool IsShrinking => shrinking;
    public bool IsShrunk => IsIdle && progress <= 0f;
    public bool IsIdle => !growing && !shrinking;

    protected void Awake()
    {
        audioSource = GetComponent<OWAudioSource>();
        SetScale(SCALE_MIN);
    }

    protected void Update()
    {
        if (!growing && !shrinking)
        {
            SetIdle();
            return;
        }
        var goal = growing ? 1f : 0f;
        progress = Mathf.MoveTowards(progress, goal, Time.deltaTime / SCALING_DURATION);
        var t = Mathf.Clamp01(1f - Mathf.Pow(1f - progress, 2f));
        SetScale(Mathf.Lerp(SCALE_MIN, SCALE_MAX, t));
        if (growing && progress >= 1f)
        {
            SetScale(SCALE_MAX);
            SetIdle();
        }
        else if (shrinking && progress <= 0f)
        {
            SetScale(SCALE_MIN);
            SetIdle();
        }
    }

    protected void SetScale(float newScale)
    {
        var scale = Mathf.Clamp(newScale, SCALE_MIN, SCALE_MAX);
        transform.localScale = new Vector3(1f, scale, 1f);
    }

    public void Grow()
    {
        growing = true;
        shrinking = false;
        enabled = true;
        audioSource.Stop();
        audioSource.Play();
    }

    public void Shrink()
    {
        growing = false;
        shrinking = true;
        enabled = true;
        audioSource.Stop();
        audioSource.Play();
    }

    public void SetIdle()
    {
        growing = false;
        shrinking = false;
        enabled = false;
        audioSource.Stop();
    }
}
