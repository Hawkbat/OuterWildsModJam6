using GhostInTheMachine.Controllers;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class DoorManager : ManagerBase<DoorManager>
{
    // The door prefab is pivoted on the floor and opens along its local X, with the two faces sitting at x = -1.43 and x = 1.43, so the volume has to reach past both of them to win the interact raycast
    static readonly Vector3 INTERACTION_OFFSET = new(0f, 2f, 0f);

    // A gateway is around ten metres wide and thin along its local Z, with the orb rail up above head height, so the volume sits low in the doorway where the player is rather than up on the controls
    static readonly Vector3 GATEWAY_INTERACTION_OFFSET = new(0f, 3f, 0f);

    const float INTERACTION_RADIUS = 1.75f;
    const float GATEWAY_INTERACTION_RADIUS = 3f;
    const float SWITCH_INTERACTION_RADIUS = 1f;
    const float INTERACTION_RANGE = 4f;
    
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
            var openSlot = door._openSwitches.FirstOrDefault();
            var closeSlot = door._closeSwitches.FirstOrDefault();
            var anchor = openSlot != null ? openSlot.transform.parent : root;
            PlaceVolume(anchor, ControlsPosition(root, openSlot, closeSlot), SWITCH_INTERACTION_RADIUS);
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

        PlaceVolume(root, root.TransformPoint(GATEWAY_INTERACTION_OFFSET), GATEWAY_INTERACTION_RADIUS);
    }

    void PlaceVolume(Transform parent, Vector3 position, float radius)
    {
        var interactable = new GameObject("InteractReceiver");
        interactable.transform.SetParent(parent, false);
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
