<div align="center">
  <a href="https://swiftlys2.net/docs/" target="_blank">
    <img src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" alt="SwiftlyS2" width="780" />
  </a>
</div>

---

[![cn](https://flagcdn.com/48x36/cn.png)中文版](./README.md) 
[![en](https://flagcdn.com/48x36/gb.png)English](./README.en.md)

---
如果你喜欢这个插件,可以用以下方式支持我,感谢!

If you like this plugin, you can support me in the following ways. Thank you!

[![ko-fi](https://github.com/user-attachments/assets/3c01a28f-efe2-48af-9385-cef3a99fbb8c)](https://www.ifdian.net/a/XMHHAN)
[![paypal](https://github.com/user-attachments/assets/da293573-12c8-40bc-b956-d562cd46d4ae)](https://www.paypal.com/paypalme/XMHHAN)
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)


<hr>

# HanHitMarkS2 v3.1

SwiftlyS2 击中反馈插件，提供中心击中特效、伤害数字和可配置的粒子组菜单。特效及声音仅发送给攻击者，支持按权限限制粒子组。

需要 .NET 10 和 SwiftlyS2 1.4.13 或更新版本。自定义粒子与声音资源需要通过服务器资源包提供给客户端。

## 安装与使用

将 `HanHitmarker.dll`、`HanHitmarker.deps.json` 和 `resources` 目录放入 `addons/swiftlys2/plugins/HanHitmarker/`。插件 ID 为 `HanHitmarker`。配置文件位于 `addons/swiftlys2/configs/plugins/HanHitmarker/HanHitmarkerCFG.jsonc`，首次加载时自动生成。

默认在聊天输入 `!hm`，或在控制台输入 `sw_hm` 打开设置菜单。菜单支持选择粒子组、切换中心击中特效和伤害数字。配置修改后会自动加载；打开的旧菜单会关闭，再次输入命令即可显示新配置。

可以配合独立的 HanHitMarkS2UI 插件使用 Panorama 界面；未安装 UI 时使用 SwiftlyS2 原生菜单。粒子组来自本体配置，无需在 UI 中重复配置。

替换早期 `HanHitMarkerS2` 插件时，请移除其插件目录，避免同时运行两套击中反馈插件。v3.1 使用 `HanHitmarkerCFG` 配置格式；已有同格式的粒子组配置可以继续使用，`HanHitMarkerS2CFG` 和 WorldText 配置需要重新配置。

## 配置

所有配置位于 `HanHitmarkerCFG` 节点。完整示例见 [HanHitmarkerCFG.jsonc](config/HanHitmarkerCFG.jsonc)。

| 配置项 | 用途 |
| --- | --- |
| `Enabled` | 启用插件 |
| `MenuCommand` | 菜单命令，默认 `sw_hm` |
| `DefaultParticleGroup` | 玩家默认粒子组名称 |
| `PlayerDefaultCenterEffectEnabled` | 玩家默认中心特效开关 |
| `PlayerDefaultDamageNumberEnabled` | 玩家默认伤害数字开关 |
| `DamageNumberHeightOffset` | 伤害数字高度偏移 |
| `DamageNumberSpacing` | 数字间距 |
| `DamageNumberLifetimeSeconds` | 数字显示时间，单位秒 |
| `PrecacheSoundEvents` | 需要预缓存的声音事件资源 |
| `ParticleGroups` | 粒子组列表 |

每个粒子组包含 `Name`、`Enabled`、`AdminFlag`、`CenterHeadParticle`、`CenterBodyParticle`、`DamageDigitParticles`、`HitHeadSound`、`HitBodySound` 和 `DamageNumberSound`。`Name` 应唯一；`AdminFlag` 为空时所有玩家可用。`DamageDigitParticles` 必须按数字 **0 到 9** 顺序填写恰好十个非空路径。增加或删除粒子组后菜单会随配置变化。

空的声音名称表示不播放对应声音。配置错误或缺失必需路径时才输出警告；正常加载及资源预缓存保持静默。玩家选择和开关保存在内存中，插件重新加载后恢复默认值。

## 构建

```powershell
dotnet build ./src/HanHitmarker.csproj -c Release -p:RestoreLockedMode=true
```
