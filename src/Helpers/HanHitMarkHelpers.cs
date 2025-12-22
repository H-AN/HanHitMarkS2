using System;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Helpers;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace HanHitMarkS2;

public class HanHitMarkHelpers
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HanHitMarkHelpers> _logger;
    private readonly Dictionary<int, PlayerDamageState> playerStates = new Dictionary<int, PlayerDamageState>();
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
        hitmarket.AddEntityIOEvent("Kill", "", null, null, 0.2f);

        return hitmarket;
    }

    public CPointWorldText CreateHitMarkWroldText(IPlayer player, Vector HitPos, HanHitMarkWorldTextConfigs Wtcfg, bool Headshot)
    {
        if (player == null || !player.IsValid)
            return null;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return null;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return null;

        CPointWorldText hitmarket = _core.EntitySystem.CreateEntity<CPointWorldText>();
        if (hitmarket == null)
            return null;

        hitmarket.MessageText = Headshot ? Wtcfg.WTHitMarkSignHead : Wtcfg.WTHitMarkSignBody;

        hitmarket.OwnerEntity = controller.OwnerEntity;
        hitmarket.OwnerEntityUpdated();

        hitmarket.Enabled = true;

        hitmarket.FontSize = Headshot ? Wtcfg.WTHitMarkSizeHead : Wtcfg.WTHitMarkSizeBody;
        hitmarket.FontSizeUpdated();

        hitmarket.Fullbright = true;
        hitmarket.Color = TryParseColor(Wtcfg.WTHitMarkFontColor, out SwiftlyS2.Shared.Natives.Color parsedColor)
        ? parsedColor
        : new Color(255, 0, 0, 255);

        hitmarket.DrawBackground = Wtcfg.WTHitMarkDrawBackground;

        hitmarket.BackgroundBorderHeight = 0.0f;
        hitmarket.BackgroundBorderWidth = 0.2f;
        hitmarket.BackgroundMaterialName = "";
        hitmarket.WorldUnitsPerPx = 1f;
        hitmarket.FontName = Wtcfg.WTHitMarkFontName;
        hitmarket.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER;
        hitmarket.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
        hitmarket.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;

        hitmarket.DispatchSpawn();

        foreach (var p in _core.PlayerManager.GetAllPlayers())
        {
            if (!p.IsValid) continue;

            if (p.PlayerID == player.PlayerID)
                hitmarket.SetTransmitState(true, p.PlayerID);
            else
                hitmarket.SetTransmitState(false, p.PlayerID);

        }

        var attackerPos = pawn.AbsOrigin;
        if (attackerPos == null)
            return null;

        Vector horizontalDir = new Vector(
            attackerPos.Value.X - HitPos.X,
            attackerPos.Value.Y - HitPos.Y,
            0f
            );

        float yaw = MathF.Atan2(horizontalDir.Y, horizontalDir.X) * 180f / MathF.PI + 90f;
        float pitch = 0f;
        float roll = 90f;

        QAngle finalAngles = new QAngle(pitch, yaw, roll);

        hitmarket.Teleport(HitPos, finalAngles, null);

        hitmarket.AcceptInput("Start", 0);
        hitmarket.AddEntityIOEvent("Kill", "", null, null, 0.2f);

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

    public CPointWorldText SpawnWorldTextAtPosition(IPlayer player, IPlayer victim, string damage, HanHitMarkWorldTextConfigs Wtcfg, bool Headshot, Vector position, QAngle? angles = null)
    {
        if (player == null || !player.IsValid)
            return null;

        var controller = player.Controller;
        if (controller == null || !controller.IsValid)
            return null;

        var pawn = player.PlayerPawn;
        if (pawn == null || !pawn.IsValid)
            return null;

        int playerId = player.PlayerID;
        var state = GetPlayerState(playerId);


        CPointWorldText ShowDamage = _core.EntitySystem.CreateEntity<CPointWorldText>();
        if (ShowDamage == null)
            return null!;

        ShowDamage.MessageText = damage; 
        ShowDamage.OwnerEntity = controller.OwnerEntity;
        ShowDamage.OwnerEntityUpdated();

        ShowDamage.Enabled = true;

        ShowDamage.FontSize = Headshot ? Wtcfg.WTHitNumberSizeHead : Wtcfg.WTHitNumberSizeBody;
        ShowDamage.FontSizeUpdated();

        ShowDamage.Fullbright = true;
        ShowDamage.Color = TryParseColor(Wtcfg.WTHitNumberFontColor, out SwiftlyS2.Shared.Natives.Color parsedColor)
        ? parsedColor
        : new Color(255, 0, 0, 255);

        ShowDamage.DrawBackground = Wtcfg.WTHitNumberDrawBackground;

        ShowDamage.BackgroundBorderHeight = 0.0f;
        ShowDamage.BackgroundBorderWidth = 0.2f;
        ShowDamage.BackgroundMaterialName = "";
        ShowDamage.WorldUnitsPerPx = 1f;
        ShowDamage.FontName = Wtcfg.WTHitNumberFontName; 
        ShowDamage.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
        ShowDamage.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP;
        ShowDamage.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;


        WorldTextContext ctx = CalculateDamageWorldTextTransform(victim, player, Wtcfg);

        var finalZ = ctx.Position.Z + state.Spacing * state.CurrentIndex;

        Vector finalPos = new Vector(ctx.Position.X, ctx.Position.Y, finalZ);

        ShowDamage.Teleport(finalPos, ctx.Angles, null);

        ShowDamage.DispatchSpawn();

        foreach (var p in _core.PlayerManager.GetAllPlayers())
        {
            if (!p.IsValid) continue;

            if (p.PlayerID == player.PlayerID)
                ShowDamage.SetTransmitState(true, p.PlayerID);
            else
                ShowDamage.SetTransmitState(false, p.PlayerID);

        }

        ShowDamage.AddEntityIOEvent("Kill", "", null, null, 0.4f);

        state.CurrentIndex = (state.CurrentIndex + 1) % state.MaxCount;

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

    public bool TryParseColor(string colorStr, out SwiftlyS2.Shared.Natives.Color color)
    {
        color = new SwiftlyS2.Shared.Natives.Color(255, 100, 0, 255); // 默认值
        var parts = colorStr.Split(',');
        if (parts.Length < 3 || parts.Length > 4)
            return false;

        if (byte.TryParse(parts[0].Trim(), out byte r) &&
            byte.TryParse(parts[1].Trim(), out byte g) &&
            byte.TryParse(parts[2].Trim(), out byte b))
        {
            byte a = (parts.Length == 4 && byte.TryParse(parts[3].Trim(), out byte parsedA)) ? parsedA : (byte)255;
            color = new SwiftlyS2.Shared.Natives.Color(r, g, b, a);
            return true;
        }
        return false;
    }

    public WorldTextContext CalculateDamageWorldTextTransform(IPlayer victim, IPlayer attacker, HanHitMarkWorldTextConfigs Wtcfg)
    {
        if (victim == null || !victim.IsValid || attacker == null || !attacker.IsValid)
            return default;

        var victimPawn = victim.PlayerPawn;
        if (victimPawn == null || !victimPawn.IsValid)
            return default;

        var attackerPawn = attacker.PlayerPawn;
        if (attackerPawn == null || !attackerPawn.IsValid)
            return default;

        var victimPos = victimPawn.AbsOrigin;
        if (victimPos == null)
            return default;

        var attackerPos = attackerPawn.AbsOrigin;
        if (attackerPos == null)
            return default;

        

        float deltaX = attackerPos.Value.X - victimPos.Value.X;
        float deltaY = attackerPos.Value.Y - victimPos.Value.Y;
        float deltaZ = attackerPos.Value.Z - victimPos.Value.Z;

        float distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        if (distance < 0.001f) distance = 0.001f; 

        float offset = 40f;
        float newX = victimPos.Value.X + (deltaX / distance) * offset;
        float newY = victimPos.Value.Y + (deltaY / distance) * offset;
        float newZ = victimPos.Value.Z + (deltaZ / distance) * offset;

        if (Wtcfg.WTHitNumberPosType == 1)
        {
            newX += (float)(Random.Shared.NextDouble() * 5 + 10); 
            newY += (float)(Random.Shared.NextDouble() * 5 + 10); 
            newZ += (float)(Random.Shared.NextDouble() * 20 + 80); 
        }
        else
        {
            newZ += 80;
        }

        Vector finalPos = new Vector(newX, newY, newZ);
        var viewOffset = attackerPawn.ViewOffset;          
        Vector attackerEye = new Vector(
            attackerPos.Value.X + viewOffset.X.Value,
            attackerPos.Value.Y + viewOffset.Y.Value,
            attackerPos.Value.Z + viewOffset.Z.Value
        );

        float yaw;
        float pitch;
        float roll;
        if (Wtcfg.WTHitNumberPosType == 1)
        {
            Vector dir = attackerEye - finalPos;

            yaw = MathF.Atan2(dir.Y, dir.X) * 180f / MathF.PI + 90f;
            pitch = -MathF.Atan2(dir.Z, MathF.Sqrt(dir.X * dir.X + dir.Y * dir.Y)) * 180f / MathF.PI;
            roll = 90f; 
        }
        else
        {
            Vector horizontalDir = new Vector(
            attackerPos.Value.X - finalPos.X,
            attackerPos.Value.Y - finalPos.Y,
            0f 
            );

            yaw = MathF.Atan2(horizontalDir.Y, horizontalDir.X) * 180f / MathF.PI + 90f;
            pitch = 0f;
            roll = 90f;   
        }
        

        QAngle finalAngles = new QAngle(pitch, yaw, roll);

        return new WorldTextContext
        {
            Position = finalPos,
            Angles = finalAngles
        };
    }

    public PlayerDamageState GetPlayerState(int playerId)
    {
        if (!playerStates.TryGetValue(playerId, out var state))
        {
            state = new PlayerDamageState();
            playerStates[playerId] = state;
        }
        return state;
    }

    public class PlayerDamageState
    {
        public int MaxCount { get; set; } = 5;     
        public int CurrentIndex { get; set; } = 0;  
        public float Spacing { get; set; } = 10f;   
    }



}