using GhostInTheMachine.Managers;
using OWML.Utils;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiMaskItem : NomaiConversationStone
{
    static string translatedName;

    public override string GetDisplayName() => translatedName;

    public override bool CheckIsDroppable() => true;

    public override void Awake()
    {
        base.Awake();
        if (string.IsNullOrEmpty(translatedName))
        {
            translatedName = GhostInTheMachine.NewHorizons.GetTranslationForUI(nameof(NomaiMaskItem));
        }
        _word = SolanumManager.Word;
        _interactable = true;
        _interactRange = 2f;
        _localDropOffset = new Vector3(0f, 0f, 0f);
        _localDropNormal = Vector3.forward;
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        transform.localPosition = new Vector3(0.25f, -1.25f, 0.25f);
        transform.localEulerAngles = new Vector3(0f, 180f, 0f);
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
    }
}
