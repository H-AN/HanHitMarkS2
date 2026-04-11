<div align="center">
  <a href="https://swiftlys2.net/docs/" target="_blank">
    <img src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" alt="SwiftlyS2" width="780" />
  </a>
</div>

<div align="center">
  <a href="./README.en.md"><img src="https://flagcdn.com/48x36/gb.png" alt="English" width="48" height="36" /> <strong>English</strong></a>
  &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
  <a href="./README.md"><img src="https://flagcdn.com/48x36/cn.png" alt="中文" width="48" height="36" /> <strong>中文版</strong></a>
</div>

<hr>

# HanHitMarkerS2

`HanHitMarkerS2` is a **SwiftlyS2** plugin for CS2 hit feedback.

It provides three attacker-only feedback channels:

- hit marker
- damage number
- screen hit particle

The hit marker and damage number can use either `WorldText` or particle mode, and the screen hit effect supports separate particles for headshots and body hits.

---

## Feature Overview

- All three effects are visible only to the attacker.
- `HitMarkType` and `DamageNumberType` now use readable string values:
  - `worldtext`
  - `particles`
- Empty or invalid `HitMarkType` / `DamageNumberType` values automatically fall back to `worldtext`.
- Screen hit particles support separate head/body configuration:
  - `ScreenHitEffectHeadParticle`
  - `ScreenHitEffectBodyParticle`
- Each feature supports independent configuration for:
  - global enable
  - team restriction
  - default player state
  - toggle command
  - command permission
  - feature permission
- Command replies are now stored in translation files instead of the main config.
- Both main config and WorldText config support hot reload.

---

## Commands

The plugin registers three raw commands by default:

| Feature | Default command | Description |
|------|------|------|
| Hit marker | `sw_hitmarker` | Toggle your own hit marker |
| Damage number | `sw_damage` | Toggle your own damage number |
| Screen hit particle | `sw_screenhitmarker` | Toggle your own screen hit effect |

Notes:

- These names are registered directly as raw command names. The plugin no longer exposes a `register raw` option.
- If your server wraps commands through chat prefixes, they can usually still be used through your existing command flow.
- Each command can have its own permission requirement.

---

## Permission Model

Each feature now has two different permission fields:

- `XXXCommandPermission`
  - controls who is allowed to run the toggle command
- `XXXFeaturePermission`
  - controls who is actually allowed to use the feature

For hit marker, for example:

- `HitMarkerCommandPermission`
- `HitMarkerFeaturePermission`

The same applies to:

- `DamageNumberCommandPermission`
- `DamageNumberFeaturePermission`
- `ScreenHitEffectCommandPermission`
- `ScreenHitEffectFeaturePermission`

Behavior rules:

- Command permission failed: the player cannot toggle that feature by command.
- Feature permission failed: the player will not actually get the effect even if the default state is enabled.
- Empty permission field: everyone can use it.

This makes it easy to support cases such as:

- everyone can use `sw_hitmarker`
- only admins can use `sw_damage`
- screen hit particles are limited to VIP or custom permission groups

---

## Configuration Files

The plugin mainly uses two config files:

- Main config:
  - `configs/plugins/HanHitMarkerS2/HanHitMarkerCFG.jsonc`
  - root section: `HanHitMarkerS2CFG`
- WorldText config:
  - `configs/plugins/HanHitMarkerS2/HanHitMarkerWorldTextCFG.jsonc`
  - root section: `HanHitMarkerWorldTextS2CFG`

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
| `HitMarkOnlyTeam` | string | Restrict by team: `any`, `t`, `ct` |
| `HitMarkHeadParticles` | string | Headshot hit marker particle path |
| `HitMarkBodyParticles` | string | Body hit marker particle path |
| `HitMarkHeadSound` | string | Headshot sound, leave empty to disable |
| `HitMarkBodySound` | string | Body sound, leave empty to disable |
| `HitMarkerFeaturePermission` | string | Permission required to actually use hit marker |
| `PlayerDefaultHitMarkerEnabled` | bool | Default hit marker state when a player's runtime state is first created |
| `HitMarkerToggleCommand` | string | Command name used to toggle hit marker |
| `HitMarkerCommandPermission` | string | Permission required to use the toggle command |

### Damage Number

| Field | Type | Description |
|------|------|------|
| `EnabledDamageNumber` | bool | Globally enable or disable damage number |
| `DamageNumberType` | string | Display mode: `worldtext` or `particles` |
| `DamageNumberOnlyTeam` | string | Restrict by team: `any`, `t`, `ct` |
| `DamageNumberParticles0` - `DamageNumberParticles9` | string | Particle paths for digits 0-9 when using particle digit mode |
| `DamageNumberSound` | string | Sound for damage number feedback, leave empty to disable |
| `DamageNumberFeaturePermission` | string | Permission required to actually use damage number |
| `PlayerDefaultDamageNumberEnabled` | bool | Default damage number state when a player's runtime state is first created |
| `DamageNumberToggleCommand` | string | Command name used to toggle damage number |
| `DamageNumberCommandPermission` | string | Permission required to use the toggle command |

