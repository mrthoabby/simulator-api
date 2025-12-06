using Microsoft.Extensions.Logging;
using ProductManagementSystem.Application.Common.AppEntities.Errors;

namespace ProductManagementSystem.Application.Common.Helpers;

public static class RetryHelper
{
    /// <summary>
    /// Creates a builder for configuring and executing an operation with retries
    /// </summary>
    public static RetryBuilder Create(Func<Task> operation)
    {
        return new RetryBuilder(operation);
    }

    /// <summary>
    /// Executes an operation with automatic retries and exponential backoff
    /// </summary>
    public static async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        ILogger? logger = null,
        Type[]? retryableExceptions = null)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= maxRetries)
        {
            try
            {
                if (attempt > 0)
                {
                    logger?.LogInformation("Retry #{Attempt} of {MaxRetries}", attempt, maxRetries);
                }

                await operation();

                if (attempt > 0)
                {
                    logger?.LogInformation("Operation successful on attempt #{Attempt}", attempt + 1);
                }

                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (retryableExceptions != null && !retryableExceptions.Contains(ex.GetType()))
                {
                    logger?.LogWarning("Non-retryable exception: {Exception}", ex.GetType().Name);
                    throw;
                }

                if (attempt > maxRetries)
                {
                    logger?.LogError(ex, "Operation failed after {MaxRetries} retries: {Error}", maxRetries, ex.Message);
                    throw;
                }

                var delayMs = (int)(baseDelayMs * Math.Pow(2, attempt - 1));

                logger?.LogWarning(ex, "Attempt #{Attempt} failed: {Error}. Retrying in {Delay}ms...",
                    attempt, ex.Message, delayMs);

                await Task.Delay(delayMs);
            }
        }

        throw lastException ?? new ApplicationError("Operation failed without registered exception");
    }

    /// <summary>
    /// Executes an operation with result and automatic retries
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        ILogger? logger = null,
        Type[]? retryableExceptions = null)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= maxRetries)
        {
            try
            {
                if (attempt > 0)
                {
                    logger?.LogInformation("Retry #{Attempt} of {MaxRetries}", attempt, maxRetries);
                }

                var result = await operation();

                if (attempt > 0)
                {
                    logger?.LogInformation("Operation successful on attempt #{Attempt}", attempt + 1);
                }

                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (retryableExceptions != null && !retryableExceptions.Contains(ex.GetType()))
                {
                    logger?.LogWarning("Non-retryable exception: {Exception}", ex.GetType().Name);
                    throw;
                }

                if (attempt > maxRetries)
                {
                    logger?.LogError(ex, "Operation failed after {MaxRetries} retries: {Error}", maxRetries, ex.Message);
                    throw;
                }

                var delayMs = (int)(baseDelayMs * Math.Pow(2, attempt - 1));

                logger?.LogWarning(ex, "Attempt #{Attempt} failed: {Error}. Retrying in {Delay}ms...",
                    attempt, ex.Message, delayMs);

                await Task.Delay(delayMs);
            }
        }

        throw lastException ?? new ApplicationError("Operation failed without registered exception");
    }

    /// <summary>
    /// Executes operation with fast retries (no exponential backoff)
    /// </summary>
    public static async Task ExecuteWithFastRetryAsync(
        Func<Task> operation,
        int maxRetries = 3,
        int fixedDelayMs = 500,
        ILogger? logger = null)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= maxRetries)
        {
            try
            {
                if (attempt > 0)
                {
                    logger?.LogInformation("Fast retry #{Attempt} of {MaxRetries}", attempt, maxRetries);
                }

                await operation();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (attempt > maxRetries)
                {
                    logger?.LogError(ex, "Operation failed after {MaxRetries} fast retries: {Error}", maxRetries, ex.Message);
                    throw;
                }

                logger?.LogWarning(ex, "Fast retry #{Attempt} failed: {Error}. Retrying in {Delay}ms...",
                    attempt, ex.Message, fixedDelayMs);

                await Task.Delay(fixedDelayMs);
            }
        }

        throw lastException ?? new ApplicationError("Operation failed without registered exception");
    }
}

/// <summary>
/// Builder for configuring and executing operations with retries
/// </summary>
public class RetryBuilder
{
    private readonly Func<Task> _operation;
    private int _maxRetries = 3;
    private int _baseDelayMs = 1000;
    private Type[]? _retryableExceptions;
    private ILogger? _logger;

    internal RetryBuilder(Func<Task> operation)
    {
        _operation = operation;
    }

    /// <summary>
    /// Configures the maximum number of retries
    /// </summary>
    public RetryBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Configures the base delay in milliseconds
    /// </summary>
    public RetryBuilder WithBaseDelay(int baseDelayMs)
    {
        _baseDelayMs = baseDelayMs;
        return this;
    }

    /// <summary>
    /// Configures which exceptions are retryable
    /// </summary>
    public RetryBuilder WithRetryableExceptions(params Type[] retryableExceptions)
    {
        _retryableExceptions = retryableExceptions;
        return this;
    }

    /// <summary>
    /// Configures the logger
    /// </summary>
    public RetryBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Executes the operation with the configured settings
    /// </summary>
    public async Task ExecuteAsync()
    {
        await RetryHelper.ExecuteWithRetryAsync(
            _operation,
            _maxRetries,
            _baseDelayMs,
            _logger,
            _retryableExceptions
        );
    }
}
