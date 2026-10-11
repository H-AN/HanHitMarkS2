using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Sounds;
using HanHitmarker.Runtime;

namespace HanHitmarker.Services;

public sealed class HitmarkerSoundService
{
    private readonly ISwiftlyCore _core;
    private readonly HitmarkerLifetime _lifetime;

    public HitmarkerSoundService(ISwiftlyCore core, HitmarkerLifetime lifetime)
    {
        _core = core;
        _lifetime = lifetime;
    }

    public void EmitToPlayer(IPlayer player, string? soundName)
    {
        if (player is null || !player.IsValid || player.IsFakeClient || string.IsNullOrWhiteSpace(soundName))
        {
            return;
        }

        var playerId = player.PlayerID;
        var sessionId = player.SessionId;
        var steamId = player.SteamID;
        var epoch = _lifetime.Capture();
        var trimmedSoundName = soundName.Trim();

        _core.Scheduler.NextTick(() =>
        {
            if (!_lifetime.IsCurrent(epoch)) return;
            var current = _core.PlayerManager.GetPlayerFromSessionId(sessionId);
            if (current is not { IsValid: true, IsFakeClient: false } || current.PlayerID != playerId || current.SteamID != steamId) return;
            using var sound = new SoundEvent(trimmedSoundName, 1.0f, 1.0f)
            {
                SourceEntityIndex = -1
            };
            sound.Recipients.AddRecipient(playerId);
            sound.Emit();
        });
    }
}
