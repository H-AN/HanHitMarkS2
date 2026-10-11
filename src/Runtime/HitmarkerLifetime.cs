namespace HanHitmarker.Runtime;

// 延迟工作只在其创建时所属的插件与地图代次仍有效时执行。
public sealed class HitmarkerLifetime : IDisposable
{
    private long _epoch;
    private bool _disposed;
    public long Capture() => _epoch;
    public bool IsCurrent(long epoch) => !_disposed && epoch == _epoch;
    public void Invalidate() => ++_epoch;
    public void Dispose() { _disposed = true; Invalidate(); }
}
