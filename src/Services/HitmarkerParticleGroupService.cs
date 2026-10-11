using HanHitmarker.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HanHitmarker.Services;

public sealed class HitmarkerParticleGroupService
{
    private readonly ILogger<HitmarkerParticleGroupService> _logger;
    private readonly IOptionsMonitor<HanHitmarkerConfig> _config;
    private readonly object _sync = new();
    private Dictionary<string, HitmarkerParticleGroupConfig> _groupsByName = new(StringComparer.OrdinalIgnoreCase);
    private List<HitmarkerParticleGroupConfig> _groups = [];
    private readonly HashSet<string> _reportedDiagnostics = [];

    public HitmarkerParticleGroupService(
        ILogger<HitmarkerParticleGroupService> logger,
        IOptionsMonitor<HanHitmarkerConfig> config)
    {
        _logger = logger;
        _config = config;
    }

    public IReadOnlyList<HitmarkerParticleGroupConfig> Groups
    {
        get
        {
            lock (_sync)
            {
                return _groups.ToArray();
            }
        }
    }

    public void Refresh()
    {
        Refresh(_config.CurrentValue);
    }

    public void Refresh(HanHitmarkerConfig config)
    {
        var groups = new List<HitmarkerParticleGroupConfig>();
        var byName = new Dictionary<string, HitmarkerParticleGroupConfig>(StringComparer.OrdinalIgnoreCase);
        var diagnostics = new HashSet<string>();

        foreach (var group in config.ParticleGroups ?? [])
        {
            if (group is null || !group.Enabled)
            {
                continue;
            }

            var name = group.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                diagnostics.Add("Skipped a particle group because Name is empty.");
                continue;
            }

            var digitParticles = NormalizeDigitParticles(name, group.DamageDigitParticles, diagnostics);
            if (digitParticles is null)
            {
                continue;
            }

            var normalizedGroup = NormalizeGroup(group, name, digitParticles);
            if (string.IsNullOrWhiteSpace(normalizedGroup.CenterHeadParticle) ||
                string.IsNullOrWhiteSpace(normalizedGroup.CenterBodyParticle))
                diagnostics.Add($"Particle group '{name}' has an empty center particle path; the corresponding center effect is unavailable.");
            if (byName.ContainsKey(name))
            {
                var existingIndex = groups.FindIndex(existing =>
                    existing.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (existingIndex >= 0)
                {
                    groups[existingIndex] = normalizedGroup;
                }

                byName[name] = normalizedGroup;
                diagnostics.Add($"Duplicate particle group '{name}' found. The later configured group wins.");
                continue;
            }

            groups.Add(normalizedGroup);
            byName[name] = normalizedGroup;
        }

        lock (_sync)
        {
            _groups = groups;
            _groupsByName = byName;
        }

        if (groups.Count == 0)
        {
            if (config.Enabled) diagnostics.Add("No enabled valid particle groups are configured.");
        }

        if (config.Enabled && !string.IsNullOrWhiteSpace(config.DefaultParticleGroup) &&
            !byName.ContainsKey(config.DefaultParticleGroup.Trim()))
            diagnostics.Add($"DefaultParticleGroup '{config.DefaultParticleGroup}' is missing or invalid; the first valid group is used.");

        _reportedDiagnostics.IntersectWith(diagnostics);
        foreach (var diagnostic in diagnostics)
            if (_reportedDiagnostics.Add(diagnostic)) _logger.LogWarning("HanHitmarker: {Diagnostic}", diagnostic);
    }

    public HitmarkerParticleGroupConfig? GetByName(string? groupName)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            return null;
        }

        lock (_sync)
        {
            return _groupsByName.TryGetValue(groupName.Trim(), out var group) ? group : null;
        }
    }

    public HitmarkerParticleGroupConfig? GetFirstGroup()
    {
        lock (_sync)
        {
            return _groups.FirstOrDefault();
        }
    }

    private static string[]? NormalizeDigitParticles(string groupName, string[]? particles, HashSet<string> diagnostics)
    {
        var values = (particles ?? [])
            .Select(particle => particle?.Trim() ?? string.Empty)
            .ToArray();

        if (values.Length != 10 || values.Any(string.IsNullOrWhiteSpace))
        {
            diagnostics.Add($"Skipped particle group '{groupName}' because DamageDigitParticles must contain exactly 10 non-empty entries for digits 0-9. Count={values.Length}");
            return null;
        }

        return values;
    }

    private static HitmarkerParticleGroupConfig NormalizeGroup(
        HitmarkerParticleGroupConfig group,
        string name,
        string[] digitParticles)
    {
        return new HitmarkerParticleGroupConfig
        {
            Enabled = group.Enabled,
            Name = name,
            AdminFlag = group.AdminFlag?.Trim() ?? string.Empty,
            CenterHeadParticle = group.CenterHeadParticle?.Trim() ?? string.Empty,
            CenterBodyParticle = group.CenterBodyParticle?.Trim() ?? string.Empty,
            DamageDigitParticles = digitParticles,
            HitHeadSound = group.HitHeadSound?.Trim() ?? string.Empty,
            HitBodySound = group.HitBodySound?.Trim() ?? string.Empty,
            DamageNumberSound = group.DamageNumberSound?.Trim() ?? string.Empty
        };
    }
}
