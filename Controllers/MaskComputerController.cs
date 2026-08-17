using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Controllers;

public class MaskComputerController : MonoBehaviour
{
    // The vanilla computer text but with our dynamic statue list instead
    static readonly string[] REPAIRED_TEXT = ["GITM_MASK_COMPUTER_FIXED_1", "GITM_MASK_COMPUTER_FIXED_2", "GITM_MASK_COMPUTER_3"];

    static readonly Color errorColor = new(1.5f, 0f, 0f, 1f);

    NomaiComputer computer;
    NomaiComputerRing[] rings;
    NomaiTextSwapper textSwapper;
    Color initialEmissionColor;
    bool init;
    bool isError;

    protected void Awake()
    {
        computer = GetComponent<NomaiComputer>();
        rings = GetComponentsInChildren<NomaiComputerRing>(true);
        textSwapper = new NomaiTextSwapper(computer, REPAIRED_TEXT);

        textSwapper.RegisterTranslations();
        TextTranslation.Get().OnLanguageChanged += textSwapper.RegisterTranslations;

        GlobalMessenger<string, bool>.AddListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
    }

    protected void OnDestroy()
    {
        GlobalMessenger<string, bool>.RemoveListener("NHPersistentConditionChanged", OnNHPersistentConditionChanged);
        if (TextTranslation.Get() != null)
        {
            TextTranslation.Get().OnLanguageChanged -= textSwapper.RegisterTranslations;
        }
    }

    protected void Start()
    {
        isError = !PlayerData.GetPersistentCondition(MASK_INSTALLED);
        UpdateComputer();
    }

    void OnNHPersistentConditionChanged(string condition, bool state)
    {
        if (condition != MASK_INSTALLED) return;

        isError = !state;
        UpdateComputer();
    }

    void UpdateComputer()
    {
        textSwapper.SetReplaced(!isError);
        UpdateRingColors();
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
