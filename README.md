<div align="center">
  <a href="https://swiftlys2.net/docs/" target="_blank">
    <img src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" alt="SwiftlyS2" width="780" />
  </a>
</div>

---

[![cn](https://flagcdn.com/48x36/cn.png)中文版](./README.md) 
[![en](https://flagcdn.com/48x36/gb.png)英文版](./README.en.md)

---
如果你喜欢这个插件,可以用以下方式支持我,感谢!

If you like this plugin, you can support me in the following ways. Thank you!

[![ko-fi](https://github.com/user-attachments/assets/3c01a28f-efe2-48af-9385-cef3a99fbb8c)](https://www.ifdian.net/a/XMHHAN)
[![paypal](https://github.com/user-attachments/assets/da293573-12c8-40bc-b956-d562cd46d4ae)](https://www.paypal.com/paypalme/XMHHAN)
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)


<hr>

# HanHitMarkerS2 V3.0 插件重构

`HanHitMarkerS2` 是一个基于 **SwiftlyS2** 的 CS2 击中反馈插件。
v3.0 新增支持 DispatchParticleEffect 将击中特效粒子直接发送至玩家屏幕准心

它为攻击者提供三类仅自己可见的反馈：

- 击中特效
- 伤害数字
- 屏幕击中粒子

其中击中特效和伤害数字都支持 `WorldText` 或粒子两种显示方式，屏幕击中粒子支持爆头与身体分别配置不同粒子。

---

## 功能概览

- 三个功能全部为攻击者单独显示，不会广播给其他玩家。
- `HitMarkType` 与 `DamageNumberType` 现在使用更直观的字符串模式：
  - `worldtext`
  - `particles`
- `HitMarkType` 或 `DamageNumberType` 留空、填写错误时会自动回退为 `worldtext`。
- 屏幕击中粒子支持爆头与身体分别配置：
  - `ScreenHitEffectHeadParticle`
  - `ScreenHitEffectBodyParticle`
- 每个功能都支持独立配置：
  - 全局开关
  - 队伍限制
  - 玩家默认开关
  - 切换命令
  - 命令权限
  - 功能权限
- 命令提示文本可在翻译文件中更改。
- 主配置与 WorldText 配置都支持热重载。

---

## 命令

默认提供三个原始命令：

| 功能 | 默认命令 | 说明 |
|------|------|------|
| 击中特效 | `sw_hitmarker` | 切换自己的击中特效 (开关)|
| 伤害数字 | `sw_damage` | 切换自己的伤害数字 (开关)|
| 屏幕击中粒子 | `sw_screenhitmarker` | 切换自己的屏幕击中粒子 (开关)|

说明：
- 每个命令都可以独立设置权限。
---

## 权限设计

每个功能现在有两种权限字段：

- `XXXCommandPermission`
  - 控制“谁可以执行切换命令”
- `XXXFeaturePermission`
  - 控制“谁可以真正使用这个功能”

以击中特效为例：

- `HitMarkerCommandPermission`
- `HitMarkerFeaturePermission`

同理：

- `DamageNumberCommandPermission`
- `DamageNumberFeaturePermission`
- `ScreenHitEffectCommandPermission`
- `ScreenHitEffectFeaturePermission`

行为规则：

- 命令权限不满足：玩家不能通过命令切换该功能。
- 功能权限不满足：玩家即使默认开启，也不会实际看到该功能效果。
- 权限字段留空：表示所有玩家都可用。

这套设计适合下面这类场景：

- 所有人都能用 `sw_hitmarker`
- 只有管理员能用 `sw_damage`
- 屏幕击中粒子只开放给 VIP 或特定权限玩家

---

## 配置文件

本插件主要有两份配置文件：

- 主配置：
  - `configs/plugins/HanHitMarkerS2/HanHitMarkerCFG.jsonc`
  - 根节点：`HanHitMarkerS2CFG`
- WorldText 配置：
  - `configs/plugins/HanHitMarkerS2/HanHitMarkerWorldTextCFG.jsonc`
  - 根节点：`HanHitMarkerWorldTextS2CFG`

翻译文件：

- `src/resources/translations/zh-CN.jsonc`
- `src/resources/translations/en.jsonc`

---

## 主配置说明

### 击中特效

| 字段 | 类型 | 说明 |
|------|------|------|
| `EnabledHitMark` | bool | 全局开启或关闭击中特效 |
| `HitMarkType` | string | 显示方式：`worldtext` 或 `particles` |
| `HitMarkOnlyTeam` | string | 仅允许指定队伍使用：`any`、`t`、`ct` |
| `HitMarkHeadParticles` | string | 爆头击中特效粒子路径 |
| `HitMarkBodyParticles` | string | 身体击中特效粒子路径 |
| `HitMarkHeadSound` | string | 爆头音效，留空则不播放 |
| `HitMarkBodySound` | string | 身体音效，留空则不播放 |
| `HitMarkerFeaturePermission` | string | 使用击中特效所需权限 |
| `PlayerDefaultHitMarkerEnabled` | bool | 玩家首次建立运行时状态时，击中特效默认是否开启 |
| `HitMarkerToggleCommand` | string | 切换击中特效的命令名 |
| `HitMarkerCommandPermission` | string | 使用切换命令所需权限 |

### 伤害数字

| 字段 | 类型 | 说明 |
|------|------|------|
| `EnabledDamageNumber` | bool | 全局开启或关闭伤害数字 |
| `DamageNumberType` | string | 显示方式：`worldtext` 或 `particles` |
| `DamageNumberOnlyTeam` | string | 仅允许指定队伍使用：`any`、`t`、`ct` |
| `DamageNumberParticles0` - `DamageNumberParticles9` | string | 粒子数字模式下 0-9 每个数字对应的粒子路径 |
| `DamageNumberSound` | string | 伤害数字音效，留空则不播放 |
| `DamageNumberFeaturePermission` | string | 使用伤害数字所需权限 |
| `PlayerDefaultDamageNumberEnabled` | bool | 玩家首次建立运行时状态时，伤害数字默认是否开启 |
| `DamageNumberToggleCommand` | string | 切换伤害数字的命令名 |
| `DamageNumberCommandPermission` | string | 使用切换命令所需权限 |

### 屏幕击中粒子

| 字段 | 类型 | 说明 |
|------|------|------|
| `EnabledScreenHitEffect` | bool | 全局开启或关闭屏幕击中粒子 |
| `ScreenHitEffectOnlyTeam` | string | 仅允许指定队伍使用：`any`、`t`、`ct` |
| `ScreenHitEffectHeadParticle` | string | 爆头时播放的屏幕粒子 |
| `ScreenHitEffectBodyParticle` | string | 身体命中时播放的屏幕粒子 |
| `ScreenHitEffectFeaturePermission` | string | 使用屏幕击中粒子所需权限 |
| `PlayerDefaultScreenHitEffectEnabled` | bool | 玩家首次建立运行时状态时，屏幕击中粒子默认是否开启 |
| `ScreenHitEffectToggleCommand` | string | 切换屏幕击中粒子的命令名 |
| `ScreenHitEffectCommandPermission` | string | 使用切换命令所需权限 |

### 共享字段

| 字段 | 类型 | 说明 |
|------|------|------|
| `PrecacheSoundEvent` | string | 预缓存 sound event 文件，多个值用 `,` 分隔 |

补充说明：

- `HitMarkType` 与 `DamageNumberType`
  - 推荐填写 `worldtext` 或 `particles`
  - 为兼容旧配置，历史值 `1` 仍会被视为 `particles`
  - 其他无效值都会按 `worldtext` 处理
- `PlayerDefault...Enabled`
  - 只在玩家第一次建立本局运行时状态时生效
  - 它不是全局总开关
---

## WorldText 配置说明

### 击中特效 WorldText

| 字段 | 类型 | 说明 |
|------|------|------|
| `WTHitMarkSignHead` | string | 爆头标记字符，默认 `◎` |
| `WTHitMarkSignBody` | string | 身体命中标记字符，默认 `X` |
| `WTHitMarkSizeHead` | float | 爆头标记大小 |
| `WTHitMarkSizeBody` | float | 身体标记大小 |
| `WTHitMarkFontColor` | string | 文字颜色，格式：`R, G, B, A` |
| `WTHitMarkDrawBackground` | bool | 是否绘制黑色背景框 |
| `WTHitMarkFontName` | string | 字体名称 |

### 伤害数字 WorldText

| 字段 | 类型 | 说明 |
|------|------|------|
| `WTHitNumberPosType` | int | 数字漂浮方式：`0` 固定向上，`1` 随机弹跳 |
| `WTHitNumberSizeHead` | float | 爆头伤害数字大小 |
| `WTHitNumberSizeBody` | float | 身体伤害数字大小 |
| `WTHitNumberFontColor` | string | 文字颜色，格式：`R, G, B, A` |
| `WTHitNumberDrawBackground` | bool | 是否绘制黑色背景框 |
| `WTHitNumberFontName` | string | 字体名称 |

---

## 配置示例

### 主配置示例

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

### WorldText 配置示例

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

## 翻译文件

- `src/resources/translations/zh-CN.jsonc`
- `src/resources/translations/en.jsonc`

你可以在这里修改：

- 命令只能由玩家使用的提示
- 功能开启/关闭提示
- 全局关闭提示
- 权限不足提示
- 功能名称文本

---

## 粒子资源说明

如果你使用：

- `HitMarkType = "particles"`
- `DamageNumberType = "particles"`
- 或者配置了屏幕击中粒子

那么你需要保证服务器已安装对应粒子资源。

如果只使用 `worldtext`，则不需要额外 Workshop 粒子资源。

创意工坊资源 ID(示例)：

- `3626771819`

如果你的服务器使用 `MultiAddonManager`，可以把 Workshop 资源交给它下载与分发，然后再用 `Source2Viewer` 查看粒子路径。

---

## 安装与构建

---

## 使用建议与检查

1. 三个默认命令是否都能正常注册。
2. `HitMarkType` 与 `DamageNumberType` 是否按预期切换为 `worldtext` 或 `particles`。
3. 爆头与身体命中时是否正确区分粒子和音效。
4. `HitMarkerCommandPermission` 与 `HitMarkerFeaturePermission` 是否符合预期。
5. 伤害数字 0-9 粒子资源是否完整。
6. `ScreenHitEffectHeadParticle` 与 `ScreenHitEffectBodyParticle` 是否按命中部位区分生效。
7. 修改配置后热重载是否符合预期。
