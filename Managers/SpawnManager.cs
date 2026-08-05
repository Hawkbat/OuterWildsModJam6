using GhostInTheMachine.Controllers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GhostInTheMachine.Constants.PersistentConditions;


namespace GhostInTheMachine.Managers;

public class SpawnManager : ManagerBase<SpawnManager>
{
    // Sun station tower opens 5.33 min
    // High energy lab accessible 5.833 min
    // High energy lab inaccessible 6.167 min
    // Stepping Stone District buried 11.0833 min
    // Last sun station warp 11.3 min
    // Sun station destroyed 11.5 min
    // Quantum Tower exits white hole 18.0833 min
    // Interloper destroyed 19.75 min
    // Last ATP warp 19.967 min
    // End Times 20.583 min
    // Supernova 22.0 min
    // Loop ends 22.667 min

    const float SUN_STATION_DESTRUCTION_TIME = 11.5f;
    const float INTERLOPER_DESTRUCTION_TIME = 19.75f;
    const float SUPERNOVA_TIME = 22f;

    static readonly SpawnGroup[] SPAWN_GROUPS =
    [
        new(
            [STATUE_GABBRO, STATUE_WORKSHOP, STATUE_PROBE],
            [
                new(20.24f,
                    new("StatueIsland_Body", new(0.8378989f, 10.73179f, 6.672981f), new(0f, 330f, 0f)) { fuel = 13f, oxygen = 180f, health = 13f, hasStaff = true },
                    new("WhiteHole_Body") { destroyed = true }),
                new(17.12f,
                    new("StatueIsland_Body", new(-38f, 0.4f, -74f), new(11f, 64f, 0f)) { fuel = 24f, oxygen = 240f, health = 13f, hasStaff = true },
                    new("WhiteHole_Body") { destroyed = true }),
                new(15.13f,
                    new("GabbroIsland_Body", new(-12.3f, 0.72f, 33.8f), new(355f, 5f, 0f)) { fuel = 44f, oxygen = 450f, health = 27f, hasStaff = true },
                    new("WhiteHole_Body") { destroyed = true, destroyDelay = 10f }),
            ]
        ),
        new(
            [STATUE_FORGE, STATUE_ATP],
            [
                new(13.14f,
                    new("BrittleHollow_Body", new(1f, 280f, -30f), new(0f, 324f, 180f)) { fuel = 33f, oxygen = 230f, health = 44f },
                    new("BrittleHollow_Body", new(2.6f, 170f, 62f), new(355f, 105f, 12f)) { outOfFuel = true }),
                new(11.11f,
                    new("BrittleHollow_Body", new(13.8f, 281f, 18.7f), new(0f, 0f, 180f)) { fuel = 38f, oxygen = 320f, health = 44f },
                    new("BrittleHollow_Body", new(2.6f, 170f, 62f), new(355f, 105f, 12f)) { outOfFuel = true }),
            ]
        ) {
            warpRecieversToRecharge = ["BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_District4/Interactables_HangingCity_District4/Prefab_NOM_WarpReceiver"],
        },
        new(
            [STATUE_SS_LOWER, STATUE_SS_UPPER],
            [
                new(8.64f,
                    new("TowerTwin_Body", new(-2.4f, 171.8f, 3.5f), new(0f, 95f, 0f)) { fuel = 64f, oxygen = 310f, health = 73f },
                    new("CaveTwin_Body", new(130.3f, 75.75f, 72.4f), new(343f, 319f, 300f))),
                new(6.32f,
                    new("TowerTwin_Body", new(0.8f, 3.8f, -123f), new(63f, 270f, 270f)) { fuel = 85f, oxygen = 400f, health = 91f },
                    new("CaveTwin_Body", new(130.3f, 75.75f, 72.4f), new(343f, 319f, 300f))),
            ]
        ),
        new(
            [SOLANUM_MASK_FIX],
            [
                new (4.05f,
                    new("Moon_Body", new(-26.4f, 43.5f, -45.5f), new(311f, 164f, 32f)) { fuel = 98f, oxygen = 450f, health = 100f },
                    new("Moon_Body", new(-28.6f, 54.5f, -38f), new(305f, 338f, 38f))),
            ]
        ),
    ];

