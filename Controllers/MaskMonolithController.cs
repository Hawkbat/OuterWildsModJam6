using System;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class MaskMonolithController : MonoBehaviour
{
    [SerializeField] string powerOffCondition;
    [SerializeField] string errorFixCondition;
    [SerializeField] bool isPowered;
    [SerializeField] bool hasMask;
    [SerializeField] bool isError;
    [SerializeField] bool isUploading;
    [SerializeField] GameObject[] maskObjects;
    [SerializeField] MeshRenderer pulseGlowRenderer;
    [SerializeField] MeshRenderer maskEyeGlowRenderer;
    [SerializeField] Material pulseUnpoweredMaterial;
    [SerializeField] Material pulsePoweredMaterial;
    [SerializeField] MeshRenderer centralPulseRenderer;
    [SerializeField] MeshRenderer dataStreamRenderer;
    [SerializeField][ColorUsage(false, true)] Color pulseGlowDefaultColor = new(0.242f, 0.292f, 2.44f, 1f);
    [SerializeField][ColorUsage(false, true)] Color pulseGlowErrorColor = new(2.44f, 0.242f, 0.292f, 1f);
    [SerializeField][ColorUsage(false, true)] Color maskEyeGlowDefaultColor = new(0.529f, 0.576f, 1.5f, 1f);
    [SerializeField][ColorUsage(false, true)] Color maskEyeGlowErrorColor = new(1.5f, 0.529f, 0.576f, 1f);

    protected void Awake()
    {
        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void Start()
    {
        if (!string.IsNullOrEmpty(powerOffCondition))
        {
            isPowered = !PlayerData.GetPersistentCondition(powerOffCondition);
        }
        if (!string.IsNullOrEmpty(errorFixCondition))
        {
            isError = !PlayerData.GetPersistentCondition(errorFixCondition);
        }
        UpdateVisuals();
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (string.IsNullOrEmpty(condition)) return;
        if (condition == powerOffCondition)
        {
            isPowered = !state;
            UpdateVisuals();
        }
        if (condition == errorFixCondition)
        {
            isError = !state;
            UpdateVisuals();
        }
    }

    public bool HasMask() => hasMask;

    public void SetHasMask(bool hasMask)
    {
        this.hasMask = hasMask;
        UpdateVisuals();
    }

    public bool IsPowered() => isPowered;

    public void SetPowered(bool isPowered)
    {
        this.isPowered = isPowered;
        UpdateVisuals();
    }

    public bool IsError() => isError;

    public void SetError(bool isError)
    {
        this.isError = isError;
        UpdateVisuals();
    }

    public bool IsUploading() => isUploading;

    public void SetUploading(bool isUploading)
    {
        this.isUploading = isUploading;
        UpdateVisuals();
    }

    protected void UpdateVisuals()
    {
        if (isPowered)
        {
            pulseGlowRenderer.material = pulsePoweredMaterial;
            if (isError)
            {
                pulseGlowRenderer.material.SetColor("_EmissionColor", pulseGlowErrorColor);
                maskEyeGlowRenderer.material.SetColor("_EmissionColor", maskEyeGlowErrorColor);
                centralPulseRenderer.material.SetColor("_EmissionColor", pulseGlowErrorColor);
                dataStreamRenderer.material.SetColor("_EmissionColor", pulseGlowErrorColor);
            }
            else
            {
                pulseGlowRenderer.material.SetColor("_EmissionColor", pulseGlowDefaultColor);
                maskEyeGlowRenderer.material.SetColor("_EmissionColor", maskEyeGlowDefaultColor);
                centralPulseRenderer.material.SetColor("_EmissionColor", pulseGlowDefaultColor);
                dataStreamRenderer.material.SetColor("_EmissionColor", pulseGlowDefaultColor);
            }
        }
        else
        {
            pulseGlowRenderer.material = pulseUnpoweredMaterial;
        }
        maskEyeGlowRenderer.gameObject.SetActive(isPowered);

        foreach (var maskObject in maskObjects)
        {
            maskObject.SetActive(hasMask);
        }

        centralPulseRenderer.gameObject.SetActive(isPowered && isUploading);
        dataStreamRenderer.gameObject.SetActive(isPowered && isUploading);
    }
}
