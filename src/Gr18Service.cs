using System.Linq;
using System.Collections.Generic;
using EnhancedShotgun.HintDisplay;
using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace EnhancedShotgun;

public sealed class Gr18Service
{
    private readonly PluginConfig _config;
    private readonly ShotgunTracker _tracker;
    private readonly Localization _localization;
    private readonly IHintDisplayProvider _hints;
    private readonly HashSet<DoorVariant> _protectedDoors = new();
    private bool _spawnedThisRound;
    private bool _configuredDoorsThisRound;

    private static readonly DoorPermissionFlags RequiredO5Permissions = new KeycardLevels(3, 3, 3).Permissions;

    public Gr18Service(PluginConfig config, ShotgunTracker tracker, Localization localization, IHintDisplayProvider hints)
    {
        _config = config;
        _tracker = tracker;
        _localization = localization;
        _hints = hints;
    }

    public void Enable()
    {
        ServerEvents.MapGenerated += OnMapGenerated;
        ServerEvents.WaitingForPlayers += OnWaitingForPlayers;
        ServerEvents.RoundRestarted += OnRoundRestarted;
        PlayerEvents.InteractingDoor += OnInteractingDoor;
        ServerEvents.DoorDamaging += OnDoorDamaging;
        ServerEvents.ExplosionSpawning += OnExplosionSpawning;
    }

    public void Disable()
    {
        ServerEvents.MapGenerated -= OnMapGenerated;
        ServerEvents.WaitingForPlayers -= OnWaitingForPlayers;
        ServerEvents.RoundRestarted -= OnRoundRestarted;
        PlayerEvents.InteractingDoor -= OnInteractingDoor;
        ServerEvents.DoorDamaging -= OnDoorDamaging;
        ServerEvents.ExplosionSpawning -= OnExplosionSpawning;
        _protectedDoors.Clear();
    }

    private void OnMapGenerated(MapGeneratedEventArgs ev)
    {
        _spawnedThisRound = false;
        _configuredDoorsThisRound = false;
    }

    private void OnWaitingForPlayers()
    {
        if (!_configuredDoorsThisRound)
        {
            SetupDoors();
            _configuredDoorsThisRound = true;
        }

        if (!_spawnedThisRound)
        {
            SpawnEnhancedShotgun();
            _spawnedThisRound = true;
        }
    }

    private void OnRoundRestarted()
    {
        _protectedDoors.Clear();
        _spawnedThisRound = false;
        _configuredDoorsThisRound = false;
    }

    private void SetupDoors()
    {
        _protectedDoors.Clear();

        Door? gr18Gate = Door.Get(DoorName.LczGr18Gate);
        Door? gr18InnerDoor = Door.Get(DoorName.LczGr18Inner);

        ProtectDoor(gr18Gate);

        if (_config.LockGr18InnerDoor)
        {
            ProtectDoor(gr18InnerDoor);
        }

        Room? gr18 = Room.Get(RoomName.LczGlassroom).FirstOrDefault();
        if (gr18 != null)
        {
            foreach (Door door in gr18.Doors)
            {
                ProtectDoor(door);
            }
        }

        if (_protectedDoors.Count == 0)
        {
            Logger.Warn("Unable to find any GR-18 doors to protect.");
        }
    }

    private void ProtectDoor(Door? door)
    {
        if (door == null)
        {
            return;
        }

        door.IsOpened = false;
        door.Base.RequiredPermissions = new DoorPermissionsPolicy(RequiredO5Permissions, requireAll: true, bypass2176: true);

        if (door is LabApi.Features.Wrappers.BreakableDoor breakableDoor)
        {
            breakableDoor.IgnoreDamageSources = (DoorDamageType)byte.MaxValue;
        }

        _protectedDoors.Add(door.Base);
    }

    private void SpawnEnhancedShotgun()
    {
        Room? gr18 = Room.Get(RoomName.LczGlassroom).FirstOrDefault();
        if (gr18 == null)
        {
            Logger.Warn("Unable to find LCZ GR-18 room for enhanced shotgun spawn.");
            return;
        }

        Vector3 spawnPosition = gr18.Position + Vector3.up;
        Pickup? pickup;
        _tracker.BeginSpecialCreation();
        try
        {
            pickup = Pickup.Create(
                ItemType.GunShotgun,
                spawnPosition,
                gr18.Rotation,
                Vector3.one * _config.PickupScale);
        }
        finally
        {
            _tracker.EndSpecialCreation();
        }

        if (pickup == null)
        {
            Logger.Warn("Failed to create enhanced shotgun pickup.");
            return;
        }

        _tracker.Mark(pickup.Serial);
        pickup.Spawn();
        Logger.Info($"Spawned enhanced shotgun in GR-18 at {spawnPosition} with serial {pickup.Serial}.");
    }

    private void OnInteractingDoor(PlayerInteractingDoorEventArgs ev)
    {
        if (!IsProtectedDoor(ev.Door))
        {
            return;
        }

        ev.CanOpen = HasHeldO5Access(ev.Player);
        if (ev.CanOpen)
        {
            return;
        }

        _hints.Show(
            ev.Player,
            HintTags.Gr18Denied,
            _localization.Get(ev.Player, MessageKey.Gr18NeedsO5),
            _config.ReplacementHintDuration);
        ev.Door.PlayPermissionDeniedAnimation();
    }

    private void OnDoorDamaging(DoorDamagingEventArgs ev)
    {
        if (IsProtectedDoor(ev.Door))
        {
            ev.IsAllowed = false;
        }
    }

    private void OnExplosionSpawning(ExplosionSpawningEventArgs ev)
    {
        if (ev.DestroyDoors && IsNearProtectedDoor(ev.Position, ev.Settings.MaxRadius))
        {
            ev.DestroyDoors = false;
        }
    }

    private bool IsProtectedDoor(Door? door)
    {
        if (door == null)
        {
            return false;
        }

        return _protectedDoors.Contains(door.Base);
    }

    private bool IsNearProtectedDoor(Vector3 position, float radius)
    {
        float sqrRadius = radius * radius;

        foreach (DoorVariant protectedDoor in _protectedDoors)
        {
            if (protectedDoor != null && (protectedDoor.transform.position - position).sqrMagnitude <= sqrRadius)
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasHeldO5Access(Player player)
    {
        if (player.Role == RoleTypeId.Overwatch)
        {
            return false;
        }

        return player.CurrentItem is KeycardItem keycard
            && keycard.Permissions.HasFlagAll(RequiredO5Permissions);
    }
}
