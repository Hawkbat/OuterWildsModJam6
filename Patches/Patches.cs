using GhostInTheMachine.Controllers;
using GhostInTheMachine.Managers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Patches;

[HarmonyPatch(typeof(ShipLogDetectiveMode))]
public static class ShipLogDetectiveModePatches
{
    static bool skipFrameAll = false;

    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogDetectiveMode.UpdateMode))]
    public static void UpdateMode(ShipLogDetectiveMode __instance)
    {
        var card = __instance._focusedSelectable as ShipLogEntryCard;
        if (card != null)
        {
            var entryID = card.GetEntry().GetID();
            if (ShipLogDialogueManager.Instance.CanActivateEntry(entryID))
            {
                // If the focused entry is one of ours and can be activated, change mark prompt to be our activation prompt instead
                __instance._markOnHUDPrompt.SetText(GhostInTheMachine.NewHorizons.GetTranslationForUI("ActivateShipLogEntryPrompt"));
                __instance._markOnHUDPrompt.SetVisibility(true);

                if (OWInput.IsNewlyPressed(InputLibrary.markEntryOnHUD) && !__instance._updateFrameAll && !__instance._updateRevealAnim)
                {
                    ShipLogDialogueManager.Instance.OnActivateEntry(entryID);
                }
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogDetectiveMode.PrepareRevealAnimations))]
    public static void PrepareRevealAnimations(ShipLogDetectiveMode __instance)
    {
        if (Time.unscaledTime > __instance._enterModeTime + 0.5f)
        {
            // Skip the zoom-in if this isn't the initial entry reveal animation
            skipFrameAll = true;
        }
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogDetectiveMode.FinishRevealAnimation))]
    public static void FinishRevealAnimation(ShipLogDetectiveMode __instance)
    {
        if (skipFrameAll)
        {
            skipFrameAll = false;
            // Skip the zoom-out after revealing entries
            __instance._updateFrameAll = false;
            // Clear the fact reveal queue so currently revealed facts aren't treated as newly revealed again
            __instance._manager.ClearNewlyRevealedFacts();
            // Account for bounds changes with newly revealed entries
            __instance.UpdateBounds();
        }
    }
}

[HarmonyPatch(typeof(ShipLogEntryCard))]
public static class ShipLogEntryCardPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogEntryCard.Init))]
    public static void Init(ShipLogEntryCard __instance)
    {
        ShipLogDialogueManager.Instance.OnInitCard(__instance);
    }

    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogEntryCard.MarkAsRead))]
    public static void MarkAsRead(ShipLogEntryCard __instance)
    {
        ShipLogDialogueManager.Instance.OnMarkCardAsRead(__instance);
    }
}

[HarmonyPatch(typeof(ShipLogMapMode))]
public static class ShipLogMapModePatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(ShipLogMapMode.SetEntryFocus))]
    public static bool SetEntryFocus(ShipLogMapMode __instance, int index, ref bool __result)
    {
        index = index < 0 ? __instance._maxIndex : index >= __instance._maxIndex ? 0 : index;
        var listItem = __instance._listItems[index];
        if (listItem.GetEntry().GetID().StartsWith("GITM_"))
        {
            // Only allow viewing these entries in detective mode, not map mode
            ShipLogController ctrl = Object.FindObjectOfType<ShipLogController>();
            string focusedEntryID = listItem.GetEntry().GetID();
            ctrl._currentMode = ctrl._detectiveMode;
            __instance.ExitMode();
            ctrl._currentMode.EnterMode(focusedEntryID, null);
            ctrl._oneShotSource.PlayOneShot(AudioType.ShipLogEnterDetectiveMode);

            __result = false;
            return false; // Skip original method
        }
        __result = false;
        return true; // Continue with original method
    }

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

[HarmonyPatch(typeof(ShipLogEntry))]
public static class ShipLogEntryPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogEntry.GetFactsForDisplay))]
    public static void GetFactsForDisplay(ShipLogEntry __instance, List<ShipLogFact> __result)
    {
        // Filter out facts with empty text (used to add rumor arrows without extra text lines)
        __result = [.. __result.Where(f => !string.IsNullOrEmpty(f.GetText()))];
    }
}

[HarmonyPatch(typeof(ShipLogEntryLink))]
public static class ShipLogEntryLinkPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogEntryLink.GetFactsForDisplay))]
    public static void GetFactsForDisplay(ShipLogEntryLink __instance, List<ShipLogFact> __result)
    {
        // Filter out facts with empty text (used to add rumor arrows without extra text lines)
        __result = [.. __result.Where(f => !string.IsNullOrEmpty(f.GetText()))];
    }
}

