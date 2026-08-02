using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GhostInTheMachine.Controllers;

public class ShipDestructionController : MonoBehaviour
{
    ShipDamageController damageController;
    float targetTime = 0f;

    protected void Awake()
    {
        damageController = FindObjectOfType<ShipDamageController>();
        enabled = false;
    }

    protected void Update()
    {
        if (TimeLoop.GetSecondsElapsed() >= targetTime)
        {
            damageController.TriggerHullBreach(true);
            enabled = false;
            Destroy(this);
        }
    }

    public void SetTargetTime(float targetTime)
    {
        this.targetTime = targetTime;
        enabled = true;
    }
}
