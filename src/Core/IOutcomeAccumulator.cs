namespace Core;

public interface IOutcomeAccumulator<TResult, TOutcome>
{
    void OnResult(TResult result);
    TOutcome Complete();
}
