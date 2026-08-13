using GhostInTheMachine.Controllers;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class DoorManager : ManagerBase<DoorManager>
{
    // The door prefab is pivoted on the floor and opens along its local X, with the two faces sitting at
    // x = -1.43 and x = 1.43, so the volume has to reach past both of them to win the interact raycast
    static readonly Vector3 INTERACTION_OFFSET = new(0f, 2f, 0f);

    const float INTERACTION_RADIUS = 1.75f;
    const float SWITCH_INTERACTION_RADIUS = 1f;
    const float INTERACTION_RANGE = 4f;

    // Reaching past the faces also means covering the orb rails that run down the middle of the door, so
    // the volume only takes the raycast while the staff is actually able to work the door
    StaffInteractionGate gate;

    protected override void Awake()
    {
        base.Awake();
        gate = gameObject.AddComponent<StaffInteractionGate>();
        gate.Init(staff => staff.IsDoorToolUnlocked());
    }

    public void PlaceDoorInteractions()
    {
        foreach (var door in Resources.FindObjectsOfTypeAll<NomaiMultiPartDoor>())
        {
            PlaceDoorInteraction(door);
        }
        foreach (var gateway in Resources.FindObjectsOfTypeAll<NomaiGateway>())
        {
            PlaceGatewayInteraction(gateway);
        }
    }

    void PlaceDoorInteraction(NomaiMultiPartDoor door)
    {
        var root = door.transform.parent;
        if (root == null || root.GetComponent<GhostDoorController>() != null) return;

        root.gameObject.AddComponent<GhostDoorController>().Init(door);

        if (door is NomaiAirlock)
        {
            PlaceVolume(root, ControlsPosition(root, door._openSwitches.FirstOrDefault(), door._closeSwitches.FirstOrDefault()), SWITCH_INTERACTION_RADIUS);
        }
        else
        {
            PlaceVolume(root, root.TransformPoint(INTERACTION_OFFSET), INTERACTION_RADIUS);
        }
    }

    void PlaceGatewayInteraction(NomaiGateway gateway)
    {
        var root = gateway.transform;
        if (root.GetComponent<GhostDoorController>() != null) return;

        root.gameObject.AddComponent<GhostDoorController>().Init(gateway);

        PlaceVolume(root, ControlsPosition(root, gateway._openSlot, gateway._closeSlot), SWITCH_INTERACTION_RADIUS);
    }

    void PlaceVolume(Transform root, Vector3 position, float radius)
    {
        var interactable = new GameObject("InteractReceiver");
        interactable.transform.SetParent(root, false);
        interactable.transform.position = position;
        interactable.layer = LayerMask.NameToLayer("Interactible");
        var col = interactable.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;
        var receiver = interactable.AddComponent<DoorPromptReceiver>();
        receiver.SetInteractRange(INTERACTION_RANGE);

        gate.Add(receiver);
    }

    static Vector3 ControlsPosition(Transform fallback, params NomaiInterfaceSlot[] slots)
    {
        var total = Vector3.zero;
        var count = 0;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            total += slot.transform.position;
            count++;
        }
        return count > 0 ? total / count : fallback.position;
    }
}
