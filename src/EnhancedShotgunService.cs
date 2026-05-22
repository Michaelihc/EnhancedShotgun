using EnhancedShotgun.HintDisplay;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;

namespace EnhancedShotgun;

public sealed class EnhancedShotgunService
{
    private readonly PluginConfig _config;
    private readonly ShotgunTracker _tracker;
    private readonly Localization _localization;
    private readonly IHintDisplayProvider _hints;

    public EnhancedShotgunService(PluginConfig config, ShotgunTracker tracker, Localization localization, IHintDisplayProvider hints)
    {
        _config = config;
        _tracker = tracker;
        _localization = localization;
        _hints = hints;
    }

    public void Enable()
    {
        PlayerEvents.PickedUpItem += OnPickedUpItem;
        PlayerEvents.ChangedItem += OnChangedItem;
        PlayerEvents.ShootingWeapon += OnShootingWeapon;
        PlayerEvents.ShotWeapon += OnShotWeapon;
        PlayerEvents.DryFiringWeapon += OnDryFiringWeapon;
        PlayerEvents.ReloadingWeapon += OnReloadingWeapon;
        PlayerEvents.Hurting += OnHurting;
    }

    public void Disable()
    {
        PlayerEvents.PickedUpItem -= OnPickedUpItem;
        PlayerEvents.ChangedItem -= OnChangedItem;
        PlayerEvents.ShootingWeapon -= OnShootingWeapon;
        PlayerEvents.ShotWeapon -= OnShotWeapon;
        PlayerEvents.DryFiringWeapon -= OnDryFiringWeapon;
        PlayerEvents.ReloadingWeapon -= OnReloadingWeapon;
        PlayerEvents.Hurting -= OnHurting;
    }

    private void OnPickedUpItem(PlayerPickedUpItemEventArgs ev)
    {
        if (!_tracker.IsSpecial(ev.Item))
        {
            return;
        }

        _tracker.Mark(ev.Item.Serial);
        Refill(ev.Item as ShotgunFirearm);
        _hints.Show(
            ev.Player,
            HintTags.Pickup,
            _localization.Get(ev.Player, MessageKey.PickedUpEnhancedShotgun),
            _config.PickupHintDuration);
    }

    private void OnChangedItem(PlayerChangedItemEventArgs ev)
    {
        if (_tracker.IsSpecial(ev.NewItem))
        {
            Refill(ev.NewItem as ShotgunFirearm);
        }
    }

    private void OnShootingWeapon(PlayerShootingWeaponEventArgs ev)
    {
        if (_tracker.IsSpecial(ev.FirearmItem))
        {
            Refill(ev.FirearmItem as ShotgunFirearm);
        }
    }

    private void OnShotWeapon(PlayerShotWeaponEventArgs ev)
    {
        if (_tracker.IsSpecial(ev.FirearmItem))
        {
            Refill(ev.FirearmItem as ShotgunFirearm);
        }
    }

    private void OnDryFiringWeapon(PlayerDryFiringWeaponEventArgs ev)
    {
        if (!_tracker.IsSpecial(ev.FirearmItem))
        {
            return;
        }

        ev.IsAllowed = false;
        Refill(ev.FirearmItem as ShotgunFirearm);
    }

    private void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
    {
        if (!_tracker.IsSpecial(ev.FirearmItem))
        {
            return;
        }

        ev.IsAllowed = false;
        Refill(ev.FirearmItem as ShotgunFirearm);
    }

    private void OnHurting(PlayerHurtingEventArgs ev)
    {
        if (ev.DamageHandler is not FirearmDamageHandler firearmDamage
            || firearmDamage.WeaponType != ItemType.GunShotgun
            || firearmDamage.Firearm == null
            || !_tracker.IsSpecial(firearmDamage.Firearm.ItemSerial))
        {
            return;
        }

        firearmDamage.Damage *= _config.DamageMultiplier;
    }

    private static void Refill(ShotgunFirearm? shotgun)
    {
        if (shotgun == null)
        {
            return;
        }

        int chamberMax = shotgun.ChamberMax;
        if (chamberMax <= 0)
        {
            chamberMax = 2;
        }

        shotgun.StoredAmmo = shotgun.MaxAmmo;
        shotgun.ChamberedAmmo = chamberMax;
        shotgun.CockedChambers = chamberMax;
    }
}
