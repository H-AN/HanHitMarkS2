<div align="center"><h1><img width="600" height="131" alt="68747470733a2f2f70616e2e73616d7979632e6465762f732f56596d4d5845" src="https://github.com/user-attachments/assets/d0316faa-c2d0-478f-a642-1e3c3651f1d4" /></h1></div>

<div class="section">
<div align="center"><h1>HitMark & Damage number for SwiftlyS2</h1></div>


<div align="center"><strong>基于 SwiftlyS2 框架开发的 CS2 伤害标记与伤害数字。</p></div>

<div align="center"><strong>支持自定义配置。</p></div>
<div align="center"><strong>支持自定义粒子,队伍开关,伤害数字,击中标记,音效等。</p></div>
</div>

<div align="center">

---
本插件免费,但是你可以买一杯咖啡支持我 😊 谢谢!

This plugin is free, but you can support me by buying me a cup of coffee. 😊 Thanks!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Z8Z31PY52N)
  

</div>

<div align="center">

视频演示 : https://www.bilibili.com/video/BV1UWqkB4ERf

Video : https://www.youtube.com/watch?v=59QzFj4j3qY

</div>

---

📦 创意工坊示例（HitMark 粒子特效 例子）


插件可结合以下创意工坊资源使用（示例）：
3626771819

Workshop Resource Example (HitMark Particle, number etc.)

The plugin can be used in conjunction with the following Workshop resources (example): Resource ID: 3626771819

```
要使用创意工坊资源,需要服务器安装metamod插件 multiaddonmanager 来管理服务器和玩家使用下载和安装创意工坊资源

安装multiaddonmanager插件后 在game\csgo\cfg\multiaddonmanager\multiaddonmanager.cfg配置文件中
 
找到第一行 mm_extra_addons  "3626771819"

把资源ID填写上去 等待服务器下载资源完毕 玩家进服会自动下载资源

之后用 Source2Viewer 软件 打开资源包 查看资源内的 粒子路径名字

之后根据需要填写到HitMark配置内使用

To use Workshop content, the server must have the Metamod plugin "MultiAddonManager" installed to handle downloads.
After installation, edit the config file:
game\csgo\cfg\multiaddonmanager\multiaddonmanager.cfg

Locate the line:
mm_extra_addons "3626771819"

Add the Workshop ID and wait for the server to download it. Clients will automatically download the content upon joining.
Then, use Source2Viewer to inspect the addon and find Particle paths.
Fill them into the HitMark configuration as needed.

```
---

🧩 插件配置 / Plugin Configuration
```
EnabledHitMark 开启/关闭 击中特效 默认 true
HitMarkOnlyTeam 仅允许某个队伍 使用 默认 "any" 所有队伍都能使用 "ct" 只有ct启用
HitMarkHeadParticles 爆头击中标记粒子路径
HitMarkBodyParticles 击中身体标记粒子路径
HitMarkHeadSound 爆头击中音效 (不填写不播放)
HitMarkBodySound 击中身体音效 (不填写不播放)
EnabledDamageNumber 开启/关闭 击中数字显示 默认 true
DamageNumberOnlyTeam 仅允许某个队伍 使用 默认 "any" 所有队伍都能使用 "ct" 只有ct启用
DamageNumberParticles0 - DamageNumberParticles9 需要0-9 一共10个单独数字粒子来显示伤害飘字
DamageNumberSound 显示伤害数字时播放的音效 (不填写不播放)
PrecacheSoundEvent 预缓存声音事件, 多个声音事件 用 , 隔开

EnabledHitMark Enable/Disable hit marker effects Default true
HitMarkOnlyTeam Only allow a specific team to use it Default "any" all teams can use it "ct" only CT team can use it
HitMarkHeadParticles Headshot hit marker particle path
HitMarkBodyParticles Body hit marker particle path
HitMarkHeadSound Headshot hit sound (leave empty to disable)
HitMarkBodySound Body hit sound (leave empty to disable)
EnabledDamageNumber Enable/Disable damage number display Default true
DamageNumberOnlyTeam Only allow a specific team to use it Default "any" all teams can use it "ct" only CT team can use it
DamageNumberParticles0 - DamageNumberParticles9 Requires 10 separate digit particles (0–9) to display floating damage numbers
DamageNumberSound Sound played when damage numbers are displayed (leave empty to disable)
PrecacheSoundEvent Precache sound events, multiple sound events should be separated by ,

```

---
v2.0 Update / v2.0 更新

New configuration added / 新增配置：

Damage hit markers and damage numbers can now be displayed without using Workshop resources.

伤害标记与伤害数字现在可以 不使用创意工坊资源 显示。

You can switch the display type via configuration and use World Text to create hit markers and damage numbers.

可以通过配置切换类型，使用 Worldtext 制作标记和数字。

New fields / 新增字段

```
HitMarkType

0 = Use World Text / 使用 Worldtext

1 = Use Workshop particle resources / 使用创意工坊粒子资源

Default / 默认: 0

DamageNumberType

0 = Use World Text / 使用 Worldtext

1 = Use Workshop particle resources / 使用创意工坊粒子资源

Default / 默认: 0

New configuration file / 新增配置文件

HanHitMarkWorldTextCFG.jsonc

World Text Hit Marker Settings / Worldtext 击中标记设置

WTHitMarkSignHead
Custom symbol for headshot hit marker / 击中爆头标记自定义符号
Default / 默认: ⊙

WTHitMarkSignBody
Custom symbol for body hit marker / 击中身体标记自定义符号
Default / 默认: X

WTHitMarkSizeHead
Custom symbol size for headshot hit marker / 击中爆头标记尺寸
Default / 默认: 25

WTHitMarkSizeBody
Custom symbol size for body hit marker / 击中身体标记尺寸
Default / 默认: 25

WTHitMarkFontColor
Custom color for hit marker text / 击中标记自定义颜色
Default / 默认: "255, 0, 0, 255"

WTHitMarkDrawBackground
Enable black background box for hit marker / 是否开启黑色方框背景
Default / 默认: false

WTHitMarkFontName
Custom font for hit marker / 击中标记自定义字体
Default / 默认: "Arial Bold"

World Text Damage Number Settings / Worldtext 击中数字设置

WTHitNumberPosType
Damage number display type / 击中数字显示类型

0 = Fixed upward vertical movement / 固定竖直向上

1 = Random bouncing movement / 随机跳动
Default / 默认: 0

WTHitNumberSizeHead
Damage number size for headshots / 爆头数字尺寸
Default / 默认: 25

WTHitNumberSizeBody
Damage number size for body hits / 身体数字尺寸
Default / 默认: 20

WTHitNumberFontColor
Custom color for damage numbers / 击中数字自定义颜色
Default / 默认: "255, 0, 0, 255"

WTHitNumberDrawBackground
Enable black background box for damage numbers / 是否开启黑色方框背景
Default / 默认: false

WTHitNumberFontName
Custom font for damage numbers / 击中数字自定义字体
Default / 默认: "Arial Bold"
```
