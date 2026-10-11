using HanHitmarker.Config;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

namespace HanHitmarker.Services;

public sealed class HitmarkerPermissionService
{
    private readonly ISwiftlyCore _core;
    private readonly HitmarkerParticleGroupService _groups;

    public HitmarkerPermissionService(ISwiftlyCore core, HitmarkerParticleGroupService groups)
    {
        _core = core;
        _groups = groups;
    }

    public bool CanUseGroup(IPlayer player, HitmarkerParticleGroupConfig group)
    {
        if (player is null || !player.IsValid)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(group.AdminFlag))
        {
            return true;
        }

        if (player.SteamID == 0)
        {
            return false;
        }

        return _core.Permission.PlayerHasPermission(player.SteamID, group.AdminFlag.Trim());
    }

    public IReadOnlyList<HitmarkerParticleGroupConfig> GetVisibleGroups(IPlayer player)
    {
        var visible = new List<HitmarkerParticleGroupConfig>();

        foreach (var group in _groups.Groups)
        {
            if (CanUseGroup(player, group))
            {
                visible.Add(group);
            }
        }

        return visible;
    }

    public HitmarkerParticleGroupConfig? GetFirstVisibleGroup(IPlayer player)
    {
        foreach (var group in _groups.Groups)
        {
            if (CanUseGroup(player, group))
            {
                return group;
            }
        }

        return null;
    }
}
