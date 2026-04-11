using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;

namespace HanHitMarkerS2;

[PluginMetadata(
    Id = "HanHitMarkerS2",
    Version = "3.0.0",
    Name = "HanHitMarkerS2",
    Author = "H-AN",
    Description = "Hitmarker, damage number, and attacker-only screen hit particles for SwiftlyS2.")]
public sealed class HanHitMarkerS2 : BasePlugin
{
    private const string MainConfigFileName = "HanHitMarkerCFG.jsonc";
    private const string MainConfigSectionName = "HanHitMarkerS2CFG";
    private const string WorldTextConfigFileName = "HanHitMarkerWorldTextCFG.jsonc";
    private const string WorldTextConfigSectionName = "HanHitMarkerWorldTextS2CFG";

    private ServiceProvider? _serviceProvider;
    private IDisposable? _mainConfigSubscription;
    private IDisposable? _worldTextConfigSubscription;
    private HanHitMarkerGlobals? _globals;
    private HanHitMarkerEvents? _events;
    private HanHitMarkerCommandService? _commandService;

    public HanHitMarkerS2(ISwiftlyCore core) : base(core)
    {
    }

    public override void Load(bool hotReload)
    {
        InitializeConfiguration();
        BuildRuntime();

        _commandService?.InstallCommands();
        _events?.HookEvents();

        Core.Logger.LogInformation("HanHitMarkerS2 loaded. HotReload={HotReload}", hotReload);
    }

    public override void Unload()
    {
        _events?.UnhookEvents();
        _commandService?.UninstallCommands();
        _globals?.ClearAllState();

        _mainConfigSubscription?.Dispose();
        _mainConfigSubscription = null;

        _worldTextConfigSubscription?.Dispose();
        _worldTextConfigSubscription = null;

        _serviceProvider?.Dispose();
        _serviceProvider = null;

        _globals = null;
        _events = null;
        _commandService = null;
    }

    private void InitializeConfiguration()
    {
        Core.Configuration.InitializeJsonWithModel<HanHitMarkerConfigs>(MainConfigFileName, MainConfigSectionName).Configure(builder =>
        {
            builder.AddJsonFile(MainConfigFileName, optional: false, reloadOnChange: true);
        });

        Core.Configuration.InitializeJsonWithModel<HanHitMarkerWorldTextConfigs>(WorldTextConfigFileName, WorldTextConfigSectionName).Configure(builder =>
        {
            builder.AddJsonFile(WorldTextConfigFileName, optional: false, reloadOnChange: true);
        });
    }

    private void BuildRuntime()
    {
        _serviceProvider?.Dispose();

        var services = new ServiceCollection();
        services.AddSwiftly(Core);
        services
            .AddOptionsWithValidateOnStart<HanHitMarkerConfigs>()
            .BindConfiguration(MainConfigSectionName);

        services
            .AddOptionsWithValidateOnStart<HanHitMarkerWorldTextConfigs>()
            .BindConfiguration(WorldTextConfigSectionName);

        services.AddSingleton<HanHitMarkerGlobals>();
        services.AddSingleton<HanHitMarkerHelpers>();
        services.AddSingleton<HanHitMarkerVictimDisplayService>();
        services.AddSingleton<HanHitMarkerScreenEffectService>();
        services.AddSingleton<HanHitMarkerCommandService>();
        services.AddSingleton<HanHitMarkerEvents>();

        _serviceProvider = services.BuildServiceProvider();

        _globals = _serviceProvider.GetRequiredService<HanHitMarkerGlobals>();
        _events = _serviceProvider.GetRequiredService<HanHitMarkerEvents>();
        _commandService = _serviceProvider.GetRequiredService<HanHitMarkerCommandService>();

        var mainConfigMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<HanHitMarkerConfigs>>();
        var worldTextConfigMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<HanHitMarkerWorldTextConfigs>>();

        _globals.LoadDigitParticles(mainConfigMonitor.CurrentValue);
        SeedConnectedPlayerStates(mainConfigMonitor.CurrentValue);

        _mainConfigSubscription?.Dispose();
        _mainConfigSubscription = mainConfigMonitor.OnChange(OnMainConfigChanged);

        _worldTextConfigSubscription?.Dispose();
        _worldTextConfigSubscription = worldTextConfigMonitor.OnChange(_ =>
        {
            Core.Logger.LogInformation("HanHitMarkerS2 worldtext config hot-reloaded.");
        });
    }

    private void OnMainConfigChanged(HanHitMarkerConfigs newConfig)
    {
        if (_globals is null)
        {
            return;
        }

        _globals.LoadDigitParticles(newConfig);
        SeedConnectedPlayerStates(newConfig);
        _commandService?.ReloadCommands();

        Core.Logger.LogInformation("HanHitMarkerS2 main config hot-reloaded.");
    }

    private void SeedConnectedPlayerStates(HanHitMarkerConfigs config)
    {
        if (_globals is null)
        {
            return;
        }

        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player is null || !player.IsValid)
            {
                continue;
            }

            _globals.EnsurePlayerFeatureState(player, config);
        }
    }
}
