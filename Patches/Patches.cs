using UnityEngine;
using HarmonyLib;
using GhostInTheMachine.Managers;

namespace GhostInTheMachine.Patches;

[HarmonyPatch(typeof(ShipLogDetectiveMode))]
public static class ShipLogDetectiveModePatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogDetectiveMode.UpdateMode))]
    public static void UpdateMode(ShipLogDetectiveMode __instance)
    {
        if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
        {
            if (!__instance._updateFrameAll && !__instance._updateRevealAnim && __instance._focusedSelectable != null)
            {
                var card = (ShipLogEntryCard)__instance._focusedSelectable;
                if (!Locator.GetEntryLocation(card.GetEntry().GetID()))
                {
                    // Used mark entry key to mark an entry that doesn't have a location, might be one of ours
                    ShipLogDialogueManager.Instance.OnActivateEntry(card.GetEntry().GetID());
                }
            }
        }
    }
}

[HarmonyPatch(typeof(ShipLogMapMode))]
public static class ShipLogMapModePatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogMapMode.UpdateMode))]
    public static void UpdateMode(ShipLogMapMode __instance)
    {
        if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD))
        {
            if (__instance._isEntryMenuOpen && __instance._entryIndex >= 0 && __instance._pressedUpTimer <= __instance._nextHoldUpTime && __instance._pressedDownTimer <= __instance._nextHoldDownTime)
            {
                var listItem = __instance._listItems[__instance._entryIndex];
                if (!Locator.GetEntryLocation(listItem.GetEntry().GetID()))
                {
                    // Used mark entry key to mark an entry that doesn't have a location, might be one of ours
                    ShipLogDialogueManager.Instance.OnActivateEntry(listItem.GetEntry().GetID());
                }
            }
        }
    }
}

[HarmonyPatch(typeof(GabbroDialogueSwapper))]
public static class GabbroDialogueSwapperPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(GabbroDialogueSwapper.Start))]
    public static void Start(GabbroDialogueSwapper __instance)
    {
        // If Gabbro's statue is deactivated, they act as if this is always the first loop
        if (PlayerData.PersistentConditionExists(Constants.PersistentConditions.STATUE_GABBRO) && PlayerData.GetPersistentCondition(Constants.PersistentConditions.STATUE_GABBRO))
        {
            __instance._activeConditionDialogue = __instance._conditionalDialogues[0];
            __instance._dialogueTree.SetTextXml(__instance._activeConditionDialogue.dialogueTextAsset);
        }
    }
}

[HarmonyPatch(typeof(SleepTimerUI))]
public static class SleepTimerUIPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(SleepTimerUI.OnWillRenderCanvases))]
    public static void OnWillRenderCanvases(SleepTimerUI __instance)
    {
        if (FastForwardManager.Instance.IsFastForwarding())
        {
            var startTime = FastForwardManager.Instance.GetDisplayStartTime();
            var endTime = FastForwardManager.Instance.GetDisplayEndTime();
            var progress = FastForwardManager.Instance.GetProgress();

            progress = Mathf.Clamp01(1f - Mathf.Pow(1f - progress, 5f));

            var displayTime = Mathf.Lerp(startTime, endTime, progress);

            var minutes = Mathf.FloorToInt(displayTime / 60f);
            var seconds = Mathf.FloorToInt(displayTime % 60f);

            __instance._stringBuilder.Length = 0;
            __instance._stringBuilder.Append(minutes.ToString("D2"));
            __instance._stringBuilder.Append(":");
            __instance._stringBuilder.Append(seconds.ToString("D2"));
            __instance._text.text = __instance._stringBuilder.ToString();
            var c = Color.red;
            __instance._text.color = new Color(c.r, c.g, c.b, __instance._text.color.a);

            foreach (var ember in __instance._emberInstances)
            {
                ember.image.enabled = false;
            }
        }
    }
}