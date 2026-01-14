namespace Core;

public interface IProcessCoordinator<TOutcome>
{
    Task<TOutcome> RunAsync(CancellationToken ct = default);
}
