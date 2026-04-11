<div align="center">
  <a href="https://swiftlys2.net/docs/" target="_blank">
    <img src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" alt="SwiftlyS2" width="780" />
  </a>
</div>

<div align="center">
  <a href="./README.md"><img src="https://flagcdn.com/48x36/cn.png" alt="中文" width="48" height="36" /> <strong>中文版</strong></a>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="./README.en.md"><img src="https://flagcdn.com/48x36/gb.png" alt="English" width="48" height="36" /> <strong>English</strong></a>
</div>

<hr>



如果你喜欢这个插件,可以用以下方式支持我,感谢!

If you like this plugin, you can support me in the following ways. Thank you!

[![ko-fi](https://github.com/user-attachments/assets/3c01a28f-efe2-48af-9385-cef3a99fbb8c)](https://www.ifdian.net/a/XMHHAN)
[![paypal](https://github.com/user-attachments/assets/da293573-12c8-40bc-b956-d562cd46d4ae)](https://www.paypal.com/paypalme/XMHHAN)
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)

---

# HanHitMarkerS2 V3.0 Plugin Refactor

`HanHitMarkerS2` is a CS2 hit feedback plugin built on **SwiftlyS2**.
V3.0 adds support for `DispatchParticleEffect`, allowing hit particles to be sent directly to the player's crosshair on screen.

It provides three types of feedback that are visible only to the attacker:

- Hit marker
- Damage number
- Screen hit particle

Both hit marker and damage number support `WorldText` or particle display modes, and the screen hit particle supports separate particle settings for headshots and body hits.

---

## Feature Overview

- All three features are shown only to the attacker and are not broadcast to other players.
- `HitMarkType` and `DamageNumberType` now use more intuitive string modes:
  - `worldtext`
  - `particles`
- If `HitMarkType` or `DamageNumberType` is empty or invalid, it automatically falls back to `worldtext`.
- Screen hit particles support separate settings for headshots and body hits:
  - `ScreenHitEffectHeadParticle`
  - `ScreenHitEffectBodyParticle`
- Each feature supports its own settings for:
  - Global toggle
  - Team restriction
  - Player default toggle
  - Toggle command
  - Command permission
  - Feature permission
- Command reply text can be changed in the translation files.
- Both the main config and the WorldText config support hot reload.

---

## Commands

The plugin provides three raw commands by default:

| Feature | Default Command | Description |
|------|------|------|
| Hit marker | `sw_hitmarker` | Toggle your own hit marker (on/off) |
| Damage number | `sw_damage` | Toggle your own damage number (on/off) |
| Screen hit particle | `sw_screenhitmarker` | Toggle your own screen hit particle (on/off) |

Notes:

- Each command can have its own permission.

---

## Permission Design

Each feature now has two permission fields:

- `XXXCommandPermission`
  - Controls who can run the toggle command
- `XXXFeaturePermission`
  - Controls who can actually use the feature

Using hit marker as an example:

- `HitMarkerCommandPermission`
- `HitMarkerFeaturePermission`

The same applies to:

- `DamageNumberCommandPermission`
- `DamageNumberFeaturePermission`
- `ScreenHitEffectCommandPermission`
- `ScreenHitEffectFeaturePermission`

Behavior rules:

- If the command permission is not met, the player cannot toggle that feature with the command.
- If the feature permission is not met, the player will not actually see the feature even if it is enabled by default.
- If the permission field is empty, it means all players can use it.

This design works well for scenarios like:

- Everyone can use `sw_hitmarker`
- Only admins can use `sw_damage`
- Screen hit particles are available only to VIP players or players with a specific permission

---

## Configuration Files

This plugin mainly uses two config files:

- Main config:
  - `configs/plugins/HanHitMarkerS2/HanHitMarkerCFG.jsonc`
  - Root node: `HanHitMarkerS2CFG`
- WorldText config:
  - `configs/plugins/HanHitMarkerS2/HanHitMarkerWorldTextCFG.jsonc`
  - Root node: `HanHitMarkerWorldTextS2CFG`

Translation files:

- `src/resources/translations/zh-CN.jsonc`
- `src/resources/translations/en.jsonc`

---

## Main Config Reference

### Hit Marker

| Field | Type | Description |
|------|------|------|
| `EnabledHitMark` | bool | Globally enable or disable hit marker |
| `HitMarkType` | string | Display mode: `worldtext` or `particles` |
| `HitMarkOnlyTeam` | string | Allow only a specific team: `any`, `t`, `ct` |
| `HitMarkHeadParticles` | string | Particle path used for headshot hit marker |
| `HitMarkBodyParticles` | string | Particle path used for body hit marker |
| `HitMarkHeadSound` | string | Headshot sound; leave empty to disable |
| `HitMarkBodySound` | string | Body-hit sound; leave empty to disable |
| `HitMarkerFeaturePermission` | string | Permission required to use the hit marker |
| `PlayerDefaultHitMarkerEnabled` | bool | Whether hit marker is enabled by default when the player's runtime state is created for the first time |
| `HitMarkerToggleCommand` | string | Command name used to toggle hit marker |
| `HitMarkerCommandPermission` | string | Permission required to use the toggle command |

### Damage Number

| Field | Type | Description |
|------|------|------|
| `EnabledDamageNumber` | bool | Globally enable or disable damage number |
| `DamageNumberType` | string | Display mode: `worldtext` or `particles` |
| `DamageNumberOnlyTeam` | string | Allow only a specific team: `any`, `t`, `ct` |
| `DamageNumberParticles0` - `DamageNumberParticles9` | string | Particle paths for digits 0-9 when using particle number mode |
| `DamageNumberSound` | string | Damage number sound; leave empty to disable |
| `DamageNumberFeaturePermission` | string | Permission required to use damage number |
| `PlayerDefaultDamageNumberEnabled` | bool | Whether damage number is enabled by default when the player's runtime state is created for the first time |
| `DamageNumberToggleCommand` | string | Command name used to toggle damage number |
| `DamageNumberCommandPermission` | string | Permission required to use the toggle command |

### Screen Hit Particle

| Field | Type | Description |
|------|------|------|
| `EnabledScreenHitEffect` | bool | Globally enable or disable the screen hit particle |
| `ScreenHitEffectOnlyTeam` | string | Allow only a specific team: `any`, `t`, `ct` |
| `ScreenHitEffectHeadParticle` | string | Screen particle played on headshot |
| `ScreenHitEffectBodyParticle` | string | Screen particle played on body hit |
| `ScreenHitEffectFeaturePermission` | string | Permission required to use the screen hit particle |
| `PlayerDefaultScreenHitEffectEnabled` | bool | Whether screen hit particle is enabled by default when the player's runtime state is created for the first time |
| `ScreenHitEffectToggleCommand` | string | Command name used to toggle the screen hit particle |
| `ScreenHitEffectCommandPermission` | string | Permission required to use the toggle command |

### Shared Field

| Field | Type | Description |
|------|------|------|
| `PrecacheSoundEvent` | string | Sound event files to precache, separated by `,` |

Additional notes:

- `HitMarkType` and `DamageNumberType`
  - Recommended values are `worldtext` or `particles`
  - For compatibility with older configs, the legacy value `1` is still treated as `particles`
  - Any other invalid value is handled as `worldtext`
- `PlayerDefault...Enabled`
  - Only takes effect when the player's runtime state is created for the first time in the current match
  - It is not the global master switch

---

## WorldText Config Reference

### Hit Marker WorldText

| Field | Type | Description |
|------|------|------|
| `WTHitMarkSignHead` | string | Headshot marker character, default `◎` |
| `WTHitMarkSignBody` | string | Body-hit marker character, default `X` |
| `WTHitMarkSizeHead` | float | Headshot marker size |
| `WTHitMarkSizeBody` | float | Body marker size |
| `WTHitMarkFontColor` | string | Text color in format: `R, G, B, A` |
| `WTHitMarkDrawBackground` | bool | Whether to draw a black background box |
| `WTHitMarkFontName` | string | Font name |

### Damage Number WorldText

| Field | Type | Description |
|------|------|------|
| `WTHitNumberPosType` | int | Number float mode: `0` fixed upward, `1` random bounce |
| `WTHitNumberSizeHead` | float | Headshot damage number size |
| `WTHitNumberSizeBody` | float | Body-hit damage number size |
| `WTHitNumberFontColor` | string | Text color in format: `R, G, B, A` |
| `WTHitNumberDrawBackground` | bool | Whether to draw a black background box |
| `WTHitNumberFontName` | string | Font name |

---

## Example Configuration

### Main Config Example

```jsonc
{
  "HanHitMarkerS2CFG": {
    "EnabledHitMark": true,
    "HitMarkType": "worldtext",
    "HitMarkOnlyTeam": "any",
    "HitMarkHeadParticles": "particles/exg/exg_hitmarker2.vpcf",
    "HitMarkBodyParticles": "particles/exg/exg_hitmarker.vpcf",
    "HitMarkHeadSound": "Breakable.Flesh",
    "HitMarkBodySound": "Flesh_Bloody.ImpactHard",
    "HitMarkerFeaturePermission": "",

    "EnabledDamageNumber": true,
    "DamageNumberType": "worldtext",
    "DamageNumberOnlyTeam": "any",
    "DamageNumberParticles0": "particles/exg/hitmarker/0.vpcf",
    "DamageNumberParticles1": "particles/exg/hitmarker/01.vpcf",
    "DamageNumberParticles2": "particles/exg/hitmarker/02.vpcf",
    "DamageNumberParticles3": "particles/exg/hitmarker/03.vpcf",
    "DamageNumberParticles4": "particles/exg/hitmarker/04.vpcf",
    "DamageNumberParticles5": "particles/exg/hitmarker/05.vpcf",
    "DamageNumberParticles6": "particles/exg/hitmarker/06.vpcf",
    "DamageNumberParticles7": "particles/exg/hitmarker/07.vpcf",
    "DamageNumberParticles8": "particles/exg/hitmarker/08.vpcf",
    "DamageNumberParticles9": "particles/exg/hitmarker/09.vpcf",
    "DamageNumberSound": "ceiling_tile.BulletImpact",
    "DamageNumberFeaturePermission": "",

    "EnabledScreenHitEffect": true,
    "ScreenHitEffectOnlyTeam": "any",
    "ScreenHitEffectHeadParticle": "particles/cgmentos/hitmarker/overlay_hitmarker_head.vpcf",
    "ScreenHitEffectBodyParticle": "particles/cgmentos/hitmarker/overlay_hitmarker_body.vpcf",
    "ScreenHitEffectFeaturePermission": "",

    "PlayerDefaultHitMarkerEnabled": true,
    "PlayerDefaultDamageNumberEnabled": true,
    "PlayerDefaultScreenHitEffectEnabled": true,

    "HitMarkerToggleCommand": "sw_hitmarker",
    "DamageNumberToggleCommand": "sw_damage",
    "ScreenHitEffectToggleCommand": "sw_screenhitmarker",

    "HitMarkerCommandPermission": "",
    "DamageNumberCommandPermission": "",
    "ScreenHitEffectCommandPermission": "",

    "PrecacheSoundEvent": "soundevents/game_sounds_physics.vsndevts"
  }
}
```

### WorldText Config Example

```jsonc
{
  "HanHitMarkerWorldTextS2CFG": {
    "WTHitMarkSignHead": "◎",
    "WTHitMarkSignBody": "X",
    "WTHitMarkSizeHead": 25,
    "WTHitMarkSizeBody": 20,
    "WTHitMarkFontColor": "255, 0, 0, 255",
    "WTHitMarkDrawBackground": false,
    "WTHitMarkFontName": "Arial Bold",

    "WTHitNumberPosType": 0,
    "WTHitNumberSizeHead": 25,
    "WTHitNumberSizeBody": 20,
    "WTHitNumberFontColor": "255, 0, 0, 255",
    "WTHitNumberDrawBackground": false,
    "WTHitNumberFontName": "Arial Bold"
  }
}
```

---

## Translation Files

- `src/resources/translations/zh-CN.jsonc`
- `src/resources/translations/en.jsonc`

You can edit these files to change:

- The message that tells users the command can only be used by players
- Feature enabled/disabled messages
- Global disabled messages
- No-permission messages
- Feature name text

---

## Particle Resource Notes

If you use:

- `HitMarkType = "particles"`
- `DamageNumberType = "particles"`
- Or configure screen hit particles

then you need to make sure the required particle resources are installed on the server.

If you only use `worldtext`, no extra Workshop particle resources are required.

Workshop resource ID (example):

- `3626771819`

If your server uses `MultiAddonManager`, you can let it download and distribute the Workshop resource, then use `Source2Viewer` to inspect particle paths.

---

## Installation and Build

---

## Usage Recommendations and Checks

1. Make sure all three default commands register correctly.
2. Make sure `HitMarkType` and `DamageNumberType` switch to `worldtext` or `particles` as expected.
3. Make sure headshots and body hits use the correct particles and sounds.
4. Make sure `HitMarkerCommandPermission` and `HitMarkerFeaturePermission` behave as expected.
5. Make sure the damage number particle resources for digits 0-9 are complete.
6. Make sure `ScreenHitEffectHeadParticle` and `ScreenHitEffectBodyParticle` work separately based on hit location.
7. Make sure hot reload behaves as expected after config changes.
