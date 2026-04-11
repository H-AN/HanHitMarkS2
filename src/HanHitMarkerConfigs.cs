namespace HanHitMarkerS2;

public sealed class HanHitMarkerConfigs
{
    public bool EnabledHitMark { get; set; } = true;
    public string HitMarkType { get; set; } = "particles";
    public string HitMarkOnlyTeam { get; set; } = "any";
    public string HitMarkHeadParticles { get; set; } = "particles/exg/exg_hitmarker2.vpcf";
    public string HitMarkBodyParticles { get; set; } = "particles/exg/exg_hitmarker.vpcf";
    public string HitMarkHeadSound { get; set; } = "Breakable.Flesh";
    public string HitMarkBodySound { get; set; } = "Flesh_Bloody.ImpactHard";
    public string HitMarkerFeaturePermission { get; set; } = string.Empty;

    public bool EnabledDamageNumber { get; set; } = true;
    public string DamageNumberType { get; set; } = "particles";
    public string DamageNumberOnlyTeam { get; set; } = "any";
    public string DamageNumberParticles0 { get; set; } = "particles/exg/hitmarker/0.vpcf";
    public string DamageNumberParticles1 { get; set; } = "particles/exg/hitmarker/01.vpcf";
    public string DamageNumberParticles2 { get; set; } = "particles/exg/hitmarker/02.vpcf";
    public string DamageNumberParticles3 { get; set; } = "particles/exg/hitmarker/03.vpcf";
    public string DamageNumberParticles4 { get; set; } = "particles/exg/hitmarker/04.vpcf";
    public string DamageNumberParticles5 { get; set; } = "particles/exg/hitmarker/05.vpcf";
    public string DamageNumberParticles6 { get; set; } = "particles/exg/hitmarker/06.vpcf";
    public string DamageNumberParticles7 { get; set; } = "particles/exg/hitmarker/07.vpcf";
    public string DamageNumberParticles8 { get; set; } = "particles/exg/hitmarker/08.vpcf";
    public string DamageNumberParticles9 { get; set; } = "particles/exg/hitmarker/09.vpcf";
    public string DamageNumberSound { get; set; } = "ceiling_tile.BulletImpact";
    public string DamageNumberFeaturePermission { get; set; } = string.Empty;

    public bool EnabledScreenHitEffect { get; set; } = true;
    public string ScreenHitEffectOnlyTeam { get; set; } = "any";
    public string ScreenHitEffectHeadParticle { get; set; } = "particles/cgmentos/hitmarker/overlay_hitmarker_head.vpcf";
    public string ScreenHitEffectBodyParticle { get; set; } = "particles/cgmentos/hitmarker/overlay_hitmarker_body.vpcf";
    public string ScreenHitEffectFeaturePermission { get; set; } = string.Empty;

    public bool PlayerDefaultHitMarkerEnabled { get; set; } = true;
    public bool PlayerDefaultDamageNumberEnabled { get; set; } = true;
    public bool PlayerDefaultScreenHitEffectEnabled { get; set; } = true;

    public string HitMarkerToggleCommand { get; set; } = "sw_hitmarker";
    public string DamageNumberToggleCommand { get; set; } = "sw_damage";
    public string ScreenHitEffectToggleCommand { get; set; } = "sw_screenhitmarker";
    public string HitMarkerCommandPermission { get; set; } = string.Empty;
    public string DamageNumberCommandPermission { get; set; } = string.Empty;
    public string ScreenHitEffectCommandPermission { get; set; } = string.Empty;

    public string PrecacheSoundEvent { get; set; } = "soundevents/game_sounds_physics.vsndevts";
}
