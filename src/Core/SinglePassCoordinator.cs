using System.Threading.Channels;

namespace Core;

public sealed class SinglePassCoordinator<TWork, TWorkResult, TOutcome>(
    TWork[] workItems,
    Func<TWork, TWorkResult> workerFunction,
    IOutcomeAccumulator<TWorkResult, TOutcome> outcomeAccumulator,
    int workerCount = 3)
    : IProcessCoordinator<TOutcome>
{
    public event EventHandler<int>? ProgressChanged;

    public async Task<TOutcome> RunAsync(CancellationToken ct = default)
    {
        // 1️⃣ Setup channels
        var workChannel = Channel.CreateUnbounded<TWork>();
        var resultChannel = Channel.CreateUnbounded<TWorkResult>();

        // 2️⃣ Start workers
        var workers = Enumerable.Range(0, workerCount)
            .Select(_ => Task.Run(
                () => WorkerLoop(workChannel.Reader, resultChannel.Writer, ct),
                ct))
            .ToArray();

        // 3️⃣ Fan-out: distribute work
        foreach (var workItem in workItems)
        {
            await workChannel.Writer.WriteAsync(workItem, ct);
        }

        workChannel.Writer.Complete();

        // 4️⃣ Fan-in: collect results
        await CoordinateWork(resultChannel, ct);

        // 5️⃣ Shutdown
        resultChannel.Writer.Complete();
        await Task.WhenAll(workers);

        return outcomeAccumulator.Complete();
    }

    private async Task CoordinateWork(
        Channel<TWorkResult> resultChannel,
        CancellationToken ct)
    {
        // TODO: collect and accumulate results, report progress and track completion
    }

    private async Task WorkerLoop(
        ChannelReader<TWork> workReader,
        ChannelWriter<TWorkResult> resultWriter,
        CancellationToken ct)
    {
        await foreach (var workItem in workReader.ReadAllAsync(ct))
        {
            var result = workerFunction(workItem);
            await resultWriter.WriteAsync(result, ct);
        }
    }
}

