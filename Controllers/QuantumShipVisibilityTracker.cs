using UnityEngine;

namespace GhostInTheMachine.Controllers;

/// <summary>
/// Builds a game-native visibility shape around the ship so QuantumObject can use the
/// same frustum and VisibilityOccluder rules as vanilla quantum props.
/// </summary>
public static class QuantumShipVisibilityTracker
{
    public static ShapeVisibilityTracker Create(OWRigidbody shipBody)
    {
        var bounds = GetShipLocalBounds(shipBody);
        var trackerObject = new GameObject("GITM_QuantumShipVisibilityTracker");
        trackerObject.transform.SetParent(shipBody.transform, false);

        var shape = trackerObject.AddComponent<BoxShape>();
        shape.SetCollisionMode(Shape.CollisionMode.Manual);
        shape.center = bounds.center;
        shape.size = bounds.size;

        return trackerObject.AddComponent<ShapeVisibilityTracker>();
    }

    static Bounds GetShipLocalBounds(OWRigidbody shipBody)
    {
        var shipTransform = shipBody.transform;
        var initialized = false;
        var localBounds = new Bounds(Vector3.zero, Vector3.one);

        foreach (var collider in shipBody.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled || collider.isTrigger) continue;
            EncapsulateWorldBounds(ref localBounds, ref initialized, collider.bounds, shipTransform);
        }

        return localBounds;
    }

    static void EncapsulateWorldBounds(ref Bounds localBounds, ref bool initialized, Bounds worldBounds, Transform root)
    {
        var min = worldBounds.min;
        var max = worldBounds.max;
        for (var x = 0; x < 2; x++)
        {
            for (var y = 0; y < 2; y++)
            {
                for (var z = 0; z < 2; z++)
                {
                    var worldPoint = new Vector3(x == 0 ? min.x : max.x, y == 0 ? min.y : max.y, z == 0 ? min.z : max.z);
                    var localPoint = root.InverseTransformPoint(worldPoint);
                    if (!initialized)
                    {
                        localBounds = new Bounds(localPoint, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localPoint);
                    }
                }
            }
        }
    }
}
