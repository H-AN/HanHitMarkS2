namespace HanHitmarker.Config;

public sealed class HanHitmarkerConfig
{
    public bool Enabled { get; set; } = true;
    public string MenuCommand { get; set; } = "sw_hm";
    public string DefaultParticleGroup { get; set; } = "默认";
    public bool PlayerDefaultCenterEffectEnabled { get; set; } = true;
    public bool PlayerDefaultDamageNumberEnabled { get; set; } = true;
    public float DamageNumberHeightOffset { get; set; } = 5.0f;
    public float DamageNumberSpacing { get; set; } = 13.0f;
    public float DamageNumberLifetimeSeconds { get; set; } = 0.4f;

    public string[] PrecacheSoundEvents { get; set; } = [];
    public List<HitmarkerParticleGroupConfig> ParticleGroups { get; set; } = [];

    public static HanHitmarkerConfig CreateDefault() => new()
    {
        PrecacheSoundEvents = ["soundevents/hanhmsounds.vsndevts"],
        ParticleGroups = [HitmarkerParticleGroupConfig.CreateDefault()]
    };
}
