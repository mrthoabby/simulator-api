namespace ProductManagementSystem.Application.Common.Helpers;

public static class ValidationHelper
{
    /// <summary>
    /// Executes validations in parallel
    /// </summary>
    public static async Task TryRunAllParallel(params Func<Task>[] validations)
    {
        if (validations == null || validations.Length == 0)
            return;

        var tasks = validations.Select(Task.Run).ToArray();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Executes validations in parallel and returns the first result
    /// </summary>
    public static async Task<T> TryRunAllParallelWithResult<T>(
        Func<Task<T>> validationWithResult,
        params Func<Task>[] otherValidations)
    {
        var tasks = new List<Task> { Task.Run(validationWithResult) };

        if (otherValidations?.Length > 0)
        {
            tasks.AddRange(otherValidations.Select(Task.Run));
        }

        await Task.WhenAll(tasks);
        return await (Task<T>)tasks[0];
    }

    /// <summary>
    /// Executes validations in parallel and returns multiple results
    /// </summary>
    public static async Task<T[]> TryRunAllParallelWithResults<T>(params Func<Task<T>>[] validationsWithResults)
    {
        if (validationsWithResults == null || validationsWithResults.Length == 0)
            return Array.Empty<T>();

        var tasks = validationsWithResults.Select(Task.Run).ToArray();
        return await Task.WhenAll(tasks);
    }
}