[HarmonyPatch(typeof(GabbroDialogueSwapper))]
public static class GabbroDialogueSwapperPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(GabbroDialogueSwapper.Start))]
    public static void Start(GabbroDialogueSwapper __instance)
    {
        // If Gabbro's statue is deactivated, they act as if this is always the first loop
        if (PlayerData.PersistentConditionExists(STATUE_GABBRO) && PlayerData.GetPersistentCondition(STATUE_GABBRO))
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
            if (PlayerData.GetPersistentCondition(MASK_INSTALLED))
            {
                c = new(0.529f, 0.576f, 1.5f, 1f);
            }
            __instance._text.color = new Color(c.r, c.g, c.b, __instance._text.color.a);

            foreach (var ember in __instance._emberInstances)
            {
                ember.image.enabled = false;
            }
        }
    }
}

[HarmonyPatch(typeof(Flashback))]
public static class FlashbackPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(Flashback.OnTriggerFlashback))]
    public static void OnTriggerFlashback(Flashback __instance)
    {
        var normalColor = new Color(0.2423f, 0.2915f, 2.4401f, 1f);
        var errorColor = new Color(2.4401f, 0.2423f, 0.2915f, 1f);

        var error0 = !HasAllConditions(STATUE_GABBRO, STATUE_WORKSHOP, STATUE_PROBE);
        var error1 = !HasAllConditions(STATUE_FORGE, STATUE_ATP);
        var error2 = !HasAllConditions(STATUE_SS_LOWER, STATUE_SS_UPPER);
        var finalError = !HasAllConditions(MASK_INSTALLED);
        if (!finalError)
        {
            // If the mask is installed, we can ignore the other errors, the system is 'fixed'
            error0 = error1 = error2 = false;
        }

        __instance._forwardStreamsRenderers[0].material.SetColor("_EmissionColor", error0 ? errorColor : normalColor);
        __instance._forwardStreamsRenderers[1].material.SetColor("_EmissionColor", error1 ? errorColor : normalColor);
        __instance._forwardStreamsRenderers[2].material.SetColor("_EmissionColor", error2 ? errorColor : normalColor);

        if (finalError)
        {
            // TODO: spawn duplicate masks?
            __instance._maskTransform.localEulerAngles += Vector3.forward * 30f;
        }
    }

    static bool HasAllConditions(params string[] conditions)
    {
        foreach (var condition in conditions)
        {
            if (!PlayerData.PersistentConditionExists(condition) || !PlayerData.GetPersistentCondition(condition))
            {
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(ToolModeSwapper))]
public static class ToolModeSwapperPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(ToolModeSwapper), nameof(ToolModeSwapper.EquipToolMode))]
    public static bool EquipToolMode(ToolModeSwapper __instance, ToolMode mode)
    {
        if (__instance._currentToolGroup == ToolGroup.Suit && __instance.IsInToolMode(ToolMode.Item) && __instance._itemCarryTool.GetHeldItemType() == StaffManager.ItemType)
        {
            if (mode == ToolMode.Probe && !OWInput.IsPressed(InputLibrary.cancel))
            {
                // If the player is holding the staff and tries to switch to probe mode without using the cancel button, don't allow it
                return false; // Skip original method
            }
        }
        return true; // Continue with original method
    }
}

[HarmonyPatch(typeof(ToolModeUI))]
public static class ToolModeUIPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ToolModeUI.Update))]
    public static void Update(ToolModeUI __instance)
    {
        if (OWInput.IsInputMode(InputMode.Character) && Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item) && Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() == StaffManager.ItemType)
        {
            // Hide default scout launcher prompt while holding the staff since we block it
            __instance._probePrompt.SetVisibility(false);
        }
    }
}

