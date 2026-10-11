using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitmarker.Runtime;

public sealed class HitmarkerHitContext
{
    public HitmarkerHitContext(
        IPlayer attacker,
        IPlayer victim,
        CCSPlayerPawn attackerPawn,
        CCSPlayerPawn victimPawn,
        int damage,
        bool headshot,
        Vector attackerOrigin,
        Vector victimOrigin)
    {
        Attacker = attacker;
        Victim = victim;
        AttackerPawn = attackerPawn;
        VictimPawn = victimPawn;
        Damage = damage;
        Headshot = headshot;
        AttackerOrigin = attackerOrigin;
        VictimOrigin = victimOrigin;
    }

    public IPlayer Attacker { get; }
    public IPlayer Victim { get; }
    public CCSPlayerPawn AttackerPawn { get; }
    public CCSPlayerPawn VictimPawn { get; }
    public int Damage { get; }
    public bool Headshot { get; }
    public Vector AttackerOrigin { get; }
    public Vector VictimOrigin { get; }
}
