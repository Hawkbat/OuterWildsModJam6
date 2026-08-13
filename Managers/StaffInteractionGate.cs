using GhostInTheMachine.Controllers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class StaffInteractionGate : MonoBehaviour
{
    readonly List<GameObject> volumes = [];
    Func<NomaiStaffItem, bool> isUnlocked;

    bool interactive = true;

    public void Init(Func<NomaiStaffItem, bool> isUnlocked)
    {
        this.isUnlocked = isUnlocked;
    }

    public void Add(InteractReceiver receiver)
    {
        volumes.Add(receiver.gameObject);
    }

    public void Update()
    {
        // Our volumes sit over props that the player already interacts with normally, so they only take the interact raycast while the staff is out and the matching ability has been unlocked
        var staff = NomaiStaffItem.GetHeldStaff();
        var nowInteractive = staff != null && isUnlocked(staff);
        if (nowInteractive == interactive) return;

        interactive = nowInteractive;
        foreach (var volume in volumes)
        {
            if (volume != null)
            {
                // Deactivating the volume rather than calling SetInteractionEnabled on the receiver, because
                // most of these sit in sectors that haven't loaded yet, so InteractReceiver.Awake hasn't run
                // and its collider is still null. Active state also sticks while the sector is away, so they
                // come back in whichever state the staff was last left in
                volume.SetActive(nowInteractive);
            }
        }
    }
}