    PlayerResources playerResources;

    protected override void Awake()
    {
        base.Awake();
        playerResources = FindObjectOfType<PlayerResources>();
    }

    public void DoInitialSpawn()
    {
        var (spawn, spawnGroup) = CalculateSpawn();
        if (spawn == null)
        {
            // No custom spawn; fall back to default spawn
            return;
        }

        var initialTimeLoopTime = TimeLoop.GetSecondsElapsed();

        // Set time loop time to target time minus 1 minute
        TimeLoop.SetSecondsRemaining((SUPERNOVA_TIME - spawn.loopTime) * 60f + 60f);
        if (spawn.ship.destroyed)
        {
            var shipDestroyer = Locator.GetShipTransform().gameObject.AddComponent<ShipDestructionController>();
            shipDestroyer.SetTargetTime(spawn.loopTime * 60f + spawn.ship.destroyDelay);
        }
        if (spawn.ship.outOfFuel)
        {
            var shipResources = FindObjectOfType<ShipResources>();
            shipResources.SetFuel(50f * (10f + 60f));
            var fuelTank = FindObjectOfType<ShipFuelTankComponent>();
            fuelTank.SetDamaged(true);
        }
        if (spawn.loopTime >= SUN_STATION_DESTRUCTION_TIME)
        {
            Locator.GetAstroObject(AstroObject.Name.SunStation).gameObject.SetActive(false);
        }
        if (spawn.loopTime >= INTERLOPER_DESTRUCTION_TIME)
        {
            Locator.GetAstroObject(AstroObject.Name.Comet).gameObject.SetActive(false);
        }


        var fragments = new List<FragmentIntegrity>(FindObjectsOfType<FragmentIntegrity>().Where(f => !f.GetIgnoreMeteorDamage()));
        var frac = TimeLoop.GetFractionElapsed();
        var totalDamage = frac * fragments.Sum(frag => frag.GetIntegrity());
        while (totalDamage > 0f && fragments.Count > 0)
        {
            var instanceDamage = Mathf.Min(totalDamage, Random.Range(20f, 80f));
            var frag = fragments[Random.Range(0, fragments.Count)];
            frag.AddDamage(instanceDamage);
            totalDamage -= instanceDamage;
            if (frag.GetIntegrity() <= 0f)
            {
                fragments.Remove(frag);
            }
        }

        GameObject.Find("TimberHearth_Body/Sector_TH/Sector_Village/Volumes_Village/MusicVolume_Village").GetComponent<VillageMusicVolume>().Deactivate();

        if (spawnGroup.warpRecieversToRecharge != null)
        {
            foreach (var path in spawnGroup.warpRecieversToRecharge)
            {
                var warpReceiver = GameObject.Find(path).GetComponent<NomaiWarpReceiver>();
                warpReceiver._returnOnEntry = true;
                warpReceiver._returnGlowFadeController.FadeTo(0.5f, 5f);

                var warpTransmitter = FindObjectsOfType<NomaiWarpTransmitter>().FirstOrDefault(trans => trans._targetReceiver == warpReceiver);
                warpReceiver._returnPlatform = warpTransmitter;
            }
        }

        if (spawn.player.hasSuit)
        {
            Locator.GetPlayerSuit().SuitUp(false, true, true);
        }
        if (spawn.player.hasStaff)
        {
            StaffManager.Instance.GivePlayerStaff();
        }

        // Fast forward remaining minute to target time to let debris settle; player will wake up after
        FastForwardManager.Instance.SetDisplayTimes(initialTimeLoopTime, spawn.loopTime * 60f);
        FastForwardManager.Instance.SetTargetTime(spawn.loopTime * 60f);
        StartCoroutine(DoInitialSpawnCoroutine(spawn));
    }

