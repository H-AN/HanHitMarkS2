using HanHitmarker.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;
using SwiftlyS2.Shared.Players;

namespace HanHitmarker.Services;

public sealed class HitmarkerCommandService
{
    private const string FallbackCommand = "sw_hm";

    private readonly ISwiftlyCore _core;
    private readonly ILogger<HitmarkerCommandService> _logger;
    private readonly IOptionsMonitor<HanHitmarkerConfig> _config;
    private readonly HitmarkerMenuService _menu;
    private readonly List<Guid> _registeredCommands = [];
    private readonly HashSet<string> _reportedConflicts = new(StringComparer.OrdinalIgnoreCase);
    private bool _installed;

    public HitmarkerCommandService(
        ISwiftlyCore core,
        ILogger<HitmarkerCommandService> logger,
        IOptionsMonitor<HanHitmarkerConfig> config,
        HitmarkerMenuService menu)
    {
        _core = core;
        _logger = logger;
        _config = config;
        _menu = menu;
    }

    public void InstallCommands()
    {
        if (_installed)
        {
            return;
        }

        RegisterCommand(_config.CurrentValue.MenuCommand);
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

    private void RegisterCommand(string? rawCommand)
    {
        var command = NormalizeCommand(rawCommand);
        if (string.IsNullOrWhiteSpace(command))
        {
            command = FallbackCommand;
        }

        if (_core.Command.IsCommandRegistered(command))
        {
            if (_reportedConflicts.Add(command))
                _logger.LogWarning("Skipping HanHitmarker command '{Command}' because it is already registered.", command);
            return;
        }

        var commandId = _core.Command.RegisterCommand(
            command,
            OpenMenuCommand,
            registerRaw: true,
            permission: string.Empty,
            helpText: "Open HanHitmarker menu");

        _registeredCommands.Add(commandId);
        _reportedConflicts.Remove(command);
    }

    private void OpenMenuCommand(ICommandContext context)
    {
        if (context.Sender is not IPlayer player || !player.IsValid || player.IsFakeClient)
        {
            context.Reply(T(context, "HanHitmarker.Command.PlayerOnly"));
            return;
        }

        _menu.OpenMainMenu(player);
    }

    private static string NormalizeCommand(string? command)
    {
        return string.IsNullOrWhiteSpace(command)
            ? string.Empty
            : command.Trim();
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
}
