using OWML.Utils;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiOrbItem : OWItem
{
    static ItemType itemType;
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
            translatedName = GhostInTheMachine.NewHorizons.GetTranslationForUI(nameof(NomaiOrbItem));
        }
        if (itemType == ItemType.Invalid)
        {
            itemType = EnumUtils.Create<ItemType>(nameof(NomaiOrbItem));
        }
        _type = itemType;
        _interactable = true;
        _interactRange = 2f;
        _localDropOffset = new Vector3(0f, 0.1f, 0f);
        _localDropNormal = Vector3.up;
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.NomaiOrbStartDrag);
        transform.localPosition = holdOffset;
        transform.localEulerAngles = holdEulerAngles;
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
        Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.NomaiOrbStartDrag);
    }

    public override void SocketItem(Transform socketTransform, Sector sector)
    {
        base.SocketItem(socketTransform, sector);
        transform.localPosition = socketOffset;
        transform.localEulerAngles = socketEulerAngles;
        Locator.GetPlayerAudioController()._oneShotExternalSource.PlayOneShot(AudioType.NomaiOrbSlotActivated);
    }
}