    IEnumerator DoInitialSpawnCoroutine(Spawn spawn)
    {
        InvincibilityManager.Instance.PushInvincibility();
        for (int i = 0; i < 20; i++)
        {
            WarpBody(Locator.GetShipBody(), spawn.ship, 4f);
            WarpBody(Locator.GetPlayerBody(), spawn.player, 1f);
            yield return new WaitForFixedUpdate();
        }
        while (FastForwardManager.Instance.IsFastForwarding()) yield return null;
        for (int i = 0; i < 20; i++)
        {
            WarpBody(Locator.GetShipBody(), spawn.ship, 4f);
            WarpBody(Locator.GetPlayerBody(), spawn.player, 1f);
            yield return new WaitForFixedUpdate();
        }
        InvincibilityManager.Instance.PopInvincibility();

        playerResources._currentFuel = spawn.player.fuel;
        playerResources._currentOxygen = spawn.player.oxygen;
        playerResources._currentHealth = spawn.player.health;

        // Mark ship on HUD since the player theoretically entered the ship even though they technically haven't yet
        PlayerState._hasPlayerEnteredShip = true;
        var shipHudMarker = FindObjectOfType<ShipHUDMarker>();
        shipHudMarker.RefreshOwnVisibility();
        shipHudMarker.gameObject.GetComponent<MapMarker>().enabled = true;
    }

    (Spawn, SpawnGroup) CalculateSpawn()
    {
        foreach (var group in SPAWN_GROUPS)
        {
            var conditionsMetCount = group.conditions.Count(cond => PlayerData.PersistentConditionExists(cond) && PlayerData.GetPersistentCondition(cond));
            var allConditionsMet = conditionsMetCount == group.conditions.Length;
            if (allConditionsMet) continue;
            var spawn = group.spawns[conditionsMetCount];
            return (spawn, group);
        }
        return (null, null);
    }

    void WarpBody(OWRigidbody body, SpawnPoint spawn, float offset)
    {
        var targetBody = GameObject.Find(spawn.parentPath).GetAttachedOWRigidbody();
        var worldRot = targetBody.transform.rotation * Quaternion.Euler(spawn.rot);
        var worldPos = targetBody.transform.TransformPoint(spawn.pos) + worldRot * (Vector3.up * offset);
        body.WarpToPositionRotation(worldPos, worldRot);
        body.SetVelocity(targetBody.GetPointVelocity(worldPos));
    }

    public class SpawnGroup(string[] conditions, Spawn[] spawns)
    {
        public readonly string[] conditions = conditions;
        public readonly Spawn[] spawns = spawns;
        public string[] warpRecieversToRecharge;
    }

    public class Spawn(float loopTime, PlayerSpawnPoint player, ShipSpawnPoint ship)
    {
        public readonly float loopTime = loopTime;
        public readonly PlayerSpawnPoint player = player;
        public readonly ShipSpawnPoint ship = ship;
    }

    public abstract class SpawnPoint(string parentPath, Vector3 pos, Vector3 rot)
    {
        public readonly string parentPath = parentPath;
        public readonly Vector3 pos = pos;
        public readonly Vector3 rot = rot;
    }

    public class PlayerSpawnPoint(string parentPath, Vector3 pos, Vector3 rot) : SpawnPoint(parentPath, pos, rot)
    {
        public bool hasSuit = true;
        public bool hasStaff = false;
        public float fuel = 100f;
        public float oxygen = 450f;
        public float health = 100f;
    }

    public class ShipSpawnPoint(string parentPath, Vector3 pos = default, Vector3 rot = default) : SpawnPoint(parentPath, pos, rot)
    {
        public bool destroyed = false;
        public float destroyDelay = 0f;
        public bool outOfFuel = false;
    }
}
