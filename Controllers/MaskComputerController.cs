using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class MaskComputerController : MonoBehaviour
{
    static readonly Color errorColor = new(1.5f, 0f, 0f, 1f);

    NomaiComputer computer;
    NomaiComputerRing[] rings;
    Color initialEmissionColor;
    bool init;
    bool isError;

    protected void Awake()
    {
        computer = GetComponent<NomaiComputer>();
        rings = GetComponentsInChildren<NomaiComputerRing>(true);

        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void Start()
    {
        isError = !PlayerData.GetPersistentCondition(Constants.PersistentConditions.MASK_INSTALLED);
        UpdateRingColors();
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (string.IsNullOrEmpty(condition)) return;
        if (condition == Constants.PersistentConditions.MASK_INSTALLED)
        {
            isError = !state;
            UpdateRingColors();
        }
    }

    void UpdateRingColors()
    {
        if (!init)
        {
            initialEmissionColor = rings[0]._baseEmissionColor;
            init = true;
        }
        foreach (var ring in rings)
        {
            ring._baseEmissionColor = isError ? errorColor : initialEmissionColor;
            NomaiComputerRing.s_matPropBlock.SetColor(NomaiComputerRing.s_propID_Detail1EmissionColor, Color.Lerp(NomaiComputerRing.s_colorTranslated, ring._baseEmissionColor, ring._emissionColorT));
            ring._renderer.SetPropertyBlock(NomaiComputerRing.s_matPropBlock);
        }
    }
}
