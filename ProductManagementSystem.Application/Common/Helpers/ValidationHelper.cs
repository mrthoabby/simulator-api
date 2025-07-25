namespace ProductManagementSystem.Application.Common.Helpers;

public static class ValidationHelper
{
    /// <summary>
    /// 🚀 Ejecuta validaciones en paralelo - Simple y directo
    /// </summary>
    /// <param name="validations">Funciones de validación que se ejecutan en paralelo</param>
    public static async Task TryRunAllParallel(params Func<Task>[] validations)
    {
        if (validations == null || validations.Length == 0)
            return;

        var tasks = validations.Select(Task.Run).ToArray();
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 🎯 Ejecuta validaciones en paralelo y retorna el primer resultado
    /// </summary>
    /// <typeparam name="T">Tipo del resultado esperado</typeparam>
    /// <param name="validationWithResult">Función que retorna un resultado</param>
    /// <param name="otherValidations">Otras validaciones que solo verifican</param>
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
    /// 📦 Ejecuta validaciones en paralelo y retorna múltiples resultados
    /// </summary>
    /// <typeparam name="T">Tipo de los resultados</typeparam>
    /// <param name="validationsWithResults">Funciones que retornan resultados</param>
    public static async Task<T[]> TryRunAllParallelWithResults<T>(params Func<Task<T>>[] validationsWithResults)
    {
        if (validationsWithResults == null || validationsWithResults.Length == 0)
            return Array.Empty<T>();

        var tasks = validationsWithResults.Select(Task.Run).ToArray();
        return await Task.WhenAll(tasks);
    }
}