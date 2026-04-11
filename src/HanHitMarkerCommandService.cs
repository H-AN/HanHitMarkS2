using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Players;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerCommandService
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HanHitMarkerCommandService> _logger;
    private readonly IOptionsMonitor<HanHitMarkerConfigs> _config;
    private readonly HanHitMarkerGlobals _globals;
    private readonly List<Guid> _registeredCommands = [];
    private bool _installed;

    public HanHitMarkerCommandService(
        ISwiftlyCore core,
        ILogger<HanHitMarkerCommandService> logger,
        IOptionsMonitor<HanHitMarkerConfigs> config,
        HanHitMarkerGlobals globals)
    {
        _core = core;
        _logger = logger;
        _config = config;
        _globals = globals;
    }

    public void InstallCommands()
    {
        if (_installed)
        {
            return;
        }

        var config = _config.CurrentValue;
        RegisterCommand(config.HitMarkerToggleCommand, HandleHitMarkerToggleCommand);
        RegisterCommand(config.DamageNumberToggleCommand, HandleDamageNumberToggleCommand);
        RegisterCommand(config.ScreenHitEffectToggleCommand, HandleScreenHitEffectToggleCommand);

        _installed = true;
    }

    public void ReloadCommands()
    {
        UninstallCommands();
        InstallCommands();
    }

    public void UninstallCommands()
    {
        foreach (var commandId in _registeredCommands)
        {
            _core.Command.UnregisterCommand(commandId);
        }

        _registeredCommands.Clear();
        _installed = false;
    }

    private void RegisterCommand(
        string? command,
        ICommandService.CommandListener handler)
    {
        var normalizedCommand = NormalizeCommand(command);
        if (string.IsNullOrWhiteSpace(normalizedCommand))
        {
            return;
        }

        if (_core.Command.IsCommandRegistered(normalizedCommand))
        {
            _logger.LogWarning("Skipping command '{Command}' because it is already registered.", normalizedCommand);
            return;
        }

        var commandId = _core.Command.RegisterCommand(
            normalizedCommand,
            handler,
            true,
            string.Empty);

        _registeredCommands.Add(commandId);
    }

    private void HandleHitMarkerToggleCommand(ICommandContext context)
    {
        HandleToggleCommand(context, HanHitMarkerFeature.HitMarker);
    }

    private void HandleDamageNumberToggleCommand(ICommandContext context)
    {
        HandleToggleCommand(context, HanHitMarkerFeature.DamageNumber);
    }

    private void HandleScreenHitEffectToggleCommand(ICommandContext context)
    {
        HandleToggleCommand(context, HanHitMarkerFeature.ScreenHitEffect);
    }

    private void HandleToggleCommand(ICommandContext context, HanHitMarkerFeature feature)
    {
        if (context.Sender is not IPlayer player || !player.IsValid)
        {
            context.Reply(T(context, "HanHitMarker.Command.PlayerOnly"));
            return;
        }

        var config = _config.CurrentValue;
        var featureName = T(player, GetFeatureNameTranslationKey(feature));
        var commandPermission = GetCommandPermission(feature, config);

        if (!HasPermission(player, commandPermission))
        {
            context.Reply(T(player, "HanHitMarker.Command.Feature.CommandPermissionDenied", featureName));
            return;
        }

        if (!_globals.HasFeaturePermission(player, feature, config))
        {
            context.Reply(T(player, "HanHitMarker.Command.Feature.UsePermissionDenied", featureName));
            return;
        }

        if (!HanHitMarkerGlobals.IsFeatureGloballyEnabled(feature, config))
        {
            context.Reply(T(player, "HanHitMarker.Command.Feature.BlockedByGlobal", featureName));
            return;
        }

        var state = _globals.EnsurePlayerFeatureState(player, config);
        var enabled = state.Toggle(feature);
        var messageKey = enabled
            ? "HanHitMarker.Command.Feature.Enabled"
            : "HanHitMarker.Command.Feature.Disabled";

        context.Reply(T(player, messageKey, featureName));
    }

    private static string NormalizeCommand(string? command)
    {
        return string.IsNullOrWhiteSpace(command)
            ? string.Empty
            : command.Trim();
    }

    private static string GetFeatureNameTranslationKey(HanHitMarkerFeature feature)
    {
        return feature switch
        {
            HanHitMarkerFeature.HitMarker => "HanHitMarker.Command.Feature.Name.HitMarker",
            HanHitMarkerFeature.DamageNumber => "HanHitMarker.Command.Feature.Name.DamageNumber",
            HanHitMarkerFeature.ScreenHitEffect => "HanHitMarker.Command.Feature.Name.ScreenHitEffect",
            _ => "HanHitMarker.Command.Feature.Name.Unknown"
        };
    }

    private static string GetCommandPermission(HanHitMarkerFeature feature, HanHitMarkerConfigs config)
    {
        return feature switch
        {
            HanHitMarkerFeature.HitMarker => config.HitMarkerCommandPermission,
            HanHitMarkerFeature.DamageNumber => config.DamageNumberCommandPermission,
            HanHitMarkerFeature.ScreenHitEffect => config.ScreenHitEffectCommandPermission,
            _ => string.Empty
        };
    }

    private string T(ICommandContext context, string key, params object[] args)
    {
        if (context.Sender is IPlayer player && player.IsValid)
        {
            return T(player, key, args);
        }

        return args.Length > 0
            ? _core.Localizer[key, args]
            : _core.Localizer[key];
    }

    private string T(IPlayer? player, string key, params object[] args)
    {
        var localizer = player is not null && player.IsValid
            ? _core.Translation.GetPlayerLocalizer(player)
            : _core.Localizer;

        return args.Length > 0
            ? localizer[key, args]
            : localizer[key];
    }

    private bool HasPermission(IPlayer player, string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return true;
        }

        if (player.SteamID == 0)
        {
            return false;
        }

        return _core.Permission.PlayerHasPermission(player.SteamID, permission.Trim());
    }
}
