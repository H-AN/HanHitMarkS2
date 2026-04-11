using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;
using SwiftlyS2.Shared.Sounds;
using Color = SwiftlyS2.Shared.Natives.Color;

namespace HanHitMarkerS2;

public sealed class HanHitMarkerHelpers
{
    private readonly ISwiftlyCore _core;
    private readonly ILogger<HanHitMarkerHelpers> _logger;

    public HanHitMarkerHelpers(ISwiftlyCore core, ILogger<HanHitMarkerHelpers> logger)
    {
        _core = core;
        _logger = logger;
    }

    public CParticleSystem? CreateHitMarkParticle(IPlayer viewer, Vector hitPosition, string particlePath)
    {
        return SpawnParticleAtPosition(viewer, hitPosition, particlePath, lifetimeSeconds: 0.2f);
    }

    public CPointWorldText? CreateHitMarkWorldText(
        IPlayer viewer,
        Vector hitPosition,
        HanHitMarkerWorldTextConfigs worldTextConfig,
        bool headshot)
    {
        if (!TryGetViewerContext(viewer, out var controller, out var pawn))
        {
            return null;
        }

        var worldText = _core.EntitySystem.CreateEntity<CPointWorldText>();
        if (worldText is null)
        {
            return null;
        }

        worldText.MessageText = headshot ? worldTextConfig.WTHitMarkSignHead : worldTextConfig.WTHitMarkSignBody;
        worldText.OwnerEntity = controller.OwnerEntity;
        worldText.OwnerEntityUpdated();
        worldText.Enabled = true;
        worldText.FontSize = headshot ? worldTextConfig.WTHitMarkSizeHead : worldTextConfig.WTHitMarkSizeBody;
        worldText.FontSizeUpdated();
        worldText.Fullbright = true;
        worldText.Color = TryParseColor(worldTextConfig.WTHitMarkFontColor, out var parsedColor)
            ? parsedColor
            : new Color(255, 0, 0, 255);
        worldText.DrawBackground = worldTextConfig.WTHitMarkDrawBackground;
        worldText.BackgroundBorderHeight = 0.0f;
        worldText.BackgroundBorderWidth = 0.2f;
        worldText.BackgroundMaterialName = string.Empty;
        worldText.WorldUnitsPerPx = 1f;
        worldText.FontName = worldTextConfig.WTHitMarkFontName;
        worldText.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER;
        worldText.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER;
        worldText.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
        worldText.DispatchSpawn();

        ConfigureTransmitOnlyToViewer(worldText, viewer);

        var attackerPosition = pawn.AbsOrigin;
        if (attackerPosition is null)
        {
            return null;
        }

        var horizontalDirection = new Vector(
            attackerPosition.Value.X - hitPosition.X,
            attackerPosition.Value.Y - hitPosition.Y,
            0f);

        var yaw = MathF.Atan2(horizontalDirection.Y, horizontalDirection.X) * 180f / MathF.PI + 90f;
        var angles = new QAngle(0f, yaw, 90f);

        worldText.Teleport(hitPosition, angles, null);
        worldText.AcceptInput("Start", 0);
        worldText.AddEntityIOEvent("Kill", string.Empty, null, null, 0.2f);

        return worldText;
    }

    public CParticleSystem? SpawnParticleAtPosition(
        IPlayer viewer,
        Vector position,
        string particleName,
        QAngle? angles = null,
        float lifetimeSeconds = 0.4f)
    {
        if (string.IsNullOrWhiteSpace(particleName))
        {
            return null;
        }

        if (!TryGetViewerContext(viewer, out var controller, out _))
        {
            return null;
        }

        var particle = _core.EntitySystem.CreateEntity<CParticleSystem>();
        if (particle is null)
        {
            return null;
        }

        particle.EffectName = particleName;
        particle.OwnerEntity = controller.OwnerEntity;
        particle.OwnerEntityUpdated();
        particle.DispatchSpawn();

        ConfigureTransmitOnlyToViewer(particle, viewer);

        particle.Teleport(position, angles, new Vector(0, 0, 0));
        particle.AcceptInput("Start", 0);
        particle.AddEntityIOEvent("Kill", string.Empty, null, null, lifetimeSeconds);

        return particle;
    }

