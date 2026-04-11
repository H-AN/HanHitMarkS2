using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerEvents
{
    private readonly ISwiftlyCore _core;
    private readonly IOptionsMonitor<HanHitMarkerConfigs> _config;
    private readonly HanHitMarkerGlobals _globals;
    private readonly HanHitMarkerHelpers _helpers;
    private readonly HanHitMarkerVictimDisplayService _victimDisplayService;
    private readonly HanHitMarkerScreenEffectService _screenEffectService;
    private Guid? _playerHurtHookId;
    private Guid? _bulletImpactHookId;
    private bool _hooked;

    public HanHitMarkerEvents(
        ISwiftlyCore core,
        IOptionsMonitor<HanHitMarkerConfigs> config,
        HanHitMarkerGlobals globals,
        HanHitMarkerHelpers helpers,
        HanHitMarkerVictimDisplayService victimDisplayService,
        HanHitMarkerScreenEffectService screenEffectService)
    {
        _core = core;
        _config = config;
        _globals = globals;
        _helpers = helpers;
        _victimDisplayService = victimDisplayService;
        _screenEffectService = screenEffectService;
    }

    public void HookEvents()
    {
        if (_hooked)
        {
            return;
        }

        _core.Event.OnPrecacheResource += OnPrecacheResource;
        _core.Event.OnClientConnected += OnClientConnected;
        _core.Event.OnClientDisconnected += OnClientDisconnected;
        _core.Event.OnMapLoad += OnMapLoad;
        _core.Event.OnMapUnload += OnMapUnload;

        _playerHurtHookId = _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurt);
        _bulletImpactHookId = _core.GameEvent.HookPre<EventBulletImpact>(OnBulletImpact);
        _hooked = true;
    }

    public void UnhookEvents()
    {
        if (!_hooked)
        {
            return;
        }

        _core.Event.OnPrecacheResource -= OnPrecacheResource;
        _core.Event.OnClientConnected -= OnClientConnected;
        _core.Event.OnClientDisconnected -= OnClientDisconnected;
        _core.Event.OnMapLoad -= OnMapLoad;
        _core.Event.OnMapUnload -= OnMapUnload;

        if (_playerHurtHookId.HasValue)
        {
            _core.GameEvent.Unhook(_playerHurtHookId.Value);
            _playerHurtHookId = null;
        }

        if (_bulletImpactHookId.HasValue)
        {
            _core.GameEvent.Unhook(_bulletImpactHookId.Value);
            _bulletImpactHookId = null;
        }

        _hooked = false;
    }

    public HookResult OnPlayerHurt(EventPlayerHurt @event)
    {
        if (!TryBuildHitContext(@event, out var context))
        {
            return HookResult.Continue;
        }

        _globals.LastHitInfo[context.Attacker.PlayerID] = new DamageHitInfo
        {
            Damage = context.Damage,
            Headshot = context.Headshot
        };

        _victimDisplayService.ShowVictimHitMarker(context);
        _victimDisplayService.ShowVictimDamageNumber(context);
        _screenEffectService.ShowConfiguredAttackerScreenHitEffect(context);

        _globals.LastImpactByPlayerId.Remove(context.Attacker.PlayerID);
        _globals.ImpactLockedPlayerIds.Remove(context.Attacker.PlayerID);
        return HookResult.Continue;
    }

    public HookResult OnBulletImpact(EventBulletImpact @event)
    {
        var player = @event.UserIdPlayer;
        if (player is null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        var playerId = player.PlayerID;
        if (_globals.ImpactLockedPlayerIds.Contains(playerId) &&
            _globals.LastImpactByPlayerId.TryGetValue(playerId, out var existingImpact) &&
            _core.Engine.GlobalVars.CurrentTime - existingImpact.Time <= 0.2f)
        {
            return HookResult.Continue;
        }

        _globals.ImpactLockedPlayerIds.Remove(playerId);
        _globals.LastImpactByPlayerId[playerId] = new ImpactInfo(
            new Vector(@event.X, @event.Y, @event.Z),
            _core.Engine.GlobalVars.CurrentTime);
        _globals.ImpactLockedPlayerIds.Add(playerId);

        return HookResult.Continue;
    }

    private void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        var config = _config.CurrentValue;
        _helpers.PrecacheSoundEventIfValid(@event, config);

        _helpers.PrecacheIfValid(
            @event,
            config.ScreenHitEffectHeadParticle,
            config.ScreenHitEffectBodyParticle,
            config.HitMarkHeadParticles,
            config.HitMarkBodyParticles,
            config.HitMarkHeadSound,
            config.HitMarkBodySound,
            config.DamageNumberSound);

        _helpers.PrecacheIfValid(
            @event,
            config.DamageNumberParticles0,
            config.DamageNumberParticles1,
            config.DamageNumberParticles2,
            config.DamageNumberParticles3,
            config.DamageNumberParticles4,
            config.DamageNumberParticles5,
            config.DamageNumberParticles6,
            config.DamageNumberParticles7,
            config.DamageNumberParticles8,
            config.DamageNumberParticles9);
    }

    private void OnClientConnected(IOnClientConnectedEvent @event)
    {
        var player = _core.PlayerManager.GetPlayer(@event.PlayerId);
        if (player is null || !player.IsValid)
        {
            return;
        }

        _globals.EnsurePlayerFeatureState(player, _config.CurrentValue);
    }

    private void OnClientDisconnected(IOnClientDisconnectedEvent @event)
    {
        _globals.RemovePlayerRuntime(@event.PlayerId);
    }

    private void OnMapLoad(IOnMapLoadEvent @event)
    {
        _globals.ClearMapScopedState();
    }

    private void OnMapUnload(IOnMapUnloadEvent @event)
    {
        _globals.ClearMapScopedState();
    }

    private bool TryBuildHitContext(EventPlayerHurt @event, out HanHitMarkerHitContext context)
    {
        context = null!;

        var attacker = _core.PlayerManager.GetPlayer(@event.Attacker);
        var victim = @event.UserIdPlayer;
        if (attacker is null || !attacker.IsValid || victim is null || !victim.IsValid)
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

        if (attackerPawn.TeamNum == victimPawn.TeamNum || @event.DmgHealth <= 0)
        {
            return false;
        }

        var victimPosition = victimPawn.AbsOrigin;
        if (victimPosition is null)
        {
            return false;
        }

        var resolvedHitPosition = ResolveHitPosition(attacker, victimPosition.Value);
        context = new HanHitMarkerHitContext(
            attacker,
            victim,
            @event.DmgHealth,
            @event.HitGroup == 1,
            victimPosition.Value,
            resolvedHitPosition);

        return true;
    }

    private Vector ResolveHitPosition(SwiftlyS2.Shared.Players.IPlayer attacker, Vector victimPosition)
    {
        if (_globals.LastImpactByPlayerId.TryGetValue(attacker.PlayerID, out var impact) &&
            _core.Engine.GlobalVars.CurrentTime - impact.Time <= 0.2f)
        {
            return impact.Position;
        }

        return victimPosition;
    }
}
