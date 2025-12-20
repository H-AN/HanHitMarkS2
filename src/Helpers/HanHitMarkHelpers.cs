using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitMarkS2;

public class HanHitMarkHelpers
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HanHitMarkHelpers> _logger;
    public HanHitMarkHelpers(ISwiftlyCore core,
        ILogger<HanHitMarkHelpers> logger)
    {
        _core = core;
        _logger = logger;
    }

    public CParticleSystem CreateHitMark(IPlayer player, Vector HitPos, string path)
    {
        if (player == null || !player.IsValid)
            return null;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return null;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return null;

        CParticleSystem hitmarket = _core.EntitySystem.CreateEntity<CParticleSystem>(); 
        if (hitmarket == null)
            return null;

        hitmarket.EffectName = path;

        hitmarket.OwnerEntity = controller.OwnerEntity;
        hitmarket.OwnerEntityUpdated();

        hitmarket.DispatchSpawn();

        foreach (var p in _core.PlayerManager.GetAllPlayers())
        {
            if (!p.IsValid) continue;

            if (p.PlayerID == player.PlayerID)
                hitmarket.SetTransmitState(true, p.PlayerID);
            else
                hitmarket.SetTransmitState(false, p.PlayerID);

        }

        hitmarket.Teleport(HitPos, null, null);

        hitmarket.AcceptInput("Start", 0);
        hitmarket.AddEntityIOEvent("Kill", "", null, null,  0.2f);

        return hitmarket;
    }

    public Vector CrossProduct(Vector a, Vector b)
    {
        return new Vector(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public Vector Normalized(Vector vector)
    {
        float x = vector.X;
        float y = vector.Y;
        float z = vector.Z;

        float length = MathF.Sqrt(x * x + y * y + z * z);
        if (length != 0)
        {
            return new Vector(x / length, y / length, z / length);
        }

        return new Vector(0, 0, 0);
    }


    public List<int> SplitDigits(int number)
    {
        List<int> digits = new List<int>();

        if (number == 0)
        {
            digits.Add(0);
            return digits;
        }

        while (number > 0)
        {
            digits.Insert(0, number % 10); 
            number /= 10;
        }

        return digits;
    }

    public CParticleSystem SpawnParticleAtPosition(IPlayer player, Vector position, string particleName, QAngle? angles = null)
    {
        if (player == null || !player.IsValid)
            return null;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return null;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return null;

        CParticleSystem ShowDamage = _core.EntitySystem.CreateEntity<CParticleSystem>();
        if (ShowDamage == null)
            return null!;

        ShowDamage.EffectName = particleName;
        ShowDamage.OwnerEntity = controller.OwnerEntity;
        ShowDamage.OwnerEntityUpdated();

        ShowDamage.DispatchSpawn();

        foreach (var p in _core.PlayerManager.GetAllPlayers())
        {
            if (!p.IsValid) continue;

            if (p.PlayerID == player.PlayerID)
                ShowDamage.SetTransmitState(true, p.PlayerID);
            else
                ShowDamage.SetTransmitState(false, p.PlayerID);

        }

        ShowDamage.Teleport(position, angles, new Vector(0, 0, 0));

        ShowDamage.AcceptInput("Start", 0);
        ShowDamage.AddEntityIOEvent("Kill", "", null, null, 0.4f);

        return ShowDamage;
    }

    public bool IsTeamAllowed(IPlayer player, string teamConfig)
    {
        if (string.IsNullOrWhiteSpace(teamConfig) || teamConfig.ToLower() == "any")
            return true;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid) 
            return false;

        var team = pawn.TeamNum == 2 ? "t" : "ct";
        return teamConfig.ToLower() == team;
    }

    public void EmitSoundToPlayer(IPlayer player, string path)
    {
        if (player == null || !player.IsValid)
            return;

        if (string.IsNullOrWhiteSpace(path))
            return;

        var sound = new SwiftlyS2.Shared.Sounds.SoundEvent(path, 1.0f, 1.0f);
        sound.SourceEntityIndex = -1;
        sound.Recipients.AddRecipient(player.PlayerID);
        _core.Scheduler.NextTick(() =>
        {
            sound.Emit();
            sound.Recipients.RemoveRecipient(player.PlayerID);
        });
    }

    public void PrecacheIfValid(SwiftlyS2.Shared.Events.IOnPrecacheResourceEvent @event, params string?[] resources)
    {
        if (@event == null || resources == null)
            return;

        foreach (var res in resources)
        {
            if (string.IsNullOrWhiteSpace(res))
                continue;

            @event.AddItem(res);
        }
    }

    public void PrecacheSoundEventIfValid(SwiftlyS2.Shared.Events.IOnPrecacheResourceEvent @event, HanHitMarkConfigs _config)
    {
        if (@event == null)
            return;

        var soundList = _config.PrecacheSoundEvent
                    .Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s));

        foreach (var sound in soundList)
        {
            @event.AddItem(sound);
        }
    }
    
}