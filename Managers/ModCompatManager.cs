using OWML.Common;
using System.Collections;
using UnityEngine;

namespace GhostInTheMachine.Managers;

// Workarounds for other mods
public class ModCompatManager : ManagerBase<ModCompatManager>
{
    const string PICK_UP_CHERT = "orclecle.PickUpChert";

    // Pick Up Chert gives Chert a copy of the player's item socket a few frames into the scene. Anything already in the player's hands by then is duplicated along with it, leaving a dead Nomai staff childed to the carry tool in earlier spawns.
    const string CHERT_SOCKET_NAME = "ChertSocket";
    const float CHERT_SOCKET_TIMEOUT = 30f;

    // Pick Up Chert reroutes anything picked up while carrying Chert into Chert's hand and places it there itself, so our own hold offsets only apply when the item really did end up in the player's hands
    public static bool IsPlayerHoldTransform(Transform holdTransform)
    {
        var itemTool = Locator.GetToolModeSwapper()?.GetItemCarryTool();
        return itemTool == null || holdTransform == itemTool._defaultItemSocket.transform;
    }

    protected override void Awake()
    {
        base.Awake();

        if (GhostInTheMachine.Instance.ModHelper.Interaction.TryGetMod(PICK_UP_CHERT) != null)
        {
            StartCoroutine(RemoveItemsClonedIntoChertSocket());
        }
    }

    IEnumerator RemoveItemsClonedIntoChertSocket()
    {
        var itemTool = Locator.GetToolModeSwapper().GetItemCarryTool();

        // The socket only exists once Chert has been found and cloned
        Transform chertSocket = null;
        for (var elapsed = 0f; elapsed < CHERT_SOCKET_TIMEOUT && chertSocket == null; elapsed += Time.deltaTime)
        {
            yield return null;
            chertSocket = itemTool.transform.Find(CHERT_SOCKET_NAME);
        }
        if (chertSocket == null) yield break;

        // Chert themselves gets moved in here later, so just to be safe, only clear out items belonging to us
        foreach (Transform child in chertSocket)
        {
            var item = child.GetComponent<OWItem>();
            if (item == null || item.GetType().Assembly != typeof(ModCompatManager).Assembly) continue;

            GhostInTheMachine.Instance.ModHelper.Console.WriteLine($"Removing duplicate {item.GetType().Name} cloned into Pick Up Chert's item socket", MessageType.Info);
            Destroy(child.gameObject);
        }
    }
}
