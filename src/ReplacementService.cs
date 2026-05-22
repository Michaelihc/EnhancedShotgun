using System.Linq;
using EnhancedShotgun.HintDisplay;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace EnhancedShotgun;

public sealed class ReplacementService
{
    private readonly PluginConfig _config;
    private readonly ShotgunTracker _tracker;
    private readonly Localization _localization;
    private readonly IHintDisplayProvider _hints;
    private bool _creatingReplacement;

    public ReplacementService(PluginConfig config, ShotgunTracker tracker, Localization localization, IHintDisplayProvider hints)
    {
        _config = config;
        _tracker = tracker;
        _localization = localization;
        _hints = hints;
    }

    public void Enable()
    {
        ServerEvents.ItemSpawning += OnItemSpawning;
        ServerEvents.PickupCreated += OnPickupCreated;
        PlayerEvents.ReceivingLoadout += OnReceivingLoadout;
        PlayerEvents.PickingUpItem += OnPickingUpItem;
        PlayerEvents.PickedUpItem += OnPickedUpItem;
        Scp914Events.ProcessedInventoryItem += OnScp914ProcessedInventoryItem;
        Scp914Events.ProcessedPickup += OnScp914ProcessedPickup;
    }

    public void Disable()
    {
        ServerEvents.ItemSpawning -= OnItemSpawning;
        ServerEvents.PickupCreated -= OnPickupCreated;
        PlayerEvents.ReceivingLoadout -= OnReceivingLoadout;
        PlayerEvents.PickingUpItem -= OnPickingUpItem;
        PlayerEvents.PickedUpItem -= OnPickedUpItem;
        Scp914Events.ProcessedInventoryItem -= OnScp914ProcessedInventoryItem;
        Scp914Events.ProcessedPickup -= OnScp914ProcessedPickup;
    }

    private void OnItemSpawning(ItemSpawningEventArgs ev)
    {
        if (ev.ItemType == ItemType.GunShotgun)
        {
            ev.ItemType = ItemType.GunAK;
        }
    }

    private void OnPickupCreated(PickupCreatedEventArgs ev)
    {
        if (_creatingReplacement || ev.Pickup.Type != ItemType.GunShotgun || _tracker.IsSpecial(ev.Pickup))
        {
            return;
        }

        if (_tracker.IsCreatingSpecial)
        {
            _tracker.Mark(ev.Pickup.Serial);
            return;
        }

        ReplacePickupWithAk(ev.Pickup, null);
    }

    private void OnReceivingLoadout(PlayerReceivingLoadoutEventArgs ev)
    {
        int replacementCount = 0;

        for (int i = 0; i < ev.Items.Count; i++)
        {
            if (ev.Items[i] != ItemType.GunShotgun)
            {
                continue;
            }

            ev.Items[i] = GetLoadoutReplacement(ev.Player);
            replacementCount++;
        }

        if (replacementCount > 0)
        {
            MessageKey message = GetLoadoutReplacement(ev.Player) == ItemType.GunA7
                ? MessageKey.NormalShotgunReplacedWithA7
                : MessageKey.NormalShotgunReplacedWithAk;

            ShowReplacementHint(ev.Player, message);
        }
    }

    private void OnPickingUpItem(PlayerPickingUpItemEventArgs ev)
    {
        if (ev.Pickup.Type != ItemType.GunShotgun || _tracker.IsSpecial(ev.Pickup))
        {
            return;
        }

        ev.IsAllowed = false;
        ReplacePickupWithAk(ev.Pickup, ev.Player);
    }

    private void OnPickedUpItem(PlayerPickedUpItemEventArgs ev)
    {
        if (ev.Item.Type != ItemType.GunShotgun || _tracker.IsSpecial(ev.Item))
        {
            return;
        }

        ev.Player.RemoveItem(ev.Item);
        ev.Player.AddItem(ItemType.GunAK);
        ShowReplacementHint(ev.Player, MessageKey.NormalShotgunReplacedWithAk);
    }

    private void OnScp914ProcessedInventoryItem(Scp914ProcessedInventoryItemEventArgs ev)
    {
        if (ev.Item.Type != ItemType.GunShotgun || _tracker.IsSpecial(ev.Item))
        {
            return;
        }

        ev.Player.RemoveItem(ev.Item);
        ev.Player.AddItem(ItemType.GunAK);
        ShowReplacementHint(ev.Player, MessageKey.NormalShotgunReplacedWithAk);
    }

    private void OnScp914ProcessedPickup(Scp914ProcessedPickupEventArgs ev)
    {
        if (ev.Pickup?.Type != ItemType.GunShotgun || _tracker.IsSpecial(ev.Pickup))
        {
            return;
        }

        ReplacePickupWithAk(ev.Pickup, null);
    }

    private void ReplacePickupWithAk(Pickup pickup, Player? player)
    {
        Vector3 position = pickup.Position;
        Quaternion rotation = pickup.Rotation;
        Vector3 scale = pickup.Transform.localScale;

        pickup.Destroy();

        _creatingReplacement = true;
        try
        {
            Pickup? ak = Pickup.Create(ItemType.GunAK, position, rotation, scale);
            ak?.Spawn();
        }
        finally
        {
            _creatingReplacement = false;
        }

        if (player != null)
        {
            ShowReplacementHint(player, MessageKey.NormalShotgunReplacedWithAk);
        }
    }

    private void ShowReplacementHint(Player player, MessageKey message)
    {
        _hints.Show(player, HintTags.Replacement, _localization.Get(player, message), _config.ReplacementHintDuration);
    }

    private static ItemType GetLoadoutReplacement(Player player)
    {
        return player.Role == RoleTypeId.ChaosMarauder ? ItemType.GunA7 : ItemType.GunAK;
    }
}
