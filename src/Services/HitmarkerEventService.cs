using HanHitmarker.Config;
using HanHitmarker.Runtime;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitmarker.Services;

public sealed class HitmarkerEventService
{
    private readonly ISwiftlyCore _core;
    private readonly IOptionsMonitor<HanHitmarkerConfig> _config;
    private readonly HitmarkerRuntimeStore _runtimeStore;
    private readonly HitmarkerParticleGroupService _groups;
    private readonly HitmarkerPermissionService _permission;
    private readonly HitmarkerResourceService _resources;
    private readonly CenterParticleService _centerParticles;
    private readonly DamageNumberService _damageNumbers;
    private readonly HitmarkerLifetime _lifetime;
    private Guid? _playerHurtHookId;
    private bool _hooked;

    public HitmarkerEventService(
        ISwiftlyCore core,
        IOptionsMonitor<HanHitmarkerConfig> config,
        HitmarkerRuntimeStore runtimeStore,
        HitmarkerParticleGroupService groups,
        HitmarkerPermissionService permission,
        HitmarkerResourceService resources,
        CenterParticleService centerParticles,
        DamageNumberService damageNumbers,
        HitmarkerLifetime lifetime)
    {
        _core = core;
        _config = config;
        _runtimeStore = runtimeStore;
        _groups = groups;
        _permission = permission;
        _resources = resources;
        _centerParticles = centerParticles;
        _damageNumbers = damageNumbers;
        _lifetime = lifetime;
    }

    public void HookEvents()
    {
        if (_hooked)
        {
            return;
        }

        _core.Event.OnPrecacheResource += OnPrecacheResource;
        _core.Event.OnClientPutInServer += OnClientPutInServer;
        _core.Event.OnClientDisconnected += OnClientDisconnected;
        _core.Event.OnMapLoad += OnMapLoad;
        _core.Event.OnMapUnload += OnMapUnload;

        _playerHurtHookId = _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurt);
        _hooked = true;
    }

    public void UnhookEvents()
    {
        if (!_hooked)
        {
            return;
        }

        _core.Event.OnPrecacheResource -= OnPrecacheResource;
        _core.Event.OnClientPutInServer -= OnClientPutInServer;
        _core.Event.OnClientDisconnected -= OnClientDisconnected;
        _core.Event.OnMapLoad -= OnMapLoad;
        _core.Event.OnMapUnload -= OnMapUnload;

        if (_playerHurtHookId.HasValue)
        {
            _core.GameEvent.Unhook(_playerHurtHookId.Value);
            _playerHurtHookId = null;
        }

        _hooked = false;
    }

    public HookResult OnPlayerHurt(EventPlayerHurt @event)
    {
        var config = _config.CurrentValue;
        if (!config.Enabled || !TryBuildHitContext(@event, out var context))
        {
            return HookResult.Continue;
        }

        var state = _runtimeStore.GetOrCreate(context.Attacker, config);
        var group = ResolveGroup(context.Attacker, state);
        if (group is null)
        {
            return HookResult.Continue;
        }

        if (!state.SelectedGroupName.Equals(group.Name, StringComparison.OrdinalIgnoreCase))
        {
            state.SelectedGroupName = group.Name;
        }

        if (state.CenterEffectEnabled)
        {
            _centerParticles.Show(context, group);
        }

        if (state.DamageNumberEnabled)
        {
            _damageNumbers.Show(context, group);
        }

        return HookResult.Continue;
    }

    private void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        _resources.OnPrecacheResource(@event);
    }

    private void OnClientPutInServer(IOnClientPutInServerEvent @event)
    {
        var player = _core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player is null || !player.IsValid || player.IsFakeClient)
        {
            return;
        }

        _runtimeStore.GetOrCreate(player, _config.CurrentValue);
    }

    private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
    {
        _runtimeStore.RemovePlayerRuntime(@event.PlayerId);
    }

    private void OnMapLoad(IOnMapLoadEvent @event)
    {
        _runtimeStore.ClearMapScopedState();
    }

    private void OnMapUnload(IOnMapUnloadEvent @event)
    {
        _lifetime.Invalidate();
        _damageNumbers.ClearPending();
        _runtimeStore.ClearMapScopedState();
    }

    private HitmarkerParticleGroupConfig? ResolveGroup(IPlayer attacker, PlayerHitmarkerState state)
    {
        var selected = _groups.GetByName(state.SelectedGroupName);
        if (selected is not null && _permission.CanUseGroup(attacker, selected))
        {
            return selected;
        }

        return _permission.GetFirstVisibleGroup(attacker);
    }

    private bool TryBuildHitContext(EventPlayerHurt @event, out HitmarkerHitContext context)
    {
        context = null!;

        var attacker = _core.PlayerManager.GetPlayer(@event.Attacker);
        var victim = @event.UserIdPlayer;
        if (attacker is null || !attacker.IsValid || attacker.IsFakeClient ||
            victim is null || !victim.IsValid)
        {
            return false;
        }

        if (attacker.PlayerID == victim.PlayerID)
        {
            return false;
        }

        if (attacker.PlayerPawn is not CCSPlayerPawn attackerPawn || !attackerPawn.IsValid)
        {
            return false;
        }

        if (victim.PlayerPawn is not CCSPlayerPawn victimPawn || !victimPawn.IsValid)
        {
            return false;
        }

        var damage = @event.ActualDmgHealth;
        if (attackerPawn.TeamNum == victimPawn.TeamNum || damage <= 0)
        {
            return false;
        }

        var attackerOrigin = attackerPawn.AbsOrigin;
        var victimOrigin = victimPawn.AbsOrigin;
        if (attackerOrigin is null || victimOrigin is null)
        {
            return false;
        }

        context = new HitmarkerHitContext(
            attacker,
            victim,
            attackerPawn,
            victimPawn,
            damage,
            @event.ActualHitGroup == HitGroup_t.HITGROUP_HEAD,
            attackerOrigin.Value,
            victimOrigin.Value);

        return true;
    }
}
