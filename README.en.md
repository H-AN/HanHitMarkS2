# HanHitMarkS2 v3.1

[中文版](README.md) | English

A SwiftlyS2 hit feedback plugin with center hit effects, damage numbers, and a configurable particle-group menu. Effects and sounds are sent only to the attacker. Particle groups can require permissions.

Requires .NET 10 and SwiftlyS2 1.4.13 or later. Supply custom particle and sound assets to players through your server's resource addon.

## Installation and use

Place `HanHitmarker.dll`, `HanHitmarker.deps.json`, and `resources` in `addons/swiftlys2/plugins/HanHitmarker/`. The plugin ID is `HanHitmarker`. Configuration is created on first load at `addons/swiftlys2/configs/plugins/HanHitmarker/HanHitmarkerCFG.jsonc`.

Enter `!hm` in chat or `sw_hm` in the console to open the menu. Players can select a particle group and toggle center effects or damage numbers. Configuration changes reload automatically and close existing menus; open the menu again to see the updated options.

The optional HanHitMarkS2UI plugin provides a Panorama interface. Without it, the plugin uses SwiftlyS2's standard menu. Both interfaces read the same particle groups and player settings.

When replacing the earlier `HanHitMarkerS2` plugin, remove its plugin folder to avoid running two hit feedback plugins. v3.1 uses the `HanHitmarkerCFG` configuration format. Existing particle-group configurations in that format can be retained; `HanHitMarkerS2CFG` and WorldText configurations require a new configuration.

## Configuration

Settings are under `HanHitmarkerCFG`. See the complete [configuration example](config/HanHitmarkerCFG.jsonc).

| Setting | Purpose |
| --- | --- |
| `Enabled` | Enable the plugin |
| `MenuCommand` | Menu command; defaults to `sw_hm` |
| `DefaultParticleGroup` | Default particle-group name |
| `PlayerDefaultCenterEffectEnabled` | Default center effect setting |
| `PlayerDefaultDamageNumberEnabled` | Default damage number setting |
| `DamageNumberHeightOffset` | Damage number height offset |
| `DamageNumberSpacing` | Spacing between digits |
| `DamageNumberLifetimeSeconds` | Damage number lifetime in seconds |
| `PrecacheSoundEvents` | Sound event resources to precache |
| `ParticleGroups` | Particle-group list |

Each group has `Name`, `Enabled`, `AdminFlag`, `CenterHeadParticle`, `CenterBodyParticle`, `DamageDigitParticles`, `HitHeadSound`, `HitBodySound`, and `DamageNumberSound`. Names must be unique. An empty `AdminFlag` allows everyone to use the group. `DamageDigitParticles` must contain exactly ten nonempty paths in **0-9** order. Adding or removing groups updates the menu automatically.

An empty sound name disables that sound. Normal loading and precaching are silent; invalid configuration or missing required paths produce warnings. Player selections and toggles are held in memory and return to defaults when the plugin reloads.

## Build

```powershell
dotnet build ./src/HanHitmarker.csproj -c Release -p:RestoreLockedMode=true
```

## Support

[爱发电](https://www.ifdian.net/a/XMHHAN) · [PayPal](https://www.paypal.com/paypalme/XMHHAN) · [Ko-fi](https://ko-fi.com/Z8Z31PY52N)
