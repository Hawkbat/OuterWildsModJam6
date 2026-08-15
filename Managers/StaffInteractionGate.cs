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
        var staff = NomaiStaffItem.GetHeldStaff();
        var nowInteractive = staff != null && isUnlocked(staff);
        if (nowInteractive == interactive) return;

        interactive = nowInteractive;
        foreach (var receiver in receivers)
        {
            if (receiver == null) continue;
            if (!nowInteractive && receiver.gameObject.activeInHierarchy)
            {
                receiver.LoseFocus();
            }
            receiver.gameObject.SetActive(nowInteractive);
        }
    }
}
