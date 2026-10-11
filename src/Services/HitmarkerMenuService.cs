using HanHitmarker.Config;
using HanHitmarker.Runtime;
using Microsoft.Extensions.Options;
using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace HanHitmarker.Services;

public sealed class HitmarkerMenuService
{
    private readonly ISwiftlyCore _core;
    private readonly IOptionsMonitor<HanHitmarkerConfig> _config;
    private readonly HitmarkerRuntimeStore _runtimeStore;
    private readonly HitmarkerPermissionService _permission;
    private readonly HitmarkerParticleGroupService _groups;
    private readonly HitmarkerMenuScope _scope;

    public HitmarkerMenuService(ISwiftlyCore core, IOptionsMonitor<HanHitmarkerConfig> config,
        HitmarkerRuntimeStore runtimeStore, HitmarkerPermissionService permission,
        HitmarkerParticleGroupService groups, HitmarkerMenuScope scope)
    {
        _core = core;
        _config = config;
        _runtimeStore = runtimeStore;
        _permission = permission;
        _groups = groups;
        _scope = scope;
    }

    public void OpenMainMenu(IPlayer player)
    {
        if (player is not { IsValid: true, IsFakeClient: false }) return;
        if (!_config.CurrentValue.Enabled)
        {
            player.SendMessage(MessageType.Chat, T(player, "HanHitmarker.Menu.Disabled"));
            return;
        }
        _scope.Open(player, BuildMainMenu(player));
    }

    private IMenuAPI NewMenu(IPlayer player, string titleKey, string tag)
    {
        var builder = _core.MenusAPI.CreateBuilder();
        builder.Design.SetMenuTitle(T(player, titleKey));
        builder.EnableSound();
        builder.SetPlayerFrozen(false);
        var menu = builder.Build();
        menu.Tag = tag;
        return menu;
    }

    private IMenuAPI BuildMainMenu(IPlayer player)
    {
        _runtimeStore.GetOrCreate(player, _config.CurrentValue);
        var owner = _scope.Capture(player);
        var menu = NewMenu(player, "HanHitmarker.Menu.Title", "hanhitmarker:main");

        menu.AddOption(new TextMenuOption
        {
            BindingText = () => _scope.Resolve(owner) is { } current ? BuildSummaryText(current) : string.Empty
        });

        var selectGroup = new ButtonMenuOption
        {
            BindingText = () => _scope.Resolve(owner) is { } current
                ? T(current, "HanHitmarker.Menu.SelectGroup", GetDisplayGroupName(current)) : string.Empty
        };
        selectGroup.Click += async (_, args) => await DispatchAsync(owner, menu, args.Player, OpenGroupSelectMenu);
        menu.AddOption(selectGroup);

        var toggleCenter = new ButtonMenuOption
        {
            BindingText = () => _scope.Resolve(owner) is { } current
                ? T(current, "HanHitmarker.Menu.CenterEffect", StateText(current,
                    _runtimeStore.GetOrCreate(current, _config.CurrentValue).CenterEffectEnabled)) : string.Empty
        };
        toggleCenter.Click += async (_, args) => await DispatchAsync(owner, menu, args.Player, current =>
        {
            var config = _config.CurrentValue;
            var next = !_runtimeStore.GetOrCreate(current, config).CenterEffectEnabled;
            _runtimeStore.SetCenterEffectEnabled(current, next, config);
            current.SendMessage(MessageType.Chat, T(current,
                next ? "HanHitmarker.Menu.CenterEnabled" : "HanHitmarker.Menu.CenterDisabled"));
        });
        menu.AddOption(toggleCenter);

        var toggleDamage = new ButtonMenuOption
        {
            BindingText = () => _scope.Resolve(owner) is { } current
                ? T(current, "HanHitmarker.Menu.DamageNumber", StateText(current,
                    _runtimeStore.GetOrCreate(current, _config.CurrentValue).DamageNumberEnabled)) : string.Empty
        };
        toggleDamage.Click += async (_, args) => await DispatchAsync(owner, menu, args.Player, current =>
        {
            var config = _config.CurrentValue;
            var next = !_runtimeStore.GetOrCreate(current, config).DamageNumberEnabled;
            _runtimeStore.SetDamageNumberEnabled(current, next, config);
            current.SendMessage(MessageType.Chat, T(current,
                next ? "HanHitmarker.Menu.DamageEnabled" : "HanHitmarker.Menu.DamageDisabled"));
        });
        menu.AddOption(toggleDamage);
        return menu;
    }

