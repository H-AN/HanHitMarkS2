using HanHitmarker.Config;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

namespace HanHitmarker.Runtime;

public sealed class HitmarkerRuntimeStore
{
    private const ulong FallbackKeyMask = 1UL << 63;

    private readonly ISwiftlyCore _core;
    private readonly Dictionary<int, ulong> _runtimeKeysByPlayerId = [];
    private readonly Dictionary<ulong, PlayerHitmarkerState> _states = [];

    public HitmarkerRuntimeStore(ISwiftlyCore core)
    {
        _core = core;
    }

    public PlayerHitmarkerState GetOrCreate(IPlayer player, HanHitmarkerConfig config)
    {
        var runtimeKey = ResolveRuntimeKey(player);
        _runtimeKeysByPlayerId[player.PlayerID] = runtimeKey;

        if (_states.TryGetValue(runtimeKey, out var state))
        {
            EnsureValidSelection(state, config);
            return state;
        }

        state = new PlayerHitmarkerState
        {
            SelectedGroupName = ResolveDefaultGroupName(config),
            CenterEffectEnabled = config.PlayerDefaultCenterEffectEnabled,
            DamageNumberEnabled = config.PlayerDefaultDamageNumberEnabled
        };

        _states[runtimeKey] = state;
        return state;
    }

    public void SeedConnectedPlayers(HanHitmarkerConfig config)
    {
        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player is null || !player.IsValid || player.IsFakeClient)
            {
                continue;
            }

            GetOrCreate(player, config);
        }
    }

    public void SetSelectedGroup(IPlayer player, string groupName)
    {
        if (!_states.TryGetValue(ResolveRuntimeKey(player), out var state))
        {
            return;
        }

        state.SelectedGroupName = groupName.Trim();
    }

    public void SetCenterEffectEnabled(IPlayer player, bool enabled, HanHitmarkerConfig config)
    {
        GetOrCreate(player, config).CenterEffectEnabled = enabled;
    }

    public void SetDamageNumberEnabled(IPlayer player, bool enabled, HanHitmarkerConfig config)
    {
        GetOrCreate(player, config).DamageNumberEnabled = enabled;
    }

    public void EnsureValidSelection(IPlayer player, HanHitmarkerConfig config)
    {
        EnsureValidSelection(GetOrCreate(player, config), config);
    }

    public void RemovePlayerRuntime(int playerId)
    {
        if (!_runtimeKeysByPlayerId.Remove(playerId, out var runtimeKey))
        {
            return;
        }

        if ((runtimeKey & FallbackKeyMask) != 0)
        {
            _states.Remove(runtimeKey);
        }
    }

    public void ClearMapScopedState()
    {
        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player is null || !player.IsValid || player.IsFakeClient)
            {
                continue;
            }

            _runtimeKeysByPlayerId[player.PlayerID] = ResolveRuntimeKey(player);
        }
    }

    public void ClearAllState()
    {
        _runtimeKeysByPlayerId.Clear();
        _states.Clear();
    }

    private static void EnsureValidSelection(PlayerHitmarkerState state, HanHitmarkerConfig config)
    {
        if (!string.IsNullOrWhiteSpace(state.SelectedGroupName) &&
            (config.ParticleGroups ?? []).Any(group =>
                group is not null && group.Enabled &&
                HasValidName(group) &&
                HasValidDigits(group) &&
                group.Name.Trim().Equals(state.SelectedGroupName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        state.SelectedGroupName = ResolveDefaultGroupName(config);
    }

    private static string ResolveDefaultGroupName(HanHitmarkerConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.DefaultParticleGroup) &&
            (config.ParticleGroups ?? []).Any(group =>
                group is not null && group.Enabled &&
                HasValidName(group) &&
                HasValidDigits(group) &&
                group.Name.Trim().Equals(config.DefaultParticleGroup.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return config.DefaultParticleGroup.Trim();
        }

        return (config.ParticleGroups ?? [])
            .FirstOrDefault(group => group is not null && group.Enabled && HasValidName(group) && HasValidDigits(group))
            ?.Name.Trim() ?? string.Empty;
    }

    private static bool HasValidName(HitmarkerParticleGroupConfig group)
    {
        return !string.IsNullOrWhiteSpace(group.Name);
    }

    private static bool HasValidDigits(HitmarkerParticleGroupConfig group)
    {
        return group.DamageDigitParticles is { Length: 10 } &&
               group.DamageDigitParticles.All(path => !string.IsNullOrWhiteSpace(path));
    }

    private static ulong ResolveRuntimeKey(IPlayer player)
    {
        if (player.SteamID != 0)
        {
            return player.SteamID;
        }

        if (player.SessionId != 0)
        {
            return FallbackKeyMask | player.SessionId;
        }

        return FallbackKeyMask | (uint)player.PlayerID;
    }
}