    public CPointWorldText? SpawnDamageWorldText(
        IPlayer viewer,
        IPlayer victim,
        string text,
        HanHitMarkerWorldTextConfigs worldTextConfig,
        bool headshot,
        PlayerDamageState damageState)
    {
        if (!TryGetViewerContext(viewer, out var controller, out _))
        {
            return null;
        }

        var transform = CalculateDamageWorldTextTransform(victim, viewer, worldTextConfig);
        if (!transform.HasValue)
        {
            return null;
        }

        var worldText = _core.EntitySystem.CreateEntity<CPointWorldText>();
        if (worldText is null)
        {
            return null;
        }

        worldText.MessageText = text;
        worldText.OwnerEntity = controller.OwnerEntity;
        worldText.OwnerEntityUpdated();
        worldText.Enabled = true;
        worldText.FontSize = headshot ? worldTextConfig.WTHitNumberSizeHead : worldTextConfig.WTHitNumberSizeBody;
        worldText.FontSizeUpdated();
        worldText.Fullbright = true;
        worldText.Color = TryParseColor(worldTextConfig.WTHitNumberFontColor, out var parsedColor)
            ? parsedColor
            : new Color(255, 0, 0, 255);
        worldText.DrawBackground = worldTextConfig.WTHitNumberDrawBackground;
        worldText.BackgroundBorderHeight = 0.0f;
        worldText.BackgroundBorderWidth = 0.2f;
        worldText.BackgroundMaterialName = string.Empty;
        worldText.WorldUnitsPerPx = 1f;
        worldText.FontName = worldTextConfig.WTHitNumberFontName;
        worldText.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
        worldText.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP;
        worldText.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;

        var finalPosition = new Vector(
            transform.Value.Position.X,
            transform.Value.Position.Y,
            transform.Value.Position.Z + damageState.Spacing * damageState.CurrentIndex);

        worldText.Teleport(finalPosition, transform.Value.Angles, null);
        worldText.DispatchSpawn();

        ConfigureTransmitOnlyToViewer(worldText, viewer);
        worldText.AddEntityIOEvent("Kill", string.Empty, null, null, 0.4f);

        damageState.CurrentIndex = (damageState.CurrentIndex + 1) % damageState.MaxCount;
        return worldText;
    }