    private void OpenGroupSelectMenu(IPlayer player)
    {
        if (player is not { IsValid: true, IsFakeClient: false } || !_config.CurrentValue.Enabled) return;
        _scope.Open(player, BuildGroupSelectMenu(player));
    }

    private IMenuAPI BuildGroupSelectMenu(IPlayer player)
    {
        var owner = _scope.Capture(player);
        var menu = NewMenu(player, "HanHitmarker.Menu.GroupTitle", "hanhitmarker:groups");
        var groups = _permission.GetVisibleGroups(player);
        if (groups.Count == 0)
        {
            menu.AddOption(new TextMenuOption(T(player, "HanHitmarker.Menu.NoGroups")) { Enabled = false });
            return menu;
        }

        foreach (var group in groups)
        {
            var groupName = group.Name;
            var button = new ButtonMenuOption
            {
                BindingText = () =>
                {
                    if (_scope.Resolve(owner) is not { } current) return string.Empty;
                    var state = _runtimeStore.GetOrCreate(current, _config.CurrentValue);
                    return state.SelectedGroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase)
                        ? T(current, "HanHitmarker.Menu.GroupSelected", groupName) : groupName;
                }
            };
            button.Click += async (_, args) => await DispatchAsync(owner, menu, args.Player, current =>
            {
                // 展示时的权限不替代点击时校验；组可能在配置重载或权限变化后已不可用。
                var selected = _groups.GetByName(groupName);
                if (selected is null || !_permission.CanUseGroup(current, selected))
                {
                    current.SendMessage(MessageType.Chat, T(current, "HanHitmarker.Menu.GroupUnavailable"));
                    OpenMainMenu(current);
                    return;
                }
                _runtimeStore.GetOrCreate(current, _config.CurrentValue);
                _runtimeStore.SetSelectedGroup(current, selected.Name);
                current.SendMessage(MessageType.Chat, T(current, "HanHitmarker.Menu.GroupApplied", selected.Name));
                OpenMainMenu(current);
            });
            menu.AddOption(button);
        }
        return menu;
    }

    private Task DispatchAsync(HitmarkerMenuScope.Ticket owner, IMenuAPI menu, IPlayer? clickedPlayer,
        Action<IPlayer> action)
    {
        if (clickedPlayer is not { IsValid: true } || clickedPlayer.PlayerID != owner.PlayerId ||
            clickedPlayer.SessionId != owner.Session || clickedPlayer.SteamID != owner.SteamId)
            return Task.CompletedTask;

        // 同步菜单与运行态 API 没有异步替代，先回游戏线程，再查完整会话和原菜单实例。
        return _core.Scheduler.NextWorldUpdateAsync(() =>
        {
            if (_scope.ResolveCurrent(owner, menu) is not { } current || !_config.CurrentValue.Enabled) return;
            action(current);
        });
    }

    private string BuildSummaryText(IPlayer player)
    {
        var state = _runtimeStore.GetOrCreate(player, _config.CurrentValue);
        return T(player, "HanHitmarker.Menu.Summary", GetDisplayGroupName(player),
            StateText(player, state.CenterEffectEnabled), StateText(player, state.DamageNumberEnabled));
    }

    private string GetDisplayGroupName(IPlayer player)
    {
        var state = _runtimeStore.GetOrCreate(player, _config.CurrentValue);
        var selected = _groups.GetByName(state.SelectedGroupName);
        if (selected is not null && _permission.CanUseGroup(player, selected)) return selected.Name;
        return _permission.GetFirstVisibleGroup(player)?.Name ?? T(player, "HanHitmarker.Menu.NoGroupName");
    }

    private string StateText(IPlayer player, bool enabled) =>
        T(player, enabled ? "HanHitmarker.State.On" : "HanHitmarker.State.Off");

    private string T(IPlayer? player, string key, params object[] args)
    {
        var localizer = player is not null && player.IsValid
            ? _core.Translation.GetPlayerLocalizer(player) : _core.Localizer;
        return args.Length > 0 ? localizer[key, args] : localizer[key];
    }
}
