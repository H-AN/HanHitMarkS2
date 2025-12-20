using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;

namespace HanHitMarkS2;

public class HanHitMarkService
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HanHitMarkService> _logger;
    private readonly HanHitMarkHelpers _helpers;
    private readonly HanHitMarkGlobals _globals;
    public HanHitMarkService(ISwiftlyCore core,
        ILogger<HanHitMarkService> logger,
        HanHitMarkHelpers helpers,
        HanHitMarkGlobals globals)
    {
        _core = core;
        _logger = logger;
        _helpers = helpers;
        _globals = globals;
    }


    public void ShowDamageParticles(IPlayer victim, IPlayer attacker)
    {
        if (victim == null || !victim.IsValid || attacker == null || !attacker.IsValid)
            return;

        var victimPawn = victim.PlayerPawn;
        if (victimPawn == null || !victimPawn.IsValid)
            return;

        var attackerPawn = attacker.PlayerPawn;
        if (attackerPawn == null || !attackerPawn.IsValid)
            return;

        var victimOrigin = victimPawn.AbsOrigin;
        var attackerOrigin = attackerPawn.AbsOrigin;

        if (victimOrigin == null || attackerOrigin == null)
            return;

        if (!_globals.LastHitInfo.TryGetValue(attacker.PlayerID, out var hitInfo))
            return;

        Vector basePos = new Vector(victimOrigin.Value.X, victimOrigin.Value.Y, victimOrigin.Value.Z + 5);
        List<int> digits = _helpers.SplitDigits(hitInfo.Damage);

        float spacing = 13f;
        float totalWidth = (digits.Count - 1) * spacing;
        Vector forward = _helpers.Normalized((SwiftlyS2.Shared.Natives.Vector)basePos - (SwiftlyS2.Shared.Natives.Vector)attackerOrigin);
        Vector up = new Vector(0, 0, 1); 
        Vector right = _helpers.Normalized(_helpers.CrossProduct(up, forward)); 
        Vector startPos = basePos - right * (totalWidth / 2);

        for (int i = 0; i < digits.Count; i++)
        {
            int digitIndex = digits.Count - 1 - i; 
            Vector offset = right * (i * spacing);
            Vector digitPos = startPos + offset;

            if (_globals.DigitParticles.TryGetValue(digits[digitIndex], out string particleName))
            {
                if (string.IsNullOrWhiteSpace(particleName))
                    continue;

                _helpers.SpawnParticleAtPosition(attacker, digitPos, particleName);
            }

        }
    }

}