using UnityEngine;

namespace GhostInTheMachine.Controllers;

/// <summary>
/// Gives the ship an identity understood by both QuantumSocket occupancy and the
/// standard QuantumObject visibility/collapse loop.
/// </summary>
public class QuantumShipSocketOccupant : QuantumObject
{
    Managers.QuantumShipManager manager;

    public override void Awake()
    {
        _collapseOnStart = false;
        _ignoreRetryQueue = true;
        base.Awake();

        // QuantumShipManager enables this only while the player is outside the ship on
        // the moon.
        enabled = false;
    }

    public void Initialize(Managers.QuantumShipManager quantumShipManager)
    {
        manager = quantumShipManager;
    }

    public override bool ChangeQuantumState(bool skipInstantVisibilityCheck)
    {
        return manager != null && manager.TryMoveToBiasedSocket();
    }
}
