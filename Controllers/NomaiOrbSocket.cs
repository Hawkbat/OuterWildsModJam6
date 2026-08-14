using GhostInTheMachine.Managers;

namespace GhostInTheMachine.Controllers;

public class NomaiOrbSocket : OWItemSocket
{
    public override void Awake()
    {
        base.Awake();
        _acceptableType = NomaiOrbItem.ItemType;
    }

    public override bool PlaceIntoSocket(OWItem item)
    {
        if (!base.PlaceIntoSocket(item)) return false;

        TornadoManager.Instance.SetOrbInstalled(true);
        return true;
    }

    public override OWItem RemoveFromSocket()
    {
        var item = base.RemoveFromSocket();
        if (item != null)
        {
            TornadoManager.Instance.SetOrbInstalled(false);
        }
        return item;
    }
}
