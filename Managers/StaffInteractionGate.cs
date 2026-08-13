using GhostInTheMachine.Controllers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class StaffInteractionGate : MonoBehaviour
{
    readonly List<InteractReceiver> receivers = [];
    Func<NomaiStaffItem, bool> isUnlocked;

    bool interactive = true;

    public void Init(Func<NomaiStaffItem, bool> isUnlocked)
    {
        this.isUnlocked = isUnlocked;
    }

    public void Add(InteractReceiver receiver)
    {
        receivers.Add(receiver);
    }

    public void Update()
    {
        // Our volumes sit over props that the player already interacts with normally, so they only take the interact raycast while the staff is out and the matching ability has been unlocked
        var staff = NomaiStaffItem.GetHeldStaff();
        var nowInteractive = staff != null && isUnlocked(staff);
        if (nowInteractive == interactive) return;

        interactive = nowInteractive;
        foreach (var receiver in receivers)
        {
            if (receiver != null)
            {
                receiver.SetInteractionEnabled(nowInteractive);
            }
        }
    }
}