[HarmonyPatch(typeof(NomaiConversationManager))]
public static class NomaiConversationManagerPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(NomaiConversationManager), nameof(NomaiConversationManager.Update))]
    public static void Update(NomaiConversationManager __instance)
    {
        var heldItem = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
        if (heldItem != null && heldItem is NomaiMaskItem)
        {
            // If we're holding the mask, handle the state machine differently

            // Skip initial dialogue
            if (!__instance._dialogueComplete)
            {
                __instance._dialogueComplete = true;
                __instance._characterDialogueTree.GetInteractVolume().DisableInteraction();
            }
            if (!__instance._solanumAnimController.isPerformingAction)
            {
                if (__instance._state == NomaiConversationManager.State.WatchingPlayer)
                {
                    // Jump ahead to raising cairns instead of creating stones first
                    __instance._state = NomaiConversationManager.State.RaisingCairns;
                    __instance._solanumAnimController.PlayRaiseCairns();
                    __instance._cairnAnimator.SetTrigger("Raise");
                    __instance._cairnCollision.SetActivation(true);
                }
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerCameraEffectController))]
public static class PlayerCameraEffectControllerPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(PlayerCameraEffectController.OnStartOfTimeLoop))]
    public static bool OnStartOfTimeLoop(PlayerCameraEffectController __instance)
    {
        if (!PlayerData.GetPersistentCondition(MASK_INSTALLED))
        {
            // If the player hasn't fixed the mask yet, skip the wake-up prompt logic to wake up immediately
            __instance.WakeUp();
            return false; // Skip original method
        }
        return true; // Continue with original method
    }
}

