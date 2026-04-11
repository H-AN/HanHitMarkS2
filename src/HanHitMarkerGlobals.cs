using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerGlobals
{
    private readonly ISwiftlyCore _core;

    public HanHitMarkerGlobals(ISwiftlyCore core)
    {
        _core = core;
    }

    public Dictionary<int, DamageHitInfo> LastHitInfo { get; } = [];
    public Dictionary<int, string> DigitParticles { get; } = [];
    public Dictionary<int, ImpactInfo> LastImpactByPlayerId { get; } = [];
    public HashSet<int> ImpactLockedPlayerIds { get; } = [];
    public Dictionary<int, ulong> PlayerRuntimeKeysByPlayerId { get; } = [];
    public Dictionary<ulong, PlayerFeatureState> PlayerFeatureStates { get; } = [];
    public Dictionary<ulong, PlayerDamageState> DamageStates { get; } = [];

    public void LoadDigitParticles(HanHitMarkerConfigs config)
    {
        DigitParticles.Clear();
        DigitParticles[0] = config.DamageNumberParticles0;
        DigitParticles[1] = config.DamageNumberParticles1;
        DigitParticles[2] = config.DamageNumberParticles2;
        DigitParticles[3] = config.DamageNumberParticles3;
        DigitParticles[4] = config.DamageNumberParticles4;
        DigitParticles[5] = config.DamageNumberParticles5;
        DigitParticles[6] = config.DamageNumberParticles6;
        DigitParticles[7] = config.DamageNumberParticles7;
        DigitParticles[8] = config.DamageNumberParticles8;
        DigitParticles[9] = config.DamageNumberParticles9;
    }

    public PlayerFeatureState EnsurePlayerFeatureState(IPlayer player, HanHitMarkerConfigs config)
    {
        var runtimeKey = ResolveRuntimeKey(player);
        PlayerRuntimeKeysByPlayerId[player.PlayerID] = runtimeKey;

        if (PlayerFeatureStates.TryGetValue(runtimeKey, out var state))
        {
            return state;
        }

        state = new PlayerFeatureState
        {
            HitMarkerEnabled = config.PlayerDefaultHitMarkerEnabled,
            DamageNumberEnabled = config.PlayerDefaultDamageNumberEnabled,
            ScreenHitEffectEnabled = config.PlayerDefaultScreenHitEffectEnabled
        };

        PlayerFeatureStates[runtimeKey] = state;
        return state;
    }

    public PlayerDamageState GetOrCreateDamageState(IPlayer player)
    {
        var runtimeKey = ResolveRuntimeKey(player);
        PlayerRuntimeKeysByPlayerId[player.PlayerID] = runtimeKey;

        if (DamageStates.TryGetValue(runtimeKey, out var state))
        {
            return state;
        }

        state = new PlayerDamageState();
        DamageStates[runtimeKey] = state;
        return state;
    }

    public bool IsFeatureEnabledForPlayer(IPlayer player, HanHitMarkerFeature feature, HanHitMarkerConfigs config)
    {
        if (!IsFeatureGloballyEnabled(feature, config))
        {
            return false;
        }

        if (!HasFeaturePermission(player, feature, config))
        {
            return false;
        }

        var state = EnsurePlayerFeatureState(player, config);
        return state.Get(feature);
    }

    public bool HasFeaturePermission(IPlayer player, HanHitMarkerFeature feature, HanHitMarkerConfigs config)
    {
        return HasPermission(player, GetFeaturePermission(feature, config));
    }

    public static string GetFeaturePermission(HanHitMarkerFeature feature, HanHitMarkerConfigs config)
    {
        return feature switch
        {
            HanHitMarkerFeature.HitMarker => config.HitMarkerFeaturePermission,
            HanHitMarkerFeature.DamageNumber => config.DamageNumberFeaturePermission,
            HanHitMarkerFeature.ScreenHitEffect => config.ScreenHitEffectFeaturePermission,
            _ => string.Empty
        };
    }

    public static bool IsFeatureGloballyEnabled(HanHitMarkerFeature feature, HanHitMarkerConfigs config)
    {
        return feature switch
        {
            HanHitMarkerFeature.HitMarker => config.EnabledHitMark,
            HanHitMarkerFeature.DamageNumber => config.EnabledDamageNumber,
            HanHitMarkerFeature.ScreenHitEffect => config.EnabledScreenHitEffect,
            _ => false
        };
    }

    public void RemovePlayerRuntime(int playerId)
    {
        LastHitInfo.Remove(playerId);
        LastImpactByPlayerId.Remove(playerId);
        ImpactLockedPlayerIds.Remove(playerId);

        if (!PlayerRuntimeKeysByPlayerId.Remove(playerId, out var runtimeKey))
        {
            return;
        }

        PlayerFeatureStates.Remove(runtimeKey);
        DamageStates.Remove(runtimeKey);
    }

    public void ClearMapScopedState()
    {
        LastHitInfo.Clear();
        LastImpactByPlayerId.Clear();
        ImpactLockedPlayerIds.Clear();
        DamageStates.Clear();
    }

    public void ClearAllState()
    {
        ClearMapScopedState();
        PlayerRuntimeKeysByPlayerId.Clear();
        PlayerFeatureStates.Clear();
    }

    private static ulong ResolveRuntimeKey(IPlayer player)
    {
        if (player.SessionId != 0)
        {
            return player.SessionId;
        }

        return unchecked((1UL << 63) | (uint)player.PlayerID);
    }

    private bool HasPermission(IPlayer player, string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return true;
        }

        var steamId = player.SteamID;
        if (steamId == 0)
        {
            return false;
        }

        return _core.Permission.PlayerHasPermission(steamId, permission.Trim());
    }
}

public sealed class DamageHitInfo
{
    public int Damage { get; init; }
    public bool Headshot { get; init; }
}

public readonly record struct ImpactInfo(Vector Position, float Time);

public sealed class PlayerFeatureState
{
    public bool HitMarkerEnabled { get; set; }
    public bool DamageNumberEnabled { get; set; }
    public bool ScreenHitEffectEnabled { get; set; }

    public bool Get(HanHitMarkerFeature feature)
    {
        return feature switch
        {
            HanHitMarkerFeature.HitMarker => HitMarkerEnabled,
            HanHitMarkerFeature.DamageNumber => DamageNumberEnabled,
            HanHitMarkerFeature.ScreenHitEffect => ScreenHitEffectEnabled,
            _ => false
        };
    }

    public bool Toggle(HanHitMarkerFeature feature)
    {
        var nextValue = !Get(feature);

        switch (feature)
        {
            case HanHitMarkerFeature.HitMarker:
                HitMarkerEnabled = nextValue;
                break;
            case HanHitMarkerFeature.DamageNumber:
                DamageNumberEnabled = nextValue;
                break;
            case HanHitMarkerFeature.ScreenHitEffect:
                ScreenHitEffectEnabled = nextValue;
                break;
        }

        return nextValue;
    }
}

public sealed class PlayerDamageState
{
    public int MaxCount { get; set; } = 5;
    public int CurrentIndex { get; set; }
    public float Spacing { get; set; } = 10f;
}
