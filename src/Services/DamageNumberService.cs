using System.Globalization;
using HanHitmarker.Config;
using HanHitmarker.Runtime;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitmarker.Services;

public sealed class DamageNumberService : IDisposable
{
    private readonly ISwiftlyCore _core;
    private readonly IOptionsMonitor<HanHitmarkerConfig> _config;
    private readonly HitmarkerSoundService _sound;
    private readonly HitmarkerLifetime _lifetime;
    private readonly HashSet<uint> _pendingHandles = [];

    public DamageNumberService(
        ISwiftlyCore core,
        IOptionsMonitor<HanHitmarkerConfig> config,
        HitmarkerSoundService sound,
        HitmarkerLifetime lifetime)
    {
        _core = core;
        _config = config;
        _sound = sound;
        _lifetime = lifetime;
    }

    public void Show(HitmarkerHitContext context, HitmarkerParticleGroupConfig group)
    {
        if (context.Attacker.IsFakeClient || !context.AttackerPawn.IsValid || !context.VictimPawn.IsValid)
        {
            return;
        }

        var batch = BuildDigitBatch(context, group);
        if (batch.Count == 0)
        {
            return;
        }

        var particleHandles = SpawnDigitBatch(context.Attacker, batch);
        if (particleHandles.Count == 0)
        {
            return;
        }

        var lifetime = Math.Max(0.05f, _config.CurrentValue.DamageNumberLifetimeSeconds);
        var epoch = _lifetime.Capture();
        _core.Scheduler.NextWorldUpdate(() =>
        {
            if (!_lifetime.IsCurrent(epoch)) return;
            foreach (var handle in particleHandles) _pendingHandles.Remove(handle);
            StartAndKillBatch(particleHandles, lifetime);
        });
        _sound.EmitToPlayer(context.Attacker, group.DamageNumberSound);
    }

    private List<DigitParticleRequest> BuildDigitBatch(
        HitmarkerHitContext context,
        HitmarkerParticleGroupConfig group)
    {
        var config = _config.CurrentValue;
        var digits = Math.Max(0, context.Damage).ToString(CultureInfo.InvariantCulture);
        var basePosition = new Vector(
            context.VictimOrigin.X,
            context.VictimOrigin.Y,
            context.VictimOrigin.Z + config.DamageNumberHeightOffset);

        var forward = Normalized(basePosition - context.AttackerOrigin);
        var up = new Vector(0, 0, 1);
        var viewerRight = Normalized(CrossProduct(forward, up));
        if (IsNearlyZero(viewerRight))
        {
            viewerRight = new Vector(1, 0, 0);
        }

        var spacing = Math.Max(0.0f, config.DamageNumberSpacing);
        var totalWidth = (digits.Length - 1) * spacing;
        var startPosition = basePosition - viewerRight * (totalWidth / 2f);
        var requests = new List<DigitParticleRequest>(digits.Length);

        for (var index = 0; index < digits.Length; index++)
        {
            var digit = digits[index] - '0';
            if (digit < 0 || digit > 9 || digit >= group.DamageDigitParticles.Length)
            {
                continue;
            }

            var particleName = group.DamageDigitParticles[digit];
            if (string.IsNullOrWhiteSpace(particleName))
            {
                continue;
            }

            requests.Add(new DigitParticleRequest(
                particleName.Trim(),
                startPosition + viewerRight * (index * spacing)));
        }

        return requests;
    }

    private List<uint> SpawnDigitBatch(IPlayer viewer, IReadOnlyList<DigitParticleRequest> batch)
    {
        var handles = new List<uint>(batch.Count);
        if (!TryGetViewerContext(viewer, out var controller))
        {
            return handles;
        }

        var playerIds = GetValidPlayerIds();
        foreach (var request in batch)
        {
            var particle = _core.EntitySystem.CreateEntity<CParticleSystem>();
            if (particle is null)
            {
                continue;
            }

            particle.EffectName = request.ParticleName;
            particle.OwnerEntity = controller.OwnerEntity;
            particle.OwnerEntityUpdated();
            particle.DispatchSpawn();

            ConfigureTransmitOnlyToViewer(particle, viewer.PlayerID, playerIds);
            particle.Teleport(request.Position, null, new Vector(0, 0, 0));

            var rawHandle = _core.EntitySystem.GetRefEHandle(particle).Raw;
            handles.Add(rawHandle);
            _pendingHandles.Add(rawHandle);
        }

        return handles;
    }

    private void StartAndKillBatch(IEnumerable<uint> particleHandles, float lifetimeSeconds)
    {
        foreach (var rawHandle in particleHandles)
        {
            var handle = new CHandle<CParticleSystem>(rawHandle);
            var particle = handle.Value;
            if (!handle.IsValid || particle is null || !particle.IsValid || !particle.IsValidEntity)
            {
                continue;
            }

            particle.AcceptInput("Start", 0);
            particle.AddEntityIOEvent("Kill", string.Empty, null, null, lifetimeSeconds);
        }
    }

    public void ClearPending()
    {
        foreach (var rawHandle in _pendingHandles)
        {
            var handle = new CHandle<CParticleSystem>(rawHandle);
            if (handle.IsValid && handle.Value is { IsValid: true, IsValidEntity: true } particle) particle.Despawn();
        }
        _pendingHandles.Clear();
    }

    public void Dispose() => ClearPending();

    private int[] GetValidPlayerIds()
    {
        return _core.PlayerManager
            .GetAllPlayers()
            .Where(player => player is not null && player.IsValid && !player.IsFakeClient)
            .Select(player => player!.PlayerID)
            .ToArray();
    }

    private static void ConfigureTransmitOnlyToViewer(CBaseEntity entity, int viewerPlayerId, IEnumerable<int> playerIds)
    {
        foreach (var playerId in playerIds)
        {
            entity.SetTransmitState(playerId == viewerPlayerId, playerId);
        }
    }

    private static bool TryGetViewerContext(IPlayer viewer, out CCSPlayerController controller)
    {
        controller = null!;

        if (viewer is null || !viewer.IsValid || viewer.IsFakeClient)
        {
            return false;
        }

        if (viewer.Controller is not CCSPlayerController playerController || !playerController.IsValid)
        {
            return false;
        }

        if (viewer.PlayerPawn is not CCSPlayerPawn playerPawn || !playerPawn.IsValid)
        {
            return false;
        }

        controller = playerController;
        return true;
    }

    private static Vector CrossProduct(Vector left, Vector right)
    {
        return new Vector(
            left.Y * right.Z - left.Z * right.Y,
            left.Z * right.X - left.X * right.Z,
            left.X * right.Y - left.Y * right.X);
    }

    private static Vector Normalized(Vector value)
    {
        var length = MathF.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
        if (length <= 0.0001f)
        {
            return new Vector(0, 0, 0);
        }

        return new Vector(value.X / length, value.Y / length, value.Z / length);
    }

    private static bool IsNearlyZero(Vector value)
    {
        return MathF.Abs(value.X) <= 0.0001f &&
               MathF.Abs(value.Y) <= 0.0001f &&
               MathF.Abs(value.Z) <= 0.0001f;
    }

    private readonly record struct DigitParticleRequest(string ParticleName, Vector Position);
}
