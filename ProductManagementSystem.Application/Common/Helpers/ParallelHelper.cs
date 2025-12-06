namespace ProductManagementSystem.Application.Common.Helpers;

public static class ParallelHelper
{
    /// <summary>
    /// Executes multiple operations in parallel and returns all results
    /// </summary>
    public static async Task<T> TryRunAllParallelWithResults<T>(Func<Task<T>> operationWithResult, Func<Task> operationWithoutResult)
    {
        if (operationWithResult == null)
            throw new ArgumentNullException(nameof(operationWithResult));
        if (operationWithoutResult == null)
            throw new ArgumentNullException(nameof(operationWithoutResult));

        var taskWithResult = Task.Run(operationWithResult);
        var taskWithoutResult = Task.Run(operationWithoutResult);
        var tasks = new List<Task> { taskWithResult, taskWithoutResult };

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            if (completedTask.IsFaulted)
            {
                throw completedTask.Exception?.InnerException ??
                      new Exception("A parallel operation failed");
            }
        }

        return await taskWithResult;
    }

    /// <summary>
    /// Executes two operations with results and multiple operations without results in parallel
    /// </summary>
    public static async Task<(T1, T2)> TryRunTwoParallelWithResults<T1, T2>(
        Func<Task<T1>> operationWithResult1,
        Func<Task<T2>> operationWithResult2,
        params Func<Task>[] operationsWithoutResult)
    {
        if (operationWithResult1 == null) throw new ArgumentNullException(nameof(operationWithResult1));
        if (operationWithResult2 == null) throw new ArgumentNullException(nameof(operationWithResult2));

        var task1 = Task.Run(operationWithResult1);
        var task2 = Task.Run(operationWithResult2);
        var tasks = new List<Task> { task1, task2 };

        if (operationsWithoutResult?.Length > 0)
        {
            tasks.AddRange(operationsWithoutResult.Select(Task.Run));
        }

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            if (completedTask.IsFaulted)
            {
                throw completedTask.Exception?.InnerException ??
                      new Exception("A parallel operation failed");
            }
        }

        return (await task1, await task2);
    }

    /// <summary>
    /// Executes a list of operations without results in parallel
    /// </summary>
    public static async Task TryRunParallelOperations(IEnumerable<Func<Task>> operations)
    {
        if (operations == null) throw new ArgumentNullException(nameof(operations));

        var tasks = operations.Select(Task.Run).ToList();

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            if (completedTask.IsFaulted)
            {
                throw completedTask.Exception?.InnerException ??
                      new Exception("A parallel operation failed");
            }
        }
    }

    /// <summary>
    /// Executes exactly two operations in parallel without returning results
    /// </summary>
    public static async Task TryRunTwoParallelOperations(Func<Task> operation1, Func<Task> operation2)
    {
        if (operation1 == null) throw new ArgumentNullException(nameof(operation1));
        if (operation2 == null) throw new ArgumentNullException(nameof(operation2));

        var task1 = Task.Run(operation1);
        var task2 = Task.Run(operation2);
        var tasks = new List<Task> { task1, task2 };

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            if (completedTask.IsFaulted)
            {
                throw completedTask.Exception?.InnerException ??
                      new Exception("A parallel operation failed");
            }
        }
    }

    /// <summary>
    /// Executes a list of operations in parallel and returns all results
    /// </summary>
    public static async Task<List<T>> TryRunParallelOperationsWithResults<T>(IEnumerable<Func<Task<T>>> operations)
    {
        if (operations == null) throw new ArgumentNullException(nameof(operations));

        var tasks = operations.Select(Task.Run).ToList();
        var results = new List<T>();

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            if (completedTask.IsFaulted)
            {
                throw completedTask.Exception?.InnerException ??
                      new Exception("A parallel operation failed");
            }

            results.Add(await completedTask);
        }

        return results;
    }

    public struct ParallelOperationResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }

    /// <summary>
    /// Executes a list of operations in parallel, continuing even if some fail
    /// </summary>
    public static async Task<ParallelOperationResult> TryRunParallelOperationsWithPartialResults(IEnumerable<Func<Task>> operations)
    {
        var failedCount = 0;
        var successCount = 0;
        if (operations == null) throw new ArgumentNullException(nameof(operations));

        var tasks = operations.Select(Task.Run).ToList();

        while (tasks.Any())
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            try
            {
                if (!completedTask.IsFaulted)
                {
                    await completedTask;
                    Interlocked.Increment(ref successCount);
                }
            }
            catch
            {
                Interlocked.Increment(ref failedCount);
            }
        }

        return new ParallelOperationResult
        {
            SuccessCount = successCount,
            FailedCount = failedCount
        };
    }
}