[HarmonyPatch(typeof(TranslatorWord))]
public static class TranslatorWordPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(TranslatorWord), MethodType.Constructor, [typeof(string), typeof(int), typeof(int), typeof(bool), typeof(float)])]
    public static bool Ctor(TranslatorWord __instance, string translatedText, int startPos, int endPos, bool previouslyTransated, float translationTime)
    {
        __instance._strBuilder = new StringBuilder();
        __instance.TranslatedText = translatedText.Replace("\\\\n", "\n");
        if (__instance.TranslatedText.Contains("<NbTimeloops>"))
        {
            int num = (TimeLoop.GetLoopCount() + 53) % 1000;
            int num2 = (int)Mathf.Floor((TimeLoop.GetLoopCount() + 318053) / 1000) % 1000;
            int num3 = (int)Mathf.Floor((TimeLoop.GetLoopCount() + 9318053) / 1000000);
            string text;
            if (TextTranslation.Get().GetLanguage() == TextTranslation.Language.ENGLISH || TextTranslation.Get().GetLanguage() == TextTranslation.Language.JAPANESE || TextTranslation.Get().GetLanguage() == TextTranslation.Language.CHINESE_SIMPLE)
            {
                text = string.Concat([num3, ",", num2.ToString("D3"), ",", num.ToString("D3")]);
            }
            else if (TextTranslation.Get().GetLanguage() == TextTranslation.Language.GERMAN || TextTranslation.Get().GetLanguage() == TextTranslation.Language.ITALIAN || TextTranslation.Get().GetLanguage() == TextTranslation.Language.PORTUGUESE_BR)
            {
                text = string.Concat([num3, ".", num2.ToString("D3"), ".", num.ToString("D3")]);
            }
            else
            {
                text = string.Concat([num3, " ", num2.ToString("D3"), " ", num.ToString("D3")]);
            }
            __instance.TranslatedText = __instance.TranslatedText.Replace("<NbTimeloops>", text);
        }
        if (__instance.TranslatedText.Contains("<WorldLine>"))
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append("X.");
            stringBuilder.Append(TimeLoop.GetWorldLineValue());
            stringBuilder.Append("M");
            string text = stringBuilder.ToString();
            __instance.TranslatedText = __instance.TranslatedText.Replace("<WorldLine>", text);
        }
        if (__instance.TranslatedText.Contains("<FirstLoop>"))
        {
            int num4 = 54;
            int num5 = 318;
            int num6 = 9;
            string text;
            if (TextTranslation.Get().GetLanguage() == TextTranslation.Language.ENGLISH || TextTranslation.Get().GetLanguage() == TextTranslation.Language.JAPANESE)
            {
                text = string.Concat([num6, ",", num5.ToString("D3"), ",", num4.ToString("D3")]);
            }
            else if (TextTranslation.Get().GetLanguage() == TextTranslation.Language.GERMAN || TextTranslation.Get().GetLanguage() == TextTranslation.Language.ITALIAN || TextTranslation.Get().GetLanguage() == TextTranslation.Language.PORTUGUESE_BR)
            {
                text = string.Concat([num6, ".", num5.ToString("D3"), ".", num4.ToString("D3")]);
            }
            else
            {
                text = string.Concat([num6, " ", num5.ToString("D3"), " ", num4.ToString("D3")]);
            }
            __instance.TranslatedText = __instance.TranslatedText.Replace("<FirstLoop>", text);
        }
        if (__instance.TranslatedText.Contains("<ActiveStatueList>"))
        {
            string text = string.Empty;
            var anyFound = false;
            foreach (var cond in ALL_STATUE_CONDITIONS)
            {
                if (!PlayerData.GetPersistentCondition(cond))
                {
                    anyFound = true;
                    text = string.Concat(text, GhostInTheMachine.NewHorizons.GetTranslationForUI(cond), "\n");
                }
            }
            if (!anyFound)
            {
                text = string.Concat(text, GhostInTheMachine.NewHorizons.GetTranslationForUI("AllStatuesDeactivated"), "\n");
            }

            __instance.TranslatedText = __instance.TranslatedText.Replace("<ActiveStatueList>", text);
        }
        if (__instance.TranslatedText.Contains("<"))
        {
            var spawn = SpawnManager.Instance.ActiveSpawn;

            string text;
            if (spawn != null)
            {
                // The elapsed time is based on the current spawn's start time, not the full 22-minute duration
                text = string.Concat(Mathf.Floor(TimeLoop.GetMinutesElapsed() - spawn.GetMinutesElapsed()));
            }
            else
            {
                text = string.Concat(Mathf.Floor(TimeLoop.GetMinutesElapsed()));
            }
            __instance.TranslatedText = __instance.TranslatedText.Replace("<TimeMinutes>", text);
            text = string.Concat(22f - Mathf.Floor(TimeLoop.GetMinutesElapsed()));
            __instance.TranslatedText = __instance.TranslatedText.Replace("<TimeMinutesRemaining>", text);
            text = string.Concat(Mathf.Floor((TimeLoop.GetSecondsElapsed() + 2501f) / 60f));
            __instance.TranslatedText = __instance.TranslatedText.Replace("<TimeMinutesSolarActivity>", text);
            if (spawn != null)
            {
                // The elapsed time is based on the current spawn's start time, not the full 22-minute duration
                text = string.Concat((int)(TimeLoop.GetSecondsElapsed() - spawn.GetSecondsElapsed()) % 60);
            }
            else
            {
                text = string.Concat((int)TimeLoop.GetSecondsElapsed() % 60);
            }
            __instance.TranslatedText = __instance.TranslatedText.Replace("<TimeSeconds>", text);
            text = string.Concat(Mathf.Max(0f, Mathf.Floor((690f - TimeLoop.GetSecondsElapsed()) / 60f)));
            __instance.TranslatedText = __instance.TranslatedText.Replace("<RemainingMinutes>", text);
            text = string.Concat(Mathf.Max(0f, (690f - Mathf.Floor(TimeLoop.GetSecondsElapsed())) % 60f));
            __instance.TranslatedText = __instance.TranslatedText.Replace("<RemainingSeconds>", text);
            text = string.Concat(22f - Mathf.Floor(TimeLoop.GetMinutesElapsed()));
            __instance.TranslatedText = __instance.TranslatedText.Replace("<MinutesToRedGiant>", text);
            text = string.Concat((1320f - Mathf.Floor(TimeLoop.GetSecondsElapsed())) % 60f);
            __instance.TranslatedText = __instance.TranslatedText.Replace("<SecondsToRedGiant>", text);
            text = string.Concat(Mathf.Floor((TimeLoop.GetSecondsElapsed() - 690f) / 60f));
            __instance.TranslatedText = __instance.TranslatedText.Replace("<MinutesSinceRedGiant>", text);
            text = string.Concat((Mathf.Floor(TimeLoop.GetSecondsElapsed()) - 690f) % 60f);
            __instance.TranslatedText = __instance.TranslatedText.Replace("<SecondsSinceRedGiant>", text);
        }
        __instance.StartPosition = startPos;
        __instance.EndPosition = endPos;
        __instance.Length = endPos - startPos;
        __instance._updateTime = 0f;
        __instance.DisplayText = "";
        __instance._startTranslating = false;
        __instance._isTranslated = false;
        __instance._translateTime = translationTime;
        return false; // Skip original constructor
    }
}

[HarmonyPatch(typeof(PauseMenuManager))]
public static class PauseMenuManagerPatches
{
    [HarmonyPrefix, HarmonyPatch(nameof(PauseMenuManager.TryOpenPauseMenu))]
    public static bool TryOpenPauseMenu(PauseMenuManager __instance, ref bool __result)
    {
        if (FastForwardManager.Instance.IsFastForwarding())
        {
            // Don't allow the pause menu to open while fast-forwarding
            __result = false;
            return false; // Skip original method
        }
        if (DialogueConditionManager.SharedInstance.GetConditionState(Constants.DialogueConditions.StatueInstalledThisLoop))
        {
            // We're in the finale cutscene, don't allow the pause menu to open
            __result = false;
            return false; // Skip original method
        }
        return true; // Continue with original method
    }
}
