using GhostInTheMachine.Controllers;
using GhostInTheMachine.Managers;
using HarmonyLib;
using UnityEngine;

using static GhostInTheMachine.Constants.PersistentConditions;

namespace GhostInTheMachine.Patches;

[HarmonyPatch(typeof(ShipLogDetectiveMode))]
public static class ShipLogDetectiveModePatches
{
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
}

[HarmonyPatch(typeof(ShipLogEntryCard))]
public static class ShipLogEntryCardPatches
{
    [HarmonyPostfix, HarmonyPatch(nameof(ShipLogEntryCard.Init))]
    public static void OnEnterComputer(ShipLogEntryCard __instance)
    {
        if (__instance.GetEntry().GetID().StartsWith("GITM_GHOST_"))
        {
            __instance._background.material = __instance._nameBackground.material = __instance._border.material = CustomAssetsManager.Instance.GhostUIMaterial;
            __instance._name.color = new Color(0.75f, 0.75f, 1f);
        }
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
        var error3 = !HasAllConditions(SOLANUM_MASK_FIX);

        __instance._forwardStreamsRenderers[0].material.SetColor("_EmissionColor", error0 ? errorColor : normalColor);
        __instance._forwardStreamsRenderers[1].material.SetColor("_EmissionColor", error1 ? errorColor : normalColor);
        __instance._forwardStreamsRenderers[2].material.SetColor("_EmissionColor", error2 ? errorColor : normalColor);

        if (error3)
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