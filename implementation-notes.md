# Implementation Notes

## Decisions

- This plugin uses LabAPI events instead of Harmony. Relevant official API surfaces are under `..\.references\LabAPI\LabApi\Events\Handlers` and event args under `..\.references\LabAPI\LabApi\Events\Arguments`.
- Special shotgun identity is tracked by item/pickup serial. The GR-18 spawned pickup serial is marked special, and the same serial is expected to transfer into inventory when picked up.
- The GR-18 spawn wraps `Pickup.Create` in a short special-creation guard because `PickupCreated` fires before `Pickup.Create` returns the serial to the spawner.
- Normal shotgun suppression is layered:
  - `ServerEvents.ItemSpawning` rewrites map spawn type from `GunShotgun` to `GunAK`.
  - `ServerEvents.PickupCreated` replaces unexpected shotgun pickups with AK pickups.
  - `PlayerEvents.ReceivingLoadout` rewrites loadout shotguns. Chaos Marauder loadouts receive `GunA7`; other loadout shotguns receive `GunAK`.
  - `PlayerEvents.PickingUpItem` and `PickedUpItem` prevent late pickup paths.
  - `Scp914Events.ProcessedInventoryItem` and `ProcessedPickup` replace SCP-914 shotgun outputs.
- GR-18 is identified as `RoomName.LczGlassroom`. GR-18 door setup and enhanced shotgun spawning run from `ServerEvents.WaitingForPlayers`, after facility objects and room door registration are ready.
- The door permission policy uses `new KeycardLevels(3, 3, 3).Permissions` with `requireAll: true` and `bypass2176: true`. Native permission and SCP-2176 behavior are from `..\.references\Decompiled\DedicatedServer\Assembly-CSharp\Interactables\Interobjects\DoorUtils\DoorPermissionsPolicy.cs` and `..\.references\Decompiled\DedicatedServer\Assembly-CSharp\InventorySystem\Items\ThrowableProjectiles\Scp2176Projectile.cs`.
- Door interaction is additionally checked in `PlayerEvents.InteractingDoor` so SCP/override access and non-O5 access are rejected for the protected GR-18 doors.
- Door damage is cancelled through `ServerEvents.DoorDamaging` for the protected GR-18 doors.
- Explosion door destruction is disabled through `ServerEvents.ExplosionSpawning` when the explosion radius reaches a protected GR-18 door. Protected breakable doors also set `IgnoreDamageSources` to all native `DoorDamageType` values. This covers grenades, SCP-018, SCP-096 pry-style damage, particle disruptor, and similar native door-damage paths.
- Damage is multiplied in `PlayerEvents.Hurting` when the damage handler is `FirearmDamageHandler` and its firearm serial is special. The native damage flow runs the LabAPI hurting event before `ApplyDamage` in `..\.references\Decompiled\DedicatedServer\Assembly-CSharp\PlayerStatsSystem\PlayerStats.cs`.
- All player-facing text goes through `EnhancedShotgun.HintDisplay.IHintDisplayProvider` with stable tags. RueI is used when loaded; missing RueI uses a null provider by default, with `hint_display.compatibility_mode: true` enabling throttled vanilla `SendHint` inside the provider only.

## Tradeoffs

- The pickup scale is server-spawned through LabAPI `Pickup.Create(type, position, rotation, scale)`. Held client viewmodels/worldmodels are not fully controlled by server-side LabAPI without deeper client-side or Harmony work.
- `language: ""` falls back to Chinese because no stable per-client language property was found in the LabAPI `Player` wrapper or current dedicated-server decompile. The config can force English or Chinese.
- Map-generated shotgun replacement has no player to message, so only player-associated replacement paths send hints.
- Vanilla hints cannot be removed by tag, so the compatibility provider only sends short throttled messages. RueI remains the intended provider for composed, replaceable player text.

## Verification Log

- Built with `dotnet build -c Release --no-restore /p:UseSharedCompilation=false`.
- Build output: `bin\Release\net48\EnhancedShotgun.dll`.
- The local .NET 10 SDK intermittently crashed when using restore/shared compilation; disabling shared compilation produced a clean build.
- Visible 7777 LocalAdmin server started and confirmed LabAPI loaded/enabled `EnhancedShotgun`.
- First live startup exposed a named GR-18 door lookup timing issue during map generation. The plugin now waits until `WaitingForPlayers` and falls back to all doors registered to `RoomName.LczGlassroom`.
- After the hint-provider refactor, `rg SendHint src` only reports `VanillaCompatibilityHintProvider`.
- Deployed to `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\7777\EnhancedShotgun.dll` and updated `%APPDATA%\SCP Secret Laboratory\LabAPI\configs\7777\EnhancedShotgun\config.yml`.
- Restarted visible `LocalAdmin.exe 7777` from the dedicated-server directory. Log `LocalAdmin Log 2026-05-22 12.28.48.txt` confirms RueI 3.1.2 enabled first, EnhancedShotgun detected RueI for hints, EnhancedShotgun enabled, and the enhanced shotgun spawned in GR-18.

## Open Manual Checks

- Start/restart the visible 7777 test server and confirm LabAPI loads `EnhancedShotgun.dll`.
- Confirm GR-18 contains exactly one doubled shotgun pickup.
- Confirm non-O5 keycards, SCP-2176/Ghostlight, and SCP pry/damage cannot open or break GR-18.
- Confirm O5-level keycard can open GR-18.
- Confirm Chaos Marauder/normal shotgun paths receive AK and see a replacement hint where a player is known.
- Confirm enhanced shotgun pickup hint appears and firearm damage/ammo behavior match the config.