    public bool IsTeamAllowed(IPlayer player, string teamConfig)
    {
        if (string.IsNullOrWhiteSpace(teamConfig) || teamConfig.Equals("any", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var pawn = player.PlayerPawn;
        if (pawn is null || !pawn.IsValid)
        {
            return false;
        }

        var playerTeam = pawn.TeamNum == 2 ? "t" : "ct";
        return teamConfig.Equals(playerTeam, StringComparison.OrdinalIgnoreCase);
    }

    public void EmitSoundToPlayer(IPlayer player, string soundPath)
    {
        if (player is null || !player.IsValid || string.IsNullOrWhiteSpace(soundPath))
        {
            return;
        }

        var sound = new SoundEvent(soundPath.Trim(), 1.0f, 1.0f);
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
        foreach (var resource in resources)
        {
            if (string.IsNullOrWhiteSpace(resource))
            {
                continue;
            }

            @event.AddItem(resource);
        }
    }

    public void PrecacheSoundEventIfValid(SwiftlyS2.Shared.Events.IOnPrecacheResourceEvent @event, HanHitMarkerConfigs config)
    {
        if (string.IsNullOrWhiteSpace(config.PrecacheSoundEvent))
        {
            return;
        }

        foreach (var soundEvent in config.PrecacheSoundEvent
                     .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            @event.AddItem(soundEvent);
        }
    }

    public bool TryParseColor(string colorValue, out Color color)
    {
        color = new Color(255, 0, 0, 255);

        var parts = colorValue.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || parts.Length > 4)
        {
            return false;
        }

        if (!byte.TryParse(parts[0], out var red) ||
            !byte.TryParse(parts[1], out var green) ||
            !byte.TryParse(parts[2], out var blue))
        {
            return false;
        }

        var alpha = (byte)255;
        if (parts.Length == 4 && !byte.TryParse(parts[3], out alpha))
        {
            return false;
        }

        color = new Color(red, green, blue, alpha);
        return true;
    }

    public List<int> SplitDigits(int number)
    {
        if (number == 0)
        {
            return [0];
        }

        var absoluteValue = Math.Abs(number);
        var digits = new List<int>();
        while (absoluteValue > 0)
        {
            digits.Insert(0, absoluteValue % 10);
            absoluteValue /= 10;
        }

        return digits;
    }

    public Vector CrossProduct(Vector left, Vector right)
    {
        return new Vector(
            left.Y * right.Z - left.Z * right.Y,
            left.Z * right.X - left.X * right.Z,
            left.X * right.Y - left.Y * right.X);
    }

    public Vector Normalized(Vector value)
    {
        var length = MathF.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
        if (length <= 0.0001f)
        {
            return new Vector(0, 0, 0);
        }

        return new Vector(value.X / length, value.Y / length, value.Z / length);
    }

    private void ConfigureTransmitOnlyToViewer(CBaseEntity entity, IPlayer viewer)
    {
        foreach (var player in _core.PlayerManager.GetAllPlayers())
        {
            if (player is null || !player.IsValid)
            {
                continue;
            }

            entity.SetTransmitState(player.PlayerID == viewer.PlayerID, player.PlayerID);
        }
    }

    private bool TryGetViewerContext(
        IPlayer viewer,
        out CCSPlayerController controller,
        out CCSPlayerPawn pawn)
    {
        controller = null!;
        pawn = null!;

        if (viewer is null || !viewer.IsValid)
        {
            return false;
        }

        if (viewer.Controller is not CCSPlayerController playerController || !playerController.IsValid)
        {
            return false;
        }

        if (viewer.PlayerPawn is not CCSPlayerPawn playerPawn || !playerPawn.IsValid)
        {
            return false;
        }

        controller = playerController;
        pawn = playerPawn;
        return true;
    }

    private DamageWorldTextTransform? CalculateDamageWorldTextTransform(
        IPlayer victim,
        IPlayer attacker,
        HanHitMarkerWorldTextConfigs worldTextConfig)
    {
        if (victim.PlayerPawn is not CCSPlayerPawn victimPawn || !victimPawn.IsValid)
        {
            return null;
        }

        if (attacker.PlayerPawn is not CCSPlayerPawn attackerPawn || !attackerPawn.IsValid)
        {
            return null;
        }

        var victimPosition = victimPawn.AbsOrigin;
        var attackerPosition = attackerPawn.AbsOrigin;
        if (victimPosition is null || attackerPosition is null)
        {
            return null;
        }

        var deltaX = attackerPosition.Value.X - victimPosition.Value.X;
        var deltaY = attackerPosition.Value.Y - victimPosition.Value.Y;
        var deltaZ = attackerPosition.Value.Z - victimPosition.Value.Z;

        var distance = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        if (distance < 0.001f)
        {
            distance = 0.001f;
        }

        const float offset = 40f;
        var position = new Vector(
            victimPosition.Value.X + deltaX / distance * offset,
            victimPosition.Value.Y + deltaY / distance * offset,
            victimPosition.Value.Z + deltaZ / distance * offset);

        float yaw;
        float pitch;

        if (worldTextConfig.WTHitNumberPosType == 1)
        {
            position.X += (float)(Random.Shared.NextDouble() * 5d + 10d);
            position.Y += (float)(Random.Shared.NextDouble() * 5d + 10d);
            position.Z += (float)(Random.Shared.NextDouble() * 20d + 80d);

            var viewOffset = attackerPawn.ViewOffset;
            var attackerEye = new Vector(
                attackerPosition.Value.X + viewOffset.X.Value,
                attackerPosition.Value.Y + viewOffset.Y.Value,
                attackerPosition.Value.Z + viewOffset.Z.Value);

            var direction = attackerEye - position;
            yaw = MathF.Atan2(direction.Y, direction.X) * 180f / MathF.PI + 90f;
            pitch = -MathF.Atan2(direction.Z, MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y)) * 180f / MathF.PI;
        }
        else
        {
            position.Z += 80f;

            var horizontalDirection = new Vector(
                attackerPosition.Value.X - position.X,
                attackerPosition.Value.Y - position.Y,
                0f);

            yaw = MathF.Atan2(horizontalDirection.Y, horizontalDirection.X) * 180f / MathF.PI + 90f;
            pitch = 0f;
        }

        return new DamageWorldTextTransform(position, new QAngle(pitch, yaw, 90f));
    }

    private readonly record struct DamageWorldTextTransform(Vector Position, QAngle Angles);
}
