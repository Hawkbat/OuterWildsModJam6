using GhostInTheMachine.Managers;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class NomaiMaskSocket : OWItemSocket
{
    public MaskMonolithController monolith;

    public override void Awake()
    {
        base.Awake();
        _acceptableType = ItemType.ConversationStone;
    }

    public override bool AcceptsItem(OWItem item) => !monolith.HasMask() && item is NomaiMaskItem;

    public override bool PlaceIntoSocket(OWItem item)
    {
        if (base.PlaceIntoSocket(item))
        {
            monolith.SetHasMask(true);
            item.SetVisible(false);
            EnableInteraction(false);

            StatueManager.Instance.OnMaskInstalled();

            return true;
        }
        return false;
    }

    public override OWItem RemoveFromSocket()
    {
        var item = base.RemoveFromSocket();
        if (item != null)
        {
            monolith.SetHasMask(false);
            item.SetVisible(true);

            StatueManager.Instance.OnMaskRemoved();
        }
        return item;
    }
}
