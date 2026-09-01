using GhostInTheMachine.Controllers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GhostInTheMachine.Managers;

/// <summary>
/// Keeps the ship with the Quantum Moon and gives it a safe place in the shrine/shuttle
/// socket network whenever the player is not aboard.
/// </summary>
public class QuantumShipManager : ManagerBase<QuantumShipManager>
{
    const string SOCKET_ROOT_NAME = "Structure_QM_QuantumShrineSockets";
    // Shrine sockets sit slightly below their local terrain. A fixed center-of-mass
    // height is more consistent across surface states than querying their colliders.
    const float SHIP_SOCKET_HEIGHT = 4.1f;
    const float TRACTOR_BEAM_EXIT_FALLBACK = 3f;

    QuantumMoon quantumMoon;
    OWRigidbody moonBody;
    OWRigidbody shipBody;
    ShipDamageController shipDamageController;
    FluidVolume shipTractorBeamFluid;
    ShapeVisibilityTracker shipVisibilityTracker;
    QuantumShipSocketOccupant socketOccupant;
    QuantumSocket[] sockets;
    QuantumSocket occupiedSocket;

    bool shipOnQuantumMoon;
    bool quantumBehaviorActive;
    bool previousShipInvulnerability;
    bool shipWasKinematic;
    int lastMoveFrame = -1;
    int failedNearbySocketRolls;
    float quantumEnableFallbackTime;

