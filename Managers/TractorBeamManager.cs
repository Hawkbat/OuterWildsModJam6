using GhostInTheMachine.Controllers;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

public class TractorBeamManager : ManagerBase<TractorBeamManager>
{
    const float INTERACTION_MARGIN = 0.3f;
    const float INTERACTION_MIN_RADIUS = 1f;
    const float INTERACTION_MAX_RADIUS = 3f;
    const float INTERACTION_RANGE = 4f;

    StaffInteractionGate gate;

    protected override void Awake()
    {
        base.Awake();
        gate = gameObject.AddComponent<StaffInteractionGate>();
        gate.Init(staff => staff.IsBeamToolUnlocked());
    }

    public void PlaceBeamInteractions()
    {
        // Take over all vanilla beams except the ship's hatch tractor beam
        var shipBody = Locator.GetShipBody();
        foreach (var beam in Resources.FindObjectsOfTypeAll<TractorBeamController>())
        {
            if (shipBody != null && beam.transform.IsChildOf(shipBody.transform)) continue;
            PlaceBeamInteraction(beam);
        }
    }

    void PlaceBeamInteraction(TractorBeamController beam)
    {
        if (beam.GetComponent<GhostBeamController>() != null) return;

        var controller = beam.gameObject.AddComponent<GhostBeamController>();
        // Only our own beams hand out the unlock memory, so the ability comes from the puzzle we built it for rather than from wandering past any of the eighty odd vanilla ones
        var triggersUnlock = beam.name.StartsWith("GITM_");
        controller.Init(beam, triggersUnlock);

        // The emitter is the only solid part of a beam, so measure the volume off its colliders instead of guessing at the prefab. The beam column itself is an effect volume, so it won't be caught up in this
        var bounds = new Bounds(beam.transform.position, Vector3.zero);
        foreach (var collider in beam.GetComponentsInChildren<MeshCollider>(true))
        {
            bounds.Encapsulate(collider.bounds);
        }

        var interactable = new GameObject("InteractReceiver");
        interactable.transform.SetParent(beam.transform, false);
        interactable.transform.position = bounds.center;
        interactable.layer = LayerMask.NameToLayer("Interactible");
        var col = interactable.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = Mathf.Clamp(bounds.extents.magnitude + INTERACTION_MARGIN, INTERACTION_MIN_RADIUS, INTERACTION_MAX_RADIUS);
        var receiver = interactable.AddComponent<BeamPromptReceiver>();
        receiver.SetInteractRange(INTERACTION_RANGE);

        gate.Add(receiver);
    }
}
