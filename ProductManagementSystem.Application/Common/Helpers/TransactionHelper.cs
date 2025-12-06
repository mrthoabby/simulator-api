using Microsoft.Extensions.Logging;

namespace ProductManagementSystem.Application.Common.Helpers;

public static class TransactionHelper
{
    /// <summary>
    /// Sequential transaction with compensation and step tracking
    /// </summary>
    public static async Task<(T1 first, T2 second, string executedStep)> CreateSequentialAsync<T1, T2>(
        Func<Task<T1>> firstFlow,
        Func<T1, Task<T2>> secondFlow,
        Func<T1, string, Task> failedRecovery,
        string firstStepName = "First Step",
        string secondStepName = "Second Step",
        ILogger? logger = null)
    {
        T1? firstEntity = default;
        var currentStep = firstStepName;

        try
        {
            currentStep = firstStepName;
            firstEntity = await firstFlow();
            currentStep = secondStepName;
            var secondEntity = await secondFlow(firstEntity);
            return (firstEntity, secondEntity, secondStepName);
        }
        catch (Exception originalEx)
        {
            if (firstEntity != null)
            {
                try
                {
                    await failedRecovery(firstEntity, currentStep);
                }
                catch (Exception compensationEx)
                {
                    logger?.LogCritical(compensationEx, "CRITICAL: Compensation FAILED for {FailedStep}", currentStep);
                }
            }

            throw new TransactionStepException(currentStep, originalEx);
        }
    }