    protected override void Awake()
    {
        base.Awake();

        quantumMoon = Locator.GetQuantumMoon();
        moonBody = quantumMoon.GetAttachedOWRigidbody(false);
        shipBody = Locator.GetShipBody();
        shipDamageController = shipBody.GetComponentInChildren<ShipDamageController>();
        var tractorBeamSwitch = shipBody.GetComponentInChildren<ShipTractorBeamSwitch>(true);
        shipTractorBeamFluid = tractorBeamSwitch != null ? tractorBeamSwitch._beamFluid : null;
        shipVisibilityTracker = QuantumShipVisibilityTracker.Create(shipBody);
        socketOccupant = shipBody.gameObject.AddComponent<QuantumShipSocketOccupant>();
        socketOccupant.Initialize(this);
        socketOccupant.SetActivation(false);

        var socketRoot = quantumMoon.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(transform => transform.name == SOCKET_ROOT_NAME);
        sockets = socketRoot != null
            ? socketRoot.GetComponentsInChildren<QuantumSocket>(true)
            : new QuantumSocket[0];

        if (sockets.Length == 0)
        {
            GhostInTheMachine.Instance.ModHelper.Console.WriteLine(
                $"Couldn't find Quantum Moon ship sockets beneath {SOCKET_ROOT_NAME}.",
                OWML.Common.MessageType.Error);
        }

        foreach (var socket in sockets)
        {
            if (socket.GetVisibilityObject() != null)
            {
                socket.OnNewlyObscured += OnSocketNewlyObscured;
            }
        }

        GlobalMessenger.AddListener("ShipEnterQuantumMoon", OnShipEnterQuantumMoon);
        GlobalMessenger.AddListener("ShipExitQuantumMoon", OnShipExitQuantumMoon);
        GlobalMessenger.AddListener("ExitShip", OnPlayerExitShip);
        GlobalMessenger.AddListener("PlayerBlink", OnPlayerBlink);
        GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonChangeState);
    }

    protected void OnDestroy()
    {
        GlobalMessenger.RemoveListener("ShipEnterQuantumMoon", OnShipEnterQuantumMoon);
        GlobalMessenger.RemoveListener("ShipExitQuantumMoon", OnShipExitQuantumMoon);
        GlobalMessenger.RemoveListener("ExitShip", OnPlayerExitShip);
        GlobalMessenger.RemoveListener("PlayerBlink", OnPlayerBlink);
        GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonChangeState);
        foreach (var socket in sockets)
        {
            if (socket != null) socket.OnNewlyObscured -= OnSocketNewlyObscured;
        }

        DisableQuantumBehavior();
        SetShipInvulnerable(false);
    }

    void Update()
    {
        if (!shipOnQuantumMoon) return;

        // Boarding must immediately return full physics control to the player. Once they
        // step out again, suspension safely parents the ship to the moon.
        if (PlayerState.IsInsideShip())
        {
            DisableQuantumBehavior();
        }
        else if (!quantumBehaviorActive && IsPlayerClearOfExitBeam())
        {
            EnableQuantumBehavior();
        }

        // Other systems can alter this flag, so enforce it for the entire visit rather
        // than only on the entry frame.
        shipDamageController._invincible = true;
    }

    void OnShipEnterQuantumMoon()
    {
        if (shipOnQuantumMoon) return;

        shipOnQuantumMoon = true;
        previousShipInvulnerability = shipDamageController._invincible;
        SetShipInvulnerable(true);
        GhostInTheMachine.Instance.ModHelper.Console.WriteLine(
            $"Ship has entered the Quantum Moon on frame {Time.frameCount}.");
    }

    void OnShipExitQuantumMoon()
    {
        if (!shipOnQuantumMoon) return;

        DisableQuantumBehavior();
        shipOnQuantumMoon = false;
        SetShipInvulnerable(false);
        GhostInTheMachine.Instance.ModHelper.Console.WriteLine(
            $"Ship has exited the Quantum Moon on frame {Time.frameCount}.");
    }

    void OnPlayerExitShip()
    {
        quantumEnableFallbackTime = Time.time + TRACTOR_BEAM_EXIT_FALLBACK;
    }

    bool IsPlayerClearOfExitBeam()
    {
        // Vanilla activates this fluid only after ShipTractorBeamSwitch receives the
        // player's real trigger exit. Waiting for it avoids making the ship kinematic
        // while the player is still crossing that trigger.
        return shipTractorBeamFluid == null
            || shipTractorBeamFluid.IsVolumeActive()
            || Time.time >= quantumEnableFallbackTime;
    }

    void OnPlayerBlink()
    {
        // A blink guarantees that the move itself cannot be observed.
        if (quantumBehaviorActive && !PlayerState.IsInsideShip())
        {
            TryMoveToBiasedSocket();
        }
    }

    void OnQuantumMoonChangeState(OWRigidbody changedMoonBody)
    {
        if (changedMoonBody == moonBody && quantumBehaviorActive && !PlayerState.IsInsideShip())
        {
            // Surface geometry has just changed while the player was in darkness. Move
            // to a valid socket for the new state instead of risking terrain overlap at
            // the old latitude/longitude.
            TryMoveToBiasedSocket();
        }
    }

    void OnSocketNewlyObscured(QuantumSocket socket)
    {
        if (!quantumBehaviorActive || PlayerState.IsInsideShip() || !IsAvailableSocket(socket)) return;

        var visibility = socket.GetVisibilityObject();
        if (!visibility.IsIlluminated() && visibility.CheckPointInside(Locator.GetPlayerCamera().transform.position)) return;

        var playerPosition = Locator.GetPlayerTransform().position;
        var socketDistance = Vector3.Distance(socket.transform.position, playerPosition);
        var shipDistance = Vector3.Distance(shipBody.transform.position, playerPosition);
        if (socketDistance >= 50f || socketDistance >= shipDistance) return;

        // Match the shrine's 10% attraction probability, with the same guarantee that a
        // run of failed rolls eventually brings the object toward the player.
        if (Random.value <= 0.1f || failedNearbySocketRolls > 4)
        {
            failedNearbySocketRolls = 0;
            TryMoveToSocket(socket);
        }
        else
        {
            failedNearbySocketRolls++;
        }
    }

    void EnableQuantumBehavior()
    {
        quantumBehaviorActive = true;

        shipWasKinematic = shipBody.IsKinematic();
        if (!shipWasKinematic) shipBody.MakeKinematic();
        shipBody.transform.SetParent(moonBody.transform, true);

        socketOccupant.SetActivation(true);

        // Do not visibly teleport the ship as the player steps out. Reserving the nearest
        // socket is enough to keep the shrine/shuttle away until the first blink or moon
        // state change performs a proper quantum move.
        ReserveNearestAvailableSocket();

        // If the player left the ship without looking back at it, it is already valid
        // for the ship to enter the socket network immediately.
        if (!shipVisibilityTracker.IsVisible())
        {
            socketOccupant.ForceCollapse();
        }

        GhostInTheMachine.Instance.ModHelper.Console.WriteLine(
            $"Quantum behavior enabled on frame {Time.frameCount}.");
    }

    void DisableQuantumBehavior()
    {
        if (!quantumBehaviorActive) return;

        quantumBehaviorActive = false;
        socketOccupant.SetActivation(false);
        ReleaseSocket();
        if (shipBody != null)
        {
            shipBody.transform.SetParent(null, true);
            if (!shipWasKinematic) shipBody.MakeNonKinematic();
            shipBody.SetVelocity(moonBody.GetPointVelocity(shipBody.GetPosition()));
            shipBody.SetAngularVelocity(moonBody.GetAngularVelocity());
        }

        GhostInTheMachine.Instance.ModHelper.Console.WriteLine(
            $"Quantum behavior disabled on frame {Time.frameCount}.");
    }

    void ReserveNearestAvailableSocket()
    {
        var socket = sockets
            .Where(IsAvailableSocket)
            .OrderBy(candidate => (candidate.transform.position - shipBody.transform.position).sqrMagnitude)
            .FirstOrDefault();

        ReserveSocket(socket);
    }

    public bool TryMoveToBiasedSocket()
    {
        var previousSocket = occupiedSocket;

        var candidates = new List<QuantumSocket>();
        foreach (var socket in sockets)
        {
            if (socket != previousSocket && IsAvailableSocket(socket)) candidates.Add(socket);
        }

        if (candidates.Count == 0)
        {
            return false;
        }

        var targetSocket = ChoosePlayerBiasedSocket(candidates);
        return TryMoveToSocket(targetSocket);
    }

    bool TryMoveToSocket(QuantumSocket targetSocket)
    {
        // A blink can cause the moon to change state, so multiple collapse paths may
        // arrive in the same frame. Treat them as one quantum move.
        if (lastMoveFrame == Time.frameCount || !IsAvailableSocket(targetSocket)) return false;
        lastMoveFrame = Time.frameCount;

        ReleaseSocket();
        ReserveSocket(targetSocket);

        var shipTransform = shipBody.transform;
        var targetUp = (targetSocket.transform.position - moonBody.GetWorldCenterOfMass()).normalized;
        var targetRotation = Quaternion.FromToRotation(shipTransform.up, targetUp) * shipTransform.rotation;
        targetRotation = Quaternion.AngleAxis(Random.Range(0f, 360f), targetUp) * targetRotation;

        shipTransform.SetPositionAndRotation(
            targetSocket.transform.position + targetUp * SHIP_SOCKET_HEIGHT,
            targetRotation);

        if (!Physics.autoSyncTransforms) Physics.SyncTransforms();
        return true;
    }

    QuantumSocket ChoosePlayerBiasedSocket(List<QuantumSocket> candidates)
    {
        var playerPosition = Locator.GetPlayerTransform().position;
        var totalWeight = 0f;
        var weights = new float[candidates.Count];
        for (var i = 0; i < candidates.Count; i++)
        {
            var distanceSquared = (candidates[i].transform.position - playerPosition).sqrMagnitude;
            weights[i] = 1f / (distanceSquared + 400f);
            totalWeight += weights[i];
        }

        var roll = Random.value * totalWeight;
        for (var i = 0; i < candidates.Count; i++)
        {
            roll -= weights[i];
            if (roll <= 0f) return candidates[i];
        }
        return candidates[candidates.Count - 1];
    }

    bool IsAvailableSocket(QuantumSocket socket)
    {
        return socket != null && socket.IsActive() && !socket.IsOccupied();
    }

    void ReserveSocket(QuantumSocket socket)
    {
        if (socket == null) return;

        occupiedSocket = socket;
        occupiedSocket.SetQuantumObject(socketOccupant);
    }

    void ReleaseSocket()
    {
        if (occupiedSocket != null && occupiedSocket.GetOccupant() == socketOccupant)
        {
            occupiedSocket.ReleaseQuantumObject();
        }
        occupiedSocket = null;
    }

    void SetShipInvulnerable(bool invulnerable)
    {
        if (shipDamageController == null) return;
        shipDamageController._invincible = invulnerable || previousShipInvulnerability;
    }

    void OnGUI()
    {
        if (!GhostInTheMachine.Instance.DebugModeEnabled) return;
        if (!Locator.GetQuantumMoon().IsPlayerInside()) return;
        GUILayout.Label($"Quantum Ship Manager");
        GUILayout.Label($"  Ship on Quantum Moon: {shipOnQuantumMoon}");
        GUILayout.Label($"  Quantum Behavior Active: {quantumBehaviorActive}");
        GUILayout.Label($"  Occupied Socket: {(occupiedSocket != null ? occupiedSocket.name : "None")}");
        GUILayout.Label($"  Ship Kinematic: {shipBody.IsKinematic()}");
    }
}
