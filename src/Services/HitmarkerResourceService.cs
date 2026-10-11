using HanHitmarker.Config;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared.Events;

namespace HanHitmarker.Services;

public sealed class HitmarkerResourceService
{
    private readonly IOptionsMonitor<HanHitmarkerConfig> _config;
    private readonly HitmarkerParticleGroupService _groups;

    public HitmarkerResourceService(
        IOptionsMonitor<HanHitmarkerConfig> config,
        HitmarkerParticleGroupService groups)
    {
        _config = config;
        _groups = groups;
    }

    public void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        var resources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var config = _config.CurrentValue;

        _groups.Refresh(config);

        AddRange(resources, config.PrecacheSoundEvents ?? []);

        var groups = _groups.Groups;
        foreach (var group in groups)
        {
            Add(resources, group.CenterHeadParticle);
            Add(resources, group.CenterBodyParticle);

            foreach (var particle in group.DamageDigitParticles)
            {
                Add(resources, particle);
            }
        }

        foreach (var resource in resources)
        {
            @event.AddItem(resource);
        }

    }

    private static void AddRange(HashSet<string> resources, IEnumerable<string?> values)
    {
        foreach (var value in values)
        {
            Add(resources, value);
        }
    }

    private static void Add(HashSet<string> resources, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            resources.Add(value.Trim());
        }
    }

}