    /// <summary>
    /// Execute operations with manual compensation (Saga Pattern)
    /// </summary>
    public static async Task ExecuteSagaAsync(
        (Func<Task> operation, Func<Task> compensation)[] operations,
        ILogger? logger = null)
    {
        var completedOperations = new Stack<Func<Task>>();
        var stepNumber = 0;

        try
        {
            foreach (var (operation, compensation) in operations)
            {
                stepNumber++;
                await operation();
                completedOperations.Push(compensation);
            }

        }
        catch (Exception originalEx)
        {
            logger?.LogError(originalEx, "Error in operation #{Step}: {Error}", stepNumber, originalEx.Message);
            logger?.LogWarning("Executing {Count} compensations", completedOperations.Count);

            var compensationStep = 0;
            while (completedOperations.Count > 0)
            {
                compensationStep++;
                try
                {
                    logger?.LogInformation("Executing compensation #{Step}", compensationStep);
                    var compensationAction = completedOperations.Pop();
                    await compensationAction();
                    logger?.LogInformation("Compensation #{Step} completed", compensationStep);
                }
                catch (Exception compensationEx)
                {
                    logger?.LogCritical(compensationEx, "CRITICAL: Compensation #{Step} FAILED", compensationStep);
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Parallel transaction: Create two independent entities simultaneously
    /// </summary>
    public static async Task<(T1 first, T2 second)> CreateParallelAsync<T1, T2>(
        Func<Task<T1>> firstFlow,
        Func<Task<T2>> secondFlow,
        Func<T1, Task> deleteFirst,
        Func<T2, Task> deleteSecond,
        string firstStepName = "First Operation",
        string secondStepName = "Second Operation",
        ILogger? logger = null)
    {
        try
        {
            var (firstEntity, secondEntity) = await ParallelHelper.TryRunTwoParallelWithResults(firstFlow, secondFlow);
            return (firstEntity, secondEntity);
        }
        catch (Exception originalEx)
        {
            await HandleParallelCompensation(firstFlow, secondFlow, deleteFirst, deleteSecond, firstStepName, secondStepName, logger);
            throw new TransactionStepException("Parallel Transaction", originalEx);
        }
    }

    /// <summary>
    /// Parallel transaction without return: Execute two operations with automatic compensation
    /// </summary>
    public static async Task CreateParallelVoidAsync<T1, T2>(
        Func<Task<T1>> firstFlow,
        Func<Task<T2>> secondFlow,
        Func<T1, Task> deleteFirst,
        Func<T2, Task> deleteSecond,
        string firstStepName = "First Operation",
        string secondStepName = "Second Operation",
        ILogger? logger = null)
    {
        try
        {
            var (firstEntity, secondEntity) = await ParallelHelper.TryRunTwoParallelWithResults(firstFlow, secondFlow);
        }
        catch (Exception originalEx)
        {
            await HandleParallelCompensation(firstFlow, secondFlow, deleteFirst, deleteSecond, firstStepName, secondStepName, logger);
            throw new TransactionStepException("Parallel Void Transaction", originalEx);
        }
    }

    /// <summary>
    /// Parallel transaction with list of operations
    /// </summary>
    public static async Task<List<T>> CreateParallelListAsync<T>(
        List<ParallelOperation<T>> operations,
        ILogger? logger = null)
    {
        if (!operations.Any())
        {
            return new List<T>();
        }

        try
        {
            var tasks = operations.Select(op => op.ExecuteAsync()).ToArray();
            await Task.WhenAll(tasks);

            var results = tasks.Select(task => task.Result).ToList();
            return results;
        }
        catch (Exception originalEx)
        {
            await HandleTypedListCompensation(operations, logger);
            throw new TransactionStepException("Parallel List Transaction", originalEx);
        }
    }

    private static async Task HandleParallelCompensation<T1, T2>(
        Func<Task<T1>> createFirst,
        Func<Task<T2>> createSecond,
        Func<T1, Task> deleteFirst,
        Func<T2, Task> deleteSecond,
        string firstStepName,
        string secondStepName,
        ILogger? logger)
    {
        var compensationTasks = new List<Task>();

        try
        {
            var firstEntity = await createFirst();
            compensationTasks.Add(ExecuteCompensationWithErrorHandling(() => deleteFirst(firstEntity), firstStepName, logger));
        }
        catch
        {
            logger?.LogInformation("{FirstStep} does not require compensation (failed)", firstStepName);
        }

        try
        {
            var secondEntity = await createSecond();
            compensationTasks.Add(ExecuteCompensationWithErrorHandling(() => deleteSecond(secondEntity), secondStepName, logger));
        }
        catch
        {
            logger?.LogInformation("{SecondStep} does not require compensation (failed)", secondStepName);
        }

        if (compensationTasks.Any())
        {
            try
            {
                await Task.WhenAll(compensationTasks);
            }
            catch (Exception compensationEx)
            {
                logger?.LogCritical(compensationEx, "CRITICAL: Parallel compensation failure");
            }
        }
    }

    private static async Task HandleTypedListCompensation<T>(List<ParallelOperation<T>> operations, ILogger? logger)
    {
        logger?.LogWarning("Starting compensation for {Count} operations", operations.Count);
        var compensationTasks = new List<Task>();

        foreach (var operation in operations)
        {
            try
            {
                var result = await operation.ExecuteAsync();
                compensationTasks.Add(ExecuteTypedCompensationWithErrorHandling(operation, result, logger));
            }
            catch
            {
                logger?.LogInformation("{OperationName} does not require compensation (failed)", operation.Name);
            }
        }

        if (compensationTasks.Any())
        {
            try
            {
                await Task.WhenAll(compensationTasks);
            }
            catch (Exception compensationEx)
            {
                logger?.LogCritical(compensationEx, "CRITICAL: Compensation failure");
            }
        }
    }

    private static async Task ExecuteCompensationWithErrorHandling(Func<Task> compensation, string operationName, ILogger? logger)
    {
        try
        {
            await compensation();
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error in compensation for {OperationName}: {Error}", operationName, ex.Message);
            throw;
        }
    }

    private static async Task ExecuteTypedCompensationWithErrorHandling<T>(ParallelOperation<T> operation, T result, ILogger? logger)
    {
        try
        {
            await operation.CompensateAsync(result);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error in compensation for {OperationName}: {Error}", operation.Name, ex.Message);
            throw;
        }
    }
}

public class TransactionStepException : Exception
{
    public string FailedStep { get; }
    public Exception OriginalException { get; }

    public TransactionStepException(string failedStep, Exception originalException)
        : base($"Transaction failed at step: {failedStep}. {originalException.Message}", originalException)
    {
        FailedStep = failedStep;
        OriginalException = originalException;
    }
}

public class ParallelOperation<T>
{
    public string Name { get; init; }
    public Func<Task<T>> Execute { get; init; }
    public Func<T, Task> Compensate { get; init; }

    public ParallelOperation(string name, Func<Task<T>> execute, Func<T, Task> compensate)
    {
        Name = name;
        Execute = execute;
        Compensate = compensate;
    }

    public async Task<T> ExecuteAsync() => await Execute();
    public async Task CompensateAsync(T result) => await Compensate(result);
}
