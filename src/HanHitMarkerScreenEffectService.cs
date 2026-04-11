using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerScreenEffectService
{
    private const ParticleAttachment_t DefaultAttachmentType = ParticleAttachment_t.PATTACH_ABSORIGIN_FOLLOW;
    private const byte DefaultAttachmentPoint = 0;
    private const bool DefaultResetAllParticlesOnEntity = false;
    private const int DefaultSplitScreenSlot = 0;

    private readonly ISwiftlyCore _core;
    private readonly IOptionsMonitor<HanHitMarkerConfigs> _config;
    private readonly HanHitMarkerGlobals _globals;
    private readonly HanHitMarkerHelpers _helpers;

    public HanHitMarkerScreenEffectService(
        ISwiftlyCore core,
        IOptionsMonitor<HanHitMarkerConfigs> config,
        HanHitMarkerGlobals globals,
        HanHitMarkerHelpers helpers)
    {
        _core = core;
        _config = config;
        _globals = globals;
        _helpers = helpers;
    }

    public void ShowConfiguredAttackerScreenHitEffect(HanHitMarkerHitContext context)
    {
        ShowAttackerScreenHitEffect(context);
    }

    public void ShowAttackerScreenHitEffect(HanHitMarkerHitContext context)
    {
        if (!TryBuildDispatchRequest(context, out var request))
        {
            return;
        }

        _core.Engine.DispatchParticleEffect(
            request.ParticleName,
            request.AttachmentType,
            request.AttachmentPoint,
            request.AttachmentName,
            request.Filter,
            request.ResetAllParticlesOnEntity,
            request.SplitScreenSlot,
            request.Entity);
    }

    public CRecipientFilter BuildRecipientFilter(SwiftlyS2.Shared.Players.IPlayer attacker)
    {
        return CRecipientFilter.FromSingle(attacker.PlayerID);
    }

    private bool TryBuildDispatchRequest(HanHitMarkerHitContext context, out ScreenEffectDispatchRequest request)
    {
        request = default;

        var config = _config.CurrentValue;
        var particleName = context.Headshot
            ? config.ScreenHitEffectHeadParticle
            : config.ScreenHitEffectBodyParticle;

        if (!_globals.IsFeatureEnabledForPlayer(context.Attacker, HanHitMarkerFeature.ScreenHitEffect, config) ||
            !_helpers.IsTeamAllowed(context.Attacker, config.ScreenHitEffectOnlyTeam) ||
            string.IsNullOrWhiteSpace(particleName))
        {
            return false;
        }

        if (context.Attacker.PlayerPawn is not CCSPlayerPawn attackerPawn || !attackerPawn.IsValid)
        {
            return false;
        }

        request = new ScreenEffectDispatchRequest(
            particleName.Trim(),
            DefaultAttachmentType,
            DefaultAttachmentPoint,
            default,
            BuildRecipientFilter(context.Attacker),
            DefaultResetAllParticlesOnEntity,
            DefaultSplitScreenSlot,
            attackerPawn);

        return true;
    }

    private readonly record struct ScreenEffectDispatchRequest(
        string ParticleName,
        ParticleAttachment_t AttachmentType,
        byte AttachmentPoint,
        CUtlSymbolLarge AttachmentName,
        CRecipientFilter Filter,
        bool ResetAllParticlesOnEntity,
        int SplitScreenSlot,
        CBaseEntity Entity);
}
