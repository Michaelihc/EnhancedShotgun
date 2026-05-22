# EnhancedShotgun

<p align="center">
  <a href="#english"><strong>English</strong></a>
  <span> | </span>
  <a href="#chinese"><strong>中文</strong></a>
</p>

## English

EnhancedShotgun is a LabAPI plugin for SCP: Secret Laboratory. It disables normal shotgun acquisition and adds one special enhanced shotgun in LCZ GR-18.

Features:

- Normal shotgun map spawns, pickup creation, player loadouts, SCP-914 processed inventory items, and SCP-914 processed pickups are replaced with `GunAK`.
- Chaos Marauder loadout shotguns are replaced with `GunA7` instead of `GunAK`.
- Players receive a localized hint when a normal shotgun is replaced.
- One enhanced shotgun spawns in LCZ GR-18 each round.
- The enhanced shotgun pickup is scaled to `2x` by default.
- Picking up the enhanced shotgun shows a localized message. English: `You picked up the ultimate shotgun. It deals four times damage.`
- The enhanced shotgun auto-refills its chamber and tube directly without granting reserve ammo.
- The enhanced shotgun deals `4x` firearm damage.
- The GR-18 gate requires all tier-3 keycard permission groups, equivalent to O5-level access.
- GR-18 access blocks SCP-2176/Ghostlight opening and cancels door damage/pry attempts, including grenade/SCP-018-style door damage where the base game routes it through door damage.
- Player text uses the shared hint-provider pattern. RueI is preferred when loaded; without RueI, hints are hidden unless vanilla compatibility mode is enabled.

Configuration is stored in:

`%APPDATA%\SCP Secret Laboratory\LabAPI\configs\7777\EnhancedShotgun\config.yml`

Config fields:

- `is_enabled`: enables or disables the plugin.
- `language`: `""` uses the default fallback, `"cn"` forces Chinese, and `"en"` forces English.
- `hint_display.compatibility_mode`: when `false`, missing RueI uses a null provider and no hints are shown. When `true`, missing RueI falls back to short throttled vanilla hints.
- `damage_multiplier`: enhanced shotgun damage multiplier. Default: `4`.
- `pickup_scale`: enhanced shotgun pickup model scale. Default: `2`.
- `pickup_hint_duration`: enhanced pickup hint duration in seconds.
- `replacement_hint_duration`: normal shotgun replacement hint duration in seconds.
- `lock_gr18_inner_door`: also applies the O5-only rule to the GR-18 inner door.

Known limitations:

- SCP:SL plugins are server-side, so this plugin reliably scales the world pickup model. Held first-person/third-person shotgun model scaling is client-controlled by the base game and is not fully server-authoritative.
- Normal shotgun replacements with no acting player, such as map-generated pickups, cannot show a player hint because there is no recipient.

## Chinese

EnhancedShotgun 是一个 SCP: Secret Laboratory 的 LabAPI 插件。它会禁用普通散弹枪获取方式，并在 LCZ 的 GR-18 中生成一把特殊强化散弹枪。

功能：

- 普通散弹枪的地图生成、拾取物创建、玩家出生装备、SCP-914 背包物品加工结果、SCP-914 地面物品加工结果会替换为 `GunAK`。
- 混沌分裂者掠夺者出生装备中的散弹枪会替换为 `GunA7`，而不是 `GunAK`。
- 当普通散弹枪被替换时，玩家会收到本地化提示。
- 每回合会在 LCZ GR-18 生成一把强化散弹枪。
- 强化散弹枪地面模型默认放大到 `2x`。
- 拾取强化散弹枪时显示：`你已拾取超级无敌散弹枪，伤害四倍`。
- 强化散弹枪会直接自动补满枪膛和弹仓，不会给玩家添加备用弹药。
- 强化散弹枪造成 `4x` 枪械伤害。
- GR-18 门需要全部三级钥卡权限，相当于 O5 级权限。
- GR-18 门会阻止 SCP-2176/Ghostlight 开门，并取消门伤害和强行破门尝试，包括基础游戏通过门伤害流程处理的手雷/SCP-018 等伤害。
- 玩家文字使用统一提示提供器模式。优先使用 RueI；没有 RueI 时默认不显示提示，除非启用原版兼容模式。

配置文件位置：

`%APPDATA%\SCP Secret Laboratory\LabAPI\configs\7777\EnhancedShotgun\config.yml`

配置项：

- `is_enabled`：启用或禁用插件。
- `language`：`""` 使用默认回退语言，`"cn"` 强制中文，`"en"` 强制英文。
- `hint_display.compatibility_mode`：为 `false` 时，没有 RueI 会使用空提示提供器，不显示提示；为 `true` 时，没有 RueI 会回退为短时间、限频的原版提示。
- `damage_multiplier`：强化散弹枪伤害倍率。默认 `4`。
- `pickup_scale`：强化散弹枪地面模型缩放。默认 `2`。
- `pickup_hint_duration`：强化散弹枪拾取提示显示秒数。
- `replacement_hint_duration`：普通散弹枪替换提示显示秒数。
- `lock_gr18_inner_door`：同时对 GR-18 内门应用 O5 限制。

已知限制：

- SCP:SL 插件是服务端插件，因此本插件可以可靠放大地面拾取物模型。手持第一/第三人称模型主要由客户端基础游戏控制，服务端无法完全权威缩放。
- 没有关联玩家的普通散弹枪替换，例如地图生成拾取物，无法显示玩家提示，因为没有接收者。
