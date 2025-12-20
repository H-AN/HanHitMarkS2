using SwiftlyS2.Shared.Players;

namespace HanHitMarkS2;

public class HanHitMarkGlobals
{
    public Dictionary<int, HitInfo> LastHitInfo = new();
    public Dictionary<int, string> DigitParticles = new Dictionary<int, string>();
    public void LoadDigitParticles(HanHitMarkConfigs cfg)
    {
        DigitParticles.Clear();
        DigitParticles[0] = cfg.DamageNumberParticles0;
        DigitParticles[1] = cfg.DamageNumberParticles1;
        DigitParticles[2] = cfg.DamageNumberParticles2;
        DigitParticles[3] = cfg.DamageNumberParticles3;
        DigitParticles[4] = cfg.DamageNumberParticles4;
        DigitParticles[5] = cfg.DamageNumberParticles5;
        DigitParticles[6] = cfg.DamageNumberParticles6;
        DigitParticles[7] = cfg.DamageNumberParticles7;
        DigitParticles[8] = cfg.DamageNumberParticles8;
        DigitParticles[9] = cfg.DamageNumberParticles9;
    }
}

public class HitInfo
{
    public IPlayer? Attacker;
    public IPlayer? Victim;
    public bool Headshot;
    public int Damage;
}