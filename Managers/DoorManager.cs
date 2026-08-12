using GhostInTheMachine.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class DoorManager : ManagerBase<DoorManager>
{
    // The door prefab is pivoted on the floor and opens along its local X, with the two faces sitting at
    // x = -1.43 and x = 1.43, so the volume has to reach past both of them to win the interact raycast
    static readonly Vector3 INTERACTION_OFFSET = new(0f, 2f, 0f);

    const float INTERACTION_RADIUS = 1.75f;
    const float INTERACTION_RANGE = 4f;

    readonly List<DoorPromptReceiver> receivers = [];
    bool receiversInteractive = true;

    public void PlaceDoorInteractions()
    {
        // TODO: Alternate layout for airlocks since they have a very different shape from regular doors
        foreach (var door in Resources.FindObjectsOfTypeAll<NomaiMultiPartDoor>().Where(door => door is not NomaiAirlock))
        {
            PlaceDoorInteraction(door);
        }
    }

    protected void Update()
    {
        // Reaching past the faces also means covering the orb rails that run down the middle of the door,
        // so the volume only takes the raycast while the staff is actually able to work the door
        var staff = NomaiStaffItem.GetHeldStaff();
        var interactive = staff != null && staff.IsDoorToolUnlocked();
        if (interactive == receiversInteractive) return;

        receiversInteractive = interactive;
        foreach (var receiver in receivers)
        {
            if (receiver != null)
            {
                receiver.SetInteractionEnabled(interactive);
            }
        }
    }

    void PlaceDoorInteraction(NomaiMultiPartDoor door)
    {
        // The controller sits beside the panels rather than above them, so wire everything to the prefab root
        var root = door.transform.parent;
        if (root == null || root.GetComponent<GhostDoorController>() != null) return;

        var controller = root.gameObject.AddComponent<GhostDoorController>();
        controller.Init(door);

        var interactable = new GameObject("InteractReceiver");
        interactable.transform.SetParent(root, false);
        interactable.transform.localPosition = INTERACTION_OFFSET;
        interactable.layer = LayerMask.NameToLayer("Interactible");
        var col = interactable.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = INTERACTION_RADIUS;
        var receiver = interactable.AddComponent<DoorPromptReceiver>();
        receiver.SetInteractRange(INTERACTION_RANGE);

        receivers.Add(receiver);
    }
}
