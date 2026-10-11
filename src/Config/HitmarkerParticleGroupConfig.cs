namespace HanHitmarker.Config;

public sealed class HitmarkerParticleGroupConfig
{
    public bool Enabled { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public string AdminFlag { get; set; } = string.Empty;
    public string CenterHeadParticle { get; set; } = "particles/han/hitmarker/center_hitmarker_head.vpcf";
    public string CenterBodyParticle { get; set; } = "particles/han/hitmarker/center_hitmarker_body.vpcf";

    // 配置绑定会向已有集合追加，因此绑定模型与默认配置模板分开创建。
    public string[] DamageDigitParticles { get; set; } = [];

    public string HitHeadSound { get; set; } = "han.hit.head";
    public string HitBodySound { get; set; } = "han.hit.body";
    public string DamageNumberSound { get; set; } = "han.hit.number";

    public static HitmarkerParticleGroupConfig CreateDefault()
    {
        return new HitmarkerParticleGroupConfig
        {
            Name = "默认",
            DamageDigitParticles = Enumerable.Range(0, 10)
                .Select(digit => $"particles/han/number/art/{digit:00}.vpcf")
                .ToArray()
        };
    }
}