### Screen Hit Particle

| Field | Type | Description |
|------|------|------|
| `EnabledScreenHitEffect` | bool | Globally enable or disable screen hit effect |
| `ScreenHitEffectOnlyTeam` | string | Restrict by team: `any`, `t`, `ct` |
| `ScreenHitEffectHeadParticle` | string | Screen particle used for headshots |
| `ScreenHitEffectBodyParticle` | string | Screen particle used for body hits |
| `ScreenHitEffectFeaturePermission` | string | Permission required to actually use screen hit effect |
| `PlayerDefaultScreenHitEffectEnabled` | bool | Default screen hit effect state when a player's runtime state is first created |
| `ScreenHitEffectToggleCommand` | string | Command name used to toggle screen hit effect |
| `ScreenHitEffectCommandPermission` | string | Permission required to use the toggle command |

### Shared Field

| Field | Type | Description |
|------|------|------|
| `PrecacheSoundEvent` | string | Sound event files to precache, separated by `,` |

Extra notes:

- `HitMarkType` and `DamageNumberType`
  - recommended values are `worldtext` or `particles`
  - legacy value `1` is still treated as `particles`
  - any other invalid value falls back to `worldtext`
- `PlayerDefault...Enabled`
  - only affects the initial runtime state for a player
  - it is not the same thing as the global feature switch
- Advanced screen particle dispatch settings are now fixed internally
  - attachment / split-screen / async-dispatch details are no longer exposed to users

---

## WorldText Config Reference

### Hit Marker WorldText

| Field | Type | Description |
|------|------|------|
| `WTHitMarkSignHead` | string | Headshot symbol, default `◎` |
| `WTHitMarkSignBody` | string | Body-hit symbol, default `X` |
| `WTHitMarkSizeHead` | float | Headshot marker size |
| `WTHitMarkSizeBody` | float | Body marker size |
| `WTHitMarkFontColor` | string | Text color in `R, G, B, A` format |
| `WTHitMarkDrawBackground` | bool | Draw black background box |
| `WTHitMarkFontName` | string | Font name |

### Damage Number WorldText

| Field | Type | Description |
|------|------|------|
| `WTHitNumberPosType` | int | Floating style: `0` fixed upward, `1` random bounce |
| `WTHitNumberSizeHead` | float | Headshot damage number size |
| `WTHitNumberSizeBody` | float | Body damage number size |
| `WTHitNumberFontColor` | string | Text color in `R, G, B, A` format |
| `WTHitNumberDrawBackground` | bool | Draw black background box |
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
    "ScreenHitEffectHeadParticle": "particles/exg/screen_hit.vpcf",
    "ScreenHitEffectBodyParticle": "particles/exg/screen_hit.vpcf",
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

Command replies are no longer stored in the main config. They now live in:

- `src/resources/translations/zh-CN.jsonc`
- `src/resources/translations/en.jsonc`

You can edit these files to change:

- player-only command message
- enabled / disabled reply text
- globally-disabled reply text
- no-permission reply text
- localized feature names

---

## Particle Resource Notes

If you use:

- `HitMarkType = "particles"`
- `DamageNumberType = "particles"`
- or configured screen hit particles

then your server must provide the required particle assets.

If you only use `worldtext`, no extra Workshop particle pack is required.

An older example Workshop resource used by this plugin family:

- `3626771819`

If your server uses `MultiAddonManager`, you can let it download and distribute the Workshop addon, then inspect particle paths with `Source2Viewer`.

---

## Build and Deployment

1. Build the plugin:

```powershell
dotnet build .\src\HanHitMarkerS2.csproj
```

2. Deploy the output files into your server plugin directory.
3. Edit the main config:
   - `configs/plugins/HanHitMarkerS2/HanHitMarkerCFG.jsonc`
4. If you use `worldtext` mode, also edit:
   - `configs/plugins/HanHitMarkerS2/HanHitMarkerWorldTextCFG.jsonc`
5. If you want custom reply text, edit:
   - `src/resources/translations/zh-CN.jsonc`
   - `src/resources/translations/en.jsonc`

---

## Recommended Validation

1. Verify all three default commands register correctly.
2. Verify `HitMarkType` and `DamageNumberType` switch correctly between `worldtext` and `particles`.
3. Verify headshot/body hit cases use the expected particle and sound.
4. Verify `HitMarkerCommandPermission` and `HitMarkerFeaturePermission` behave as expected.
5. Verify all 10 digit particles for damage number mode are present.
6. Verify `ScreenHitEffectHeadParticle` and `ScreenHitEffectBodyParticle` trigger correctly by hit location.
7. Verify hot reload updates behavior as expected after config edits.
