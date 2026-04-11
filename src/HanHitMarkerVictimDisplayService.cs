using System.Globalization;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerVictimDisplayService
{
    private readonly IOptionsMonitor<HanHitMarkerConfigs> _config;
    private readonly IOptionsMonitor<HanHitMarkerWorldTextConfigs> _worldTextConfig;
    private readonly HanHitMarkerHelpers _helpers;
    private readonly HanHitMarkerGlobals _globals;

    public HanHitMarkerVictimDisplayService(
        IOptionsMonitor<HanHitMarkerConfigs> config,
        IOptionsMonitor<HanHitMarkerWorldTextConfigs> worldTextConfig,
        HanHitMarkerHelpers helpers,
        HanHitMarkerGlobals globals)
    {
        _config = config;
        _worldTextConfig = worldTextConfig;
        _helpers = helpers;
        _globals = globals;
    }

    public void ShowVictimHitMarker(HanHitMarkerHitContext context)
    {
        var config = _config.CurrentValue;
        if (!_globals.IsFeatureEnabledForPlayer(context.Attacker, HanHitMarkerFeature.HitMarker, config) ||
            !_helpers.IsTeamAllowed(context.Attacker, config.HitMarkOnlyTeam))
        {
            return;
        }

        if (UsesParticleMode(config.HitMarkType))
        {
            var particlePath = context.Headshot ? config.HitMarkHeadParticles : config.HitMarkBodyParticles;
            _helpers.CreateHitMarkParticle(context.Attacker, context.ResolvedHitPosition, particlePath);
        }
        else
        {
            _helpers.CreateHitMarkWorldText(context.Attacker, context.ResolvedHitPosition, _worldTextConfig.CurrentValue, context.Headshot);
        }

        _helpers.EmitSoundToPlayer(
            context.Attacker,
            context.Headshot ? config.HitMarkHeadSound : config.HitMarkBodySound);
    }

    public void ShowVictimDamageNumber(HanHitMarkerHitContext context)
    {
        var config = _config.CurrentValue;
        if (!_globals.IsFeatureEnabledForPlayer(context.Attacker, HanHitMarkerFeature.DamageNumber, config) ||
            !_helpers.IsTeamAllowed(context.Attacker, config.DamageNumberOnlyTeam))
        {
            return;
        }

        if (UsesParticleMode(config.DamageNumberType))
        {
            ShowDamageDigitParticles(context);
        }
        else
        {
            ShowDamageWorldText(context);
        }

        _helpers.EmitSoundToPlayer(context.Attacker, config.DamageNumberSound);
    }

    private static bool UsesParticleMode(string? rawType)
    {
        if (string.IsNullOrWhiteSpace(rawType))
        {
            return false;
        }

        return rawType.Trim() switch
        {
            "1" => true,
            var mode when mode.Equals("particles", StringComparison.OrdinalIgnoreCase) => true,
            var mode when mode.Equals("particle", StringComparison.OrdinalIgnoreCase) => true,
            _ => false
        };
    }

    private void ShowDamageDigitParticles(HanHitMarkerHitContext context)
    {
        if (context.Victim.PlayerPawn is not CCSPlayerPawn victimPawn || !victimPawn.IsValid)
        {
            return;
        }

        if (context.Attacker.PlayerPawn is not CCSPlayerPawn attackerPawn || !attackerPawn.IsValid)
        {
            return;
        }

        var victimOrigin = victimPawn.AbsOrigin;
        var attackerOrigin = attackerPawn.AbsOrigin;
        if (victimOrigin is null || attackerOrigin is null)
        {
            return;
        }

        var basePosition = new Vector(victimOrigin.Value.X, victimOrigin.Value.Y, victimOrigin.Value.Z + 5f);
        var digits = _helpers.SplitDigits(context.Damage);
        const float spacing = 13f;
        var totalWidth = (digits.Count - 1) * spacing;
        var forward = _helpers.Normalized(basePosition - attackerOrigin.Value);
        var up = new Vector(0, 0, 1);
        var right = _helpers.Normalized(_helpers.CrossProduct(up, forward));
        var startPosition = basePosition - right * (totalWidth / 2f);

        for (var index = 0; index < digits.Count; index++)
        {
            var digitIndex = digits.Count - 1 - index;
            var digitPosition = startPosition + right * (index * spacing);
            if (!_globals.DigitParticles.TryGetValue(digits[digitIndex], out var particleName) ||
                string.IsNullOrWhiteSpace(particleName))
            {
                continue;
            }

            _helpers.SpawnParticleAtPosition(context.Attacker, digitPosition, particleName);
        }
    }

    private void ShowDamageWorldText(HanHitMarkerHitContext context)
    {
        var damageState = _globals.GetOrCreateDamageState(context.Attacker);
        _helpers.SpawnDamageWorldText(
            context.Attacker,
            context.Victim,
            context.Damage.ToString(CultureInfo.InvariantCulture),
            _worldTextConfig.CurrentValue,
            context.Headshot,
            damageState);
    }
}
