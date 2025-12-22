
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;


namespace HanHitMarkS2;


[PluginMetadata(
    Id = "HanHitMarkS2",
    Version = "2.0.0",
    Name = "HanHitMarkS2",
    Author = "H-AN",
    Description = "击中特效与伤害数字 for Sw2/HitMark & Damage number for Sw2")]

public partial class HanHitMarkS2(ISwiftlyCore core) : BasePlugin(core)
{
    private ServiceProvider? ServiceProvider { get; set; }

    private HanHitMarkConfigs _HanHitMarkCFG = null!;
    private HanHitMarkWorldTextConfigs _HanHitMarkWorldTextCFG = null!;
    private HanHitMarkGlobals _Globals = null!;
    private HanHitMarkEvents _Events = null!;


    public override void Load(bool hotReload)
    {
        Core.Configuration.InitializeJsonWithModel<HanHitMarkConfigs>("HanHitMarkCFG.jsonc", "HanHitMarkS2CFG").Configure(builder =>
        {
            builder.AddJsonFile("HanHitMarkCFG.jsonc", false, true);
        });

        Core.Configuration.InitializeJsonWithModel<HanHitMarkWorldTextConfigs>("HanHitMarkWorldTextCFG.jsonc", "HanHitMarkWorldTextS2CFG").Configure(builder =>
        {
            builder.AddJsonFile("HanHitMarkWorldTextCFG.jsonc", false, true);
        });

        var collection = new ServiceCollection();
        collection.AddSwiftly(Core);

        collection
            .AddOptionsWithValidateOnStart<HanHitMarkConfigs>()
            .BindConfiguration("HanHitMarkS2CFG");

        collection
            .AddOptionsWithValidateOnStart<HanHitMarkWorldTextConfigs>()
            .BindConfiguration("HanHitMarkWorldTextS2CFG");

        collection.AddSingleton<HanHitMarkGlobals>();
        collection.AddSingleton<HanHitMarkEvents>();
        collection.AddSingleton<HanHitMarkHelpers>();
        collection.AddSingleton<HanHitMarkService>();

        ServiceProvider = collection.BuildServiceProvider();

        _Globals = ServiceProvider.GetRequiredService<HanHitMarkGlobals>();
        _Events = ServiceProvider.GetRequiredService<HanHitMarkEvents>();

        var monitor = ServiceProvider.GetRequiredService<IOptionsMonitor<HanHitMarkConfigs>>();
        var worldtextmonitor = ServiceProvider.GetRequiredService<IOptionsMonitor<HanHitMarkWorldTextConfigs>>();

        _HanHitMarkCFG = monitor.CurrentValue;
        _HanHitMarkWorldTextCFG = worldtextmonitor.CurrentValue;

        _Globals.LoadDigitParticles(_HanHitMarkCFG);

        monitor.OnChange(newConfig =>
        {
            _HanHitMarkCFG = newConfig;
            _Globals.LoadDigitParticles(_HanHitMarkCFG);
            Core.Logger.LogInformation("[H-AN/HitMark] HanHitMark configuration file has been hot-reloaded!");
        });

        worldtextmonitor.OnChange(newConfig =>
        {
            _HanHitMarkWorldTextCFG = newConfig;
            Core.Logger.LogInformation("[H-AN/HitMark] HanHitMark configuration file has been hot-reloaded!");
        });

        _Events.HookEvents();

    }

    public override void Unload()
    {
        ServiceProvider!.Dispose();
    }

}