using System.Collections.Concurrent;
using Core;

namespace Tests.Workshop1;

public class Workshop1Tests
{

    private class ResultCollectingOutcomeAccumulator : IOutcomeAccumulator<int, IEnumerable<int>>
    {
        private readonly ConcurrentBag<int> _results = [];

        public void OnResult(int result)
        {
            _results.Add(result);
        }

        public IEnumerable<int> Complete()
        {
            return [.. _results];
        }
    }

    private class SumOutcomeAccumulator : IOutcomeAccumulator<int, int>
    {
        private int _sum = 0;

        public void OnResult(int result)
        {
            _sum += result;
        }

        public int Complete()
        {
            return _sum;
        }
    }

    [Fact]
    public async Task All_items_are_processed_exactly_once()
    {
        // Arrange
        var payloads = Enumerable.Range(0, 10).ToArray();

        var coordinator =
            new SinglePassCoordinator<int, int, IEnumerable<int>>(
                payloads,
                payload => payload,
                new ResultCollectingOutcomeAccumulator());

        // Act
        var outcome = await coordinator.RunAsync();

        // Assert
        Assert.Equal(payloads.Length, outcome.Count());
        Assert.Equal(
            payloads.OrderBy(x => x),
            outcome.OrderBy(x => x));
    }

    [Fact]
    public async Task Results_are_aggregated_correctly()
    {
        // Arrange
        var payloads = Enumerable.Range(1, 100).ToArray();

        var coordinator =
            new SinglePassCoordinator<int, int, int>(
                payloads,
                payload => payload * 2,
                new SumOutcomeAccumulator());

        // Act
        var outcome = await coordinator.RunAsync();

        // Assert
        var expected = payloads.Sum(x => x * 2);
        Assert.Equal(expected, outcome);
    }

    [Fact]
    public async Task Progress_reaches_100_percent()
    {
        // Arrange
        var payloads = Enumerable.Range(0, 20).ToArray();
        var progressUpdates = new List<int>();

        var coordinator =
            new SinglePassCoordinator<int, int, IEnumerable<int>>(
                payloads,
                payload => payload,
                new ResultCollectingOutcomeAccumulator());

        coordinator.ProgressChanged += (_, completedCount) =>
        {
            progressUpdates.Add(completedCount);
        };

        // Act
        await coordinator.RunAsync();

        // Assert
        Assert.NotEmpty(progressUpdates);
        Assert.Equal(payloads.Length, progressUpdates.Count());
        Assert.Equal(payloads.Length, progressUpdates.Last());
    }
}
