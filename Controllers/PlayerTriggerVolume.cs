using System;
using UnityEngine;
using UnityEngine.Events;

namespace GhostInTheMachine.Controllers;

public class PlayerTriggerVolume : MonoBehaviour
{
    public UnityEvent OnPlayerEnter = new();

    OWTriggerVolume trigger;

    public static PlayerTriggerVolume Create(string name, Transform parent, Vector3 position, float radius)
    {
        var volume = new GameObject(name);
        volume.SetActive(false);
        volume.transform.SetParent(parent, false);
        volume.transform.position = position;
        volume.layer = LayerMask.NameToLayer("BasicEffectVolume");

        volume.AddComponent<SphereShape>().radius = radius;
        volume.AddComponent<OWTriggerVolume>();
        var playerTrigger = volume.AddComponent<PlayerTriggerVolume>();

        volume.SetActive(true);
        return playerTrigger;
    }

    protected void Awake()
    {
        trigger = GetComponent<OWTriggerVolume>();
        trigger.OnEntry += HandleEntry;
    }

    protected void OnDestroy()
    {
        if (trigger != null)
        {
            trigger.OnEntry -= HandleEntry;
        }
    }

    void HandleEntry(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
        {
            OnPlayerEnter?.Invoke();
        }
    }
}
