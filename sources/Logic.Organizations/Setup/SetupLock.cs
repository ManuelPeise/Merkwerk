namespace Logic.Organizations.Setup;

/// <summary>
/// Serializes setup calls (singleton). Merkwerk runs as one process (ADR 002), so an in-process lock is enough to keep
/// two parallel requests from creating two owners.
/// </summary>
internal sealed class SetupLock : IDisposable
{
    public SemaphoreSlim Semaphore { get; } = new(1, 1);

    public void Dispose() => Semaphore.Dispose();
}
