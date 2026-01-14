# Workshop 1: Single-pass, global completion

## Exercise 1: All work processed once
<details>
    <summary>💡 Hint for enqueing work</summary>
    
    ```csharp
    foreach (var workItem in workItems)
    {
        await workChannel.Writer.WriteAsync(workItem, ct);
    }
    ```
</details>

<details>
    <summary>💡 Hint for collecting results and detect completion</summary>

    ```csharp
    private async Task CoordinateWork(
        Channel<TWorkResult> resultChannel,
        CancellationToken ct)
    {
        var completedCount = 0;
        await foreach (var result in resultChannel.Reader.ReadAllAsync(ct))
        {
            // accumulate result
            outcomeAccumulator.OnResult(result);

            // track completion
            completedCount++;

            // report progress
            ProgressChanged?.Invoke(this, completedCount);

            // check for completion
            if (completedCount >= workItems.Length)
            {
                break;
            }
        }
    }
    ```
</details>
