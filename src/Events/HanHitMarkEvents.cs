using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;

namespace HanHitMarkS2;

public class HanHitMarkEvents
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HanHitMarkEvents> _logger;
    private readonly IOptionsMonitor<HanHitMarkConfigs> _config;
    private readonly HanHitMarkHelpers _helpers;
    private readonly HanHitMarkGlobals _globals;
    private readonly HanHitMarkService _service;
    public HanHitMarkEvents(ISwiftlyCore core,
        ILogger<HanHitMarkEvents> logger,
        IOptionsMonitor<HanHitMarkConfigs> config,
        HanHitMarkHelpers helpers,
        HanHitMarkGlobals globals,
        HanHitMarkService service)
    {
        _core = core;
        _logger = logger;
        _helpers = helpers;
        _config = config;
        _globals = globals;
        _service = service;
    }
    public void HookEvents()
    {
        _core.Event.OnPrecacheResource += Event_OnPrecacheResource;
        _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurt);
        _core.GameEvent.HookPre<EventBulletImpact>(OnBulletImpact);
    }


    public HookResult OnPlayerHurt(EventPlayerHurt @event)
    {
        var attacker = _core.PlayerManager.GetPlayer(@event.Attacker);
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        var victim = @event.UserIdPlayer;
        if (victim == null || !victim.IsValid)
            return HookResult.Continue;

        var cfg = _config.CurrentValue;
        

        bool headshot = @event.HitGroup == 1;
        int damage = @event.DmgHealth;

        _globals.LastHitInfo[attacker.PlayerID] = new HitInfo
        {
            Attacker = attacker,
            Victim = victim,
            Headshot = headshot,
            Damage = damage
        };

        if (!cfg.EnabledDamageNumber)
            return HookResult.Continue;

        if (!_helpers.IsTeamAllowed(attacker, cfg.DamageNumberOnlyTeam))
            return HookResult.Continue;

        _service.ShowDamageParticles(victim, attacker);
        _helpers.EmitSoundToPlayer(attacker, cfg.DamageNumberSound);

        return HookResult.Continue;
    }
    

    public HookResult OnBulletImpact(EventBulletImpact @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        var cfg = _config.CurrentValue;
        

        Vector HitPos = new Vector(@event.X, @event.Y, @event.Z);

        if (_globals.LastHitInfo.TryGetValue(player.PlayerID, out var hitInfo))
        {
            string path = hitInfo.Headshot ? cfg.HitMarkHeadParticles
                                           : cfg.HitMarkBodyParticles;

            if (!cfg.EnabledHitMark)
                return HookResult.Continue;

            if (!_helpers.IsTeamAllowed(player, cfg.HitMarkOnlyTeam))
                return HookResult.Continue;

            _helpers.CreateHitMark(player, HitPos, path);
            _globals.LastHitInfo.Remove(player.PlayerID);

            string soundpath = hitInfo.Headshot ? cfg.HitMarkHeadSound
                                           : cfg.HitMarkBodySound;

            _helpers.EmitSoundToPlayer(player, soundpath);
        }

        

        return HookResult.Continue;
    }

    

    private void Event_OnPrecacheResource(SwiftlyS2.Shared.Events.IOnPrecacheResourceEvent @event)
    {
        var cfg = _config.CurrentValue;

        _helpers.PrecacheSoundEventIfValid(@event, cfg);

        _helpers.PrecacheIfValid(@event,
            cfg.HitMarkHeadParticles,
            cfg.HitMarkBodyParticles,
            cfg.HitMarkHeadSound,
            cfg.HitMarkBodySound,
            cfg.DamageNumberSound);

        _helpers.PrecacheIfValid(@event,
            cfg.DamageNumberParticles0,
            cfg.DamageNumberParticles1,
            cfg.DamageNumberParticles2,
            cfg.DamageNumberParticles3,
            cfg.DamageNumberParticles4,
            cfg.DamageNumberParticles5,
            cfg.DamageNumberParticles6,
            cfg.DamageNumberParticles7,
            cfg.DamageNumberParticles8,
            cfg.DamageNumberParticles9);
    }

}