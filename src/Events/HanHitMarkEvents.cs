using System.IO;
using System.Runtime;
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
    private readonly IOptionsMonitor<HanHitMarkWorldTextConfigs> _worldtextconfig;
    private readonly HanHitMarkHelpers _helpers;
    private readonly HanHitMarkGlobals _globals;
    private readonly HanHitMarkService _service;
    public HanHitMarkEvents(ISwiftlyCore core,
        ILogger<HanHitMarkEvents> logger,
        IOptionsMonitor<HanHitMarkConfigs> config,
        HanHitMarkHelpers helpers,
        HanHitMarkGlobals globals,
        HanHitMarkService service,
        IOptionsMonitor<HanHitMarkWorldTextConfigs> worldtextconfig)
    {
        _core = core;
        _logger = logger;
        _helpers = helpers;
        _config = config;
        _globals = globals;
        _service = service;
        _worldtextconfig = worldtextconfig;
    }
    public void HookEvents()
    {
        _core.Event.OnPrecacheResource += Event_OnPrecacheResource;
        _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurtMarket);
        _core.GameEvent.HookPre<EventPlayerHurt>(OnPlayerHurtNubmer);
        _core.GameEvent.HookPre<EventBulletImpact>(OnBulletImpact);
    }

    public HookResult OnPlayerHurtMarket(EventPlayerHurt @event)
    {
        var attacker = _core.PlayerManager.GetPlayer(@event.Attacker);
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        var victim = @event.UserIdPlayer;
        if (victim == null || !victim.IsValid)
            return HookResult.Continue;

        var attackerpawn = attacker.PlayerPawn;
        if (attackerpawn == null || !attackerpawn.IsValid)
            return HookResult.Continue;

        var victimpawn = victim.PlayerPawn;
        if (victimpawn == null || !victimpawn.IsValid)
            return HookResult.Continue;

        if(attackerpawn.TeamNum == victimpawn.TeamNum)
            return HookResult.Continue;

        var cfg = _config.CurrentValue;

        var Wtcfg = _worldtextconfig.CurrentValue;

        Vector hitPos;

        if (_globals.lastImpact.TryGetValue(attacker.PlayerID, out var impact) &&
            _core.Engine.GlobalVars.CurrentTime - impact.Time <= 0.2f)
        {
            hitPos = impact.Position;
        }
        else
        {
            hitPos = new Vector(0, 0, 0);
        }

        _globals.lastImpact.Remove(attacker.PlayerID);
        _globals.impactLocked.Remove(attacker.PlayerID);

        bool headshot = @event.HitGroup == 1;

        string path = headshot ? cfg.HitMarkHeadParticles
                                           : cfg.HitMarkBodyParticles;

        if (cfg.HitMarkType == 0)
            _helpers.CreateHitMarkWroldText(attacker, hitPos, Wtcfg, headshot);
        else
            _helpers.CreateHitMark(attacker, hitPos, path);



        _helpers.EmitSoundToPlayer(attacker, headshot ? cfg.HitMarkHeadSound : cfg.HitMarkBodySound);

        return HookResult.Continue;
    }



    public HookResult OnPlayerHurtNubmer(EventPlayerHurt @event)
    {
        var attacker = _core.PlayerManager.GetPlayer(@event.Attacker);
        if (attacker == null || !attacker.IsValid)
            return HookResult.Continue;

        var victim = @event.UserIdPlayer;
        if (victim == null || !victim.IsValid)
            return HookResult.Continue;

        var attackerpawn = attacker.PlayerPawn;
        if (attackerpawn == null || !attackerpawn.IsValid)
            return HookResult.Continue;

        var victimpawn = victim.PlayerPawn;
        if (victimpawn == null || !victimpawn.IsValid)
            return HookResult.Continue;

        if (attackerpawn.TeamNum == victimpawn.TeamNum)
            return HookResult.Continue;

        var cfg = _config.CurrentValue;

        var Wtcfg = _worldtextconfig.CurrentValue;


        bool headshot = @event.HitGroup == 1;
        int damage = @event.DmgHealth;


        _globals.LastHitInfo[attacker.PlayerID] = new HitInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = damage
        };


        if (!cfg.EnabledDamageNumber)
            return HookResult.Continue;

        if (!_helpers.IsTeamAllowed(attacker, cfg.DamageNumberOnlyTeam))
            return HookResult.Continue;

        if (cfg.DamageNumberType == 0)
        {
            _service.ShowDamageWorldText(victim, attacker, Wtcfg);
        }
        else if (cfg.DamageNumberType == 1)
        {
            _service.ShowDamageParticles(victim, attacker);
        }
        else
        {
            _service.ShowDamageWorldText(victim, attacker, Wtcfg);
        }

        _helpers.EmitSoundToPlayer(attacker, cfg.DamageNumberSound);

        return HookResult.Continue;
    }

    public HookResult OnBulletImpact(EventBulletImpact @event)
    {
        var player = @event.UserIdPlayer;
        if (player == null || !player.IsValid)
            return HookResult.Continue;

        if (_globals.impactLocked.Contains(player.PlayerID))
            return HookResult.Continue;

        _globals.lastImpact[player.PlayerID] = new ImpactInfo
        {
            Position = new Vector(@event.X, @event.Y, @event.Z),
            Time = _core.Engine.GlobalVars.CurrentTime
        };

        _globals.impactLocked.Add(player.PlayerID);

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