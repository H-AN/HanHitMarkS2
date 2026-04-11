using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerHitContext
{
    public HanHitMarkerHitContext(
        IPlayer attacker,
        IPlayer victim,
        int damage,
        bool headshot,
        Vector victimPosition,
        Vector resolvedHitPosition)
    {
        Attacker = attacker;
        Victim = victim;
        Damage = damage;
        Headshot = headshot;
        VictimPosition = victimPosition;
        ResolvedHitPosition = resolvedHitPosition;
    }

    public IPlayer Attacker { get; }
    public IPlayer Victim { get; }
    public int Damage { get; }
    public bool Headshot { get; }
    public Vector VictimPosition { get; }
    public Vector ResolvedHitPosition { get; }
}
