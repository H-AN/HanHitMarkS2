using HanHitmarker.Config;
using HanHitmarker.Runtime;
using HanHitmarker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;
using System.Text.Json;

namespace HanHitmarker;

[PluginMetadata(
    Id = "HanHitmarker",
    Version = "3.1.0",
    Name = "HanHitmarker",
    Author = "H-AN",
    Description = "Particle-group hitmarker and damage number plugin for SwiftlyS2.")]
public sealed class HanHitmarkerPlugin : BasePlugin
{
    private const string ConfigFileName = "HanHitmarkerCFG.jsonc";
    private const string ConfigSectionName = "HanHitmarkerCFG";

    private ServiceProvider? _serviceProvider;
    private IDisposable? _configSubscription;
    private HitmarkerParticleGroupService? _particleGroups;
    private HitmarkerRuntimeStore? _runtimeStore;
    private HitmarkerCommandService? _commandService;
    private HitmarkerEventService? _eventService;
    private HitmarkerMenuScope? _menus;
    private bool _unloaded;
    private long _configRequest;

    public HanHitmarkerPlugin(ISwiftlyCore core) : base(core)
    {
    }

    private void Lifecycle(Action action)
    {
        // 框架的同步插件生命周期可能从后台调用；安装与清理必须完成后才返回。
        if (Core.IsGameThread) action();
        else Core.Scheduler.NextTickAsync(action).GetAwaiter().GetResult();
    }

    public override void Load(bool hotReload) => Lifecycle(() =>
    {
        _unloaded = false;
        InitializeConfiguration();
        BuildRuntime();

        _particleGroups?.Refresh();
        _commandService?.InstallCommands();
        _eventService?.HookEvents();

        if (_serviceProvider?.GetService<IOptionsMonitor<HanHitmarkerConfig>>() is { } monitor)
        {
            _runtimeStore?.SeedConnectedPlayers(monitor.CurrentValue);
        }

    });

    public override void Unload() => Lifecycle(() =>
    {
        _unloaded = true;
        Interlocked.Increment(ref _configRequest);
        _configSubscription?.Dispose();
        _configSubscription = null;
        _serviceProvider?.GetService<HitmarkerLifetime>()?.Dispose();
        _eventService?.UnhookEvents();
        _commandService?.UninstallCommands();
        _runtimeStore?.ClearAllState();

        _serviceProvider?.Dispose();
        _serviceProvider = null;

        _particleGroups = null;
        _runtimeStore = null;
        _commandService = null;
        _eventService = null;
        _menus = null;
    });

    private void InitializeConfiguration()
    {
        var configPath = Core.Configuration.GetConfigPath(ConfigFileName);
        if (!File.Exists(configPath))
        {
            // 默认模板单独创建，绑定模型保持空集合，避免 .NET 配置绑定追加默认项。
            var template = new Dictionary<string, HanHitmarkerConfig>
            {
                [ConfigSectionName] = HanHitmarkerConfig.CreateDefault()
            };
            File.WriteAllText(configPath, JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true }));
        }
        Core.Configuration.Configure(builder =>
        {
            builder.AddJsonFile(ConfigFileName, optional: false, reloadOnChange: true);
        });
    }

    private void BuildRuntime()
    {
        _serviceProvider?.Dispose();

        var services = new ServiceCollection();
        services.AddSwiftly(Core);
        services
            .AddOptionsWithValidateOnStart<HanHitmarkerConfig>()
            .BindConfiguration(ConfigSectionName);

        services.AddSingleton<HitmarkerRuntimeStore>();
        services.AddSingleton<HitmarkerLifetime>();
        services.AddSingleton<HitmarkerMenuScope>();
        services.AddSingleton<HitmarkerParticleGroupService>();
        services.AddSingleton<HitmarkerPermissionService>();
        services.AddSingleton<HitmarkerSoundService>();
        services.AddSingleton<HitmarkerResourceService>();
        services.AddSingleton<CenterParticleService>();
        services.AddSingleton<DamageNumberService>();
        services.AddSingleton<HitmarkerMenuService>();
        services.AddSingleton<HitmarkerCommandService>();
        services.AddSingleton<HitmarkerEventService>();

        _serviceProvider = services.BuildServiceProvider();

        _particleGroups = _serviceProvider.GetRequiredService<HitmarkerParticleGroupService>();
        _runtimeStore = _serviceProvider.GetRequiredService<HitmarkerRuntimeStore>();
        _commandService = _serviceProvider.GetRequiredService<HitmarkerCommandService>();
        _eventService = _serviceProvider.GetRequiredService<HitmarkerEventService>();
        _menus = _serviceProvider.GetRequiredService<HitmarkerMenuScope>();

        var configMonitor = _serviceProvider.GetRequiredService<IOptionsMonitor<HanHitmarkerConfig>>();
        _configSubscription?.Dispose();
        _configSubscription = configMonitor.OnChange(OnConfigChanged);
    }

    private void OnConfigChanged(HanHitmarkerConfig config)
    {
        var request = Interlocked.Increment(ref _configRequest);
        Core.Scheduler.NextWorldUpdate(() =>
        {
            if (_unloaded || request != Volatile.Read(ref _configRequest)) return;
            _menus?.Invalidate();
            _particleGroups?.Refresh(config);
            _commandService?.ReloadCommands();
            _runtimeStore?.SeedConnectedPlayers(config);
        });
    }
}
