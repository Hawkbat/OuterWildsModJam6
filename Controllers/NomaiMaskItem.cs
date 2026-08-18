using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiMaskItem : NomaiConversationStone
{
    static string translatedName;

    public Vector3 holdOffset;
    public Vector3 holdEulerAngles;
    public Vector3 socketOffset;
    public Vector3 socketEulerAngles;

    public override string GetDisplayName() => translatedName;

    public override bool CheckIsDroppable() => true;

    public override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(translatedName))
        {
            translatedName = GhostInTheMachine.NewHorizons.GetTranslationForUI(nameof(NomaiMaskItem));
        }
        _word = SolanumManager.CustomWord;
        SetColliderActivation(true);
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        if (ModCompatManager.IsPlayerHoldTransform(holdTranform))
        {
            transform.localPosition = holdOffset;
            transform.localEulerAngles = holdEulerAngles;
        }
        Locator.GetShipLogManager().RevealFact(Constants.ShipLogFacts.MaskAcquired);
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
    }

    public override void SocketItem(Transform socketTransform, Sector sector)
    {
        base.SocketItem(socketTransform, sector);
        transform.localPosition = socketOffset;
        transform.localEulerAngles = socketEulerAngles;
    }
}
