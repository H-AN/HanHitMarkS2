using HanHitmarker.Config;
using HanHitmarker.Runtime;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitmarker.Services;

public sealed class CenterParticleService
{
    private const ParticleAttachment_t DefaultAttachmentType = ParticleAttachment_t.PATTACH_ABSORIGIN_FOLLOW;
    private const byte DefaultAttachmentPoint = 0;
    private const bool DefaultResetAllParticlesOnEntity = false;
    private const int DefaultSplitScreenSlot = 0;

    private readonly ISwiftlyCore _core;
    private readonly HitmarkerSoundService _sound;

    public CenterParticleService(ISwiftlyCore core, HitmarkerSoundService sound)
    {
        _core = core;
        _sound = sound;
    }

    public void Show(HitmarkerHitContext context, HitmarkerParticleGroupConfig group)
    {
        var particleName = context.Headshot
            ? group.CenterHeadParticle
            : group.CenterBodyParticle;

        if (string.IsNullOrWhiteSpace(particleName) || !context.AttackerPawn.IsValid)
        {
            return;
        }

        var filter = CRecipientFilter.FromSingle(context.Attacker.PlayerID);

        _core.Engine.DispatchParticleEffect(
            particleName.Trim(),
            DefaultAttachmentType,
            DefaultAttachmentPoint,
            default,
            filter,
            DefaultResetAllParticlesOnEntity,
            DefaultSplitScreenSlot,
            context.AttackerPawn);

        _sound.EmitToPlayer(
            context.Attacker,
            context.Headshot ? group.HitHeadSound : group.HitBodySound);
    }
}
