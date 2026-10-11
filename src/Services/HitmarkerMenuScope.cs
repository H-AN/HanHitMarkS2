using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace HanHitmarker.Services;

// 菜单所有权与配置代次由本体维护；UI 借用原菜单而不接管业务状态。
public sealed class HitmarkerMenuScope : IDisposable
{
    public readonly record struct Ticket(int PlayerId, ulong Session, ulong SteamId, long Epoch);
    private readonly ISwiftlyCore _core;
    private readonly Dictionary<IMenuAPI, Ticket> _menus = new(ReferenceEqualityComparer.Instance);
    private CancellationTokenSource? _timer;
    private long _epoch;
    private bool _disposed;

    public HitmarkerMenuScope(ISwiftlyCore core)
    {
        _core = core;
        _core.Event.OnMapLoad += OnMapLoad;
        _core.Event.OnMapUnload += OnMapUnload;
        _core.Event.OnClientDisconnected += OnDisconnected;
        StartTimer();
    }

    public Ticket Capture(IPlayer player) => new(player.PlayerID, player.SessionId, player.SteamID, _epoch);

    public IPlayer? Resolve(Ticket ticket)
    {
        if (_disposed || ticket.Epoch != _epoch) return null;
        var player = _core.PlayerManager.GetPlayerFromSessionId(ticket.Session);
        return player is { IsValid: true, IsFakeClient: false }
            && player.PlayerID == ticket.PlayerId && player.SteamID == ticket.SteamId ? player : null;
    }

    public IPlayer? ResolveCurrent(Ticket ticket, IMenuAPI menu)
    {
        var player = Resolve(ticket);
        return player is not null && _menus.TryGetValue(menu, out var owner) && owner == ticket
            && ReferenceEquals(_core.MenusAPI.GetCurrentMenu(player), menu) ? player : null;
    }

    public void Open(IPlayer player, IMenuAPI menu)
    {
        _menus.Add(menu, Capture(player));
        _core.MenusAPI.OpenMenuForPlayer(player, menu);
    }

    public void Invalidate()
    {
        ++_epoch;
        foreach (var pair in _menus.ToArray()) Release(pair.Key, pair.Value);
    }

    private void StartTimer()
    {
        _timer?.Cancel();
        _timer?.Dispose();
        _timer = _core.Scheduler.RepeatBySeconds(0.5f, Sweep);
    }

    private void Sweep()
    {
        foreach (var pair in _menus.ToArray())
        {
            var player = Resolve(pair.Value);
            if (player is not null && ContainsMenu(_core.MenusAPI.GetCurrentMenu(player), pair.Key)) continue;
            if (player is not null && pair.Key.Options.Any(option => !option.IsClickTaskCompleted(player))) continue;
            Release(pair.Key, pair.Value);
        }
    }

    private static bool ContainsMenu(IMenuAPI? active, IMenuAPI ancestor)
    {
        var visited = new HashSet<IMenuAPI>(ReferenceEqualityComparer.Instance);
        for (var menu = active; menu is not null && visited.Add(menu); menu = menu.Parent.ParentMenu)
            if (ReferenceEquals(menu, ancestor)) return true;
        return false;
    }

    private void Release(IMenuAPI menu, Ticket owner)
    {
        _menus.Remove(menu);
        var player = _core.PlayerManager.GetPlayerFromSessionId(owner.Session);
        if (player is { IsValid: true } && player.PlayerID == owner.PlayerId && player.SteamID == owner.SteamId)
        {
            // 子菜单仍依赖本体的父菜单；失效时沿当前父链关闭，避免返回已释放的菜单。
            var visited = new HashSet<IMenuAPI>(ReferenceEqualityComparer.Instance);
            while (_core.MenusAPI.GetCurrentMenu(player) is { } active && ContainsMenu(active, menu) && visited.Add(active))
                _core.MenusAPI.CloseMenuForPlayer(player, active);
        }
        foreach (var option in menu.Options.ToArray()) option.Dispose();
        menu.Dispose();
    }

    private void OnMapLoad(IOnMapLoadEvent _) { if (!_disposed) StartTimer(); }
    private void OnMapUnload(IOnMapUnloadEvent _)
    {
        Invalidate();
        _timer?.Cancel();
        _timer?.Dispose();
        _timer = null;
    }
    private void OnDisconnected(IOnClientDisconnectedEvent @event)
    {
        foreach (var pair in _menus.Where(pair => pair.Value.PlayerId == @event.PlayerId).ToArray())
            Release(pair.Key, pair.Value);
    }
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Invalidate();
        _timer?.Cancel();
        _timer?.Dispose();
        _timer = null;
        _core.Event.OnMapLoad -= OnMapLoad;
        _core.Event.OnMapUnload -= OnMapUnload;
        _core.Event.OnClientDisconnected -= OnDisconnected;
    }
}
