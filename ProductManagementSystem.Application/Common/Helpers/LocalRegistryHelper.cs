using System.Collections.Concurrent;
using System.Text.Json;
using ProductManagementSystem.Application.Common.AppEntities.Errors;

namespace ProductManagementSystem.Application.Common.Helpers;

public static class LocalRegistryHelper
{
    private static readonly SemaphoreSlim _writeLock = new(1, 1);
    private static readonly string _defaultRegistryPath = Path.Combine("logs", "local_registry.json");
    private static readonly ConcurrentQueue<FailureRecord> _pendingWrites = new();
    private static volatile bool _flushInProgress = false;

    /// <summary>
    /// Creates a builder for constructing and registering a failure
    /// </summary>
    public static FailureBuilder CreateFailure()
    {
        return new FailureBuilder();
    }

    internal static Task RegisterFailureAsync(
        FailureOperationType operationType,
        string entityId,
        string errorMessage,
        object? additionalData = null,
        ILogger? logger = null,
        string? registryPath = null)
    {
        var record = new FailureRecord
        {
            Id = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow,
            OperationType = operationType,
            EntityId = entityId,
            ErrorMessage = errorMessage,
            AdditionalData = additionalData,
            RetryCount = 0,
            Status = FailureStatus.Pending
        };

        _pendingWrites.Enqueue(record);

        logger?.LogInformation("Failure registered locally: {OperationType} for entity {EntityId}",
            operationType, entityId);

        _ = Task.Run(() => FlushPendingRecordsAsync(registryPath, logger));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Registers a retry attempt for a failed operation
    /// </summary>
    public static async Task RegisterRetryAsync(
        string failureId,
        string? newErrorMessage = null,
        ILogger? logger = null,
        string? registryPath = null)
    {
        await _writeLock.WaitAsync();
        try
        {
            var filePath = registryPath ?? _defaultRegistryPath;
            var records = await LoadRecordsAsync(filePath);

            var record = records.FirstOrDefault(r => r.Id == failureId);
            if (record != null)
            {
                record.RetryCount++;
                record.LastRetryAttempt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(newErrorMessage))
                {
                    record.ErrorMessage = newErrorMessage;
                }

                await SaveRecordsAsync(records, filePath);

                logger?.LogInformation("Retry #{RetryCount} registered for failure {FailureId}",
                    record.RetryCount, failureId);
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Marks a failure as resolved
    /// </summary>
    public static async Task MarkAsResolvedAsync(
        string failureId,
        string? resolutionNotes = null,
        ILogger? logger = null,
        string? registryPath = null)
    {
        await _writeLock.WaitAsync();
        try
        {
            var filePath = registryPath ?? _defaultRegistryPath;
            var records = await LoadRecordsAsync(filePath);

            var record = records.FirstOrDefault(r => r.Id == failureId);
            if (record != null)
            {
                record.Status = FailureStatus.Resolved;
                record.ResolvedAt = DateTime.UtcNow;
                record.ResolutionNotes = resolutionNotes;

                await SaveRecordsAsync(records, filePath);

                logger?.LogInformation("Failure {FailureId} marked as resolved", failureId);
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Gets all pending failures
    /// </summary>
    public static async Task<List<FailureRecord>> GetPendingFailuresAsync(string? registryPath = null)
    {
        var filePath = registryPath ?? _defaultRegistryPath;
        var records = await LoadRecordsAsync(filePath);

        return records.Where(r => r.Status == FailureStatus.Pending).ToList();
    }

    /// <summary>
    /// Cleans up old resolved records
    /// </summary>
    public static async Task CleanupOldRecordsAsync(
        int olderThanDays = 30,
        ILogger? logger = null,
        string? registryPath = null)
    {
        await _writeLock.WaitAsync();
        try
        {
            var filePath = registryPath ?? _defaultRegistryPath;
            var records = await LoadRecordsAsync(filePath);

            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
            var initialCount = records.Count;

            records.RemoveAll(r =>
                r.Status == FailureStatus.Resolved &&
                r.ResolvedAt.HasValue &&
                r.ResolvedAt.Value < cutoffDate);

            if (records.Count < initialCount)
            {
                await SaveRecordsAsync(records, filePath);
                var removedCount = initialCount - records.Count;

                logger?.LogInformation("Cleanup completed: {RemovedCount} old records removed", removedCount);
            }
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Manual flush of pending records
    /// </summary>
    public static async Task FlushPendingRecordsAsync(string? registryPath = null, ILogger? logger = null)
    {
        if (_flushInProgress || _pendingWrites.IsEmpty)
            return;

        _flushInProgress = true;

        try
        {
            await _writeLock.WaitAsync();
            try
            {
                var filePath = registryPath ?? _defaultRegistryPath;
                var existingRecords = await LoadRecordsAsync(filePath);

                var newRecords = new List<FailureRecord>();
                while (_pendingWrites.TryDequeue(out var record))
                {
                    newRecords.Add(record);
                }

                if (newRecords.Count > 0)
                {
                    existingRecords.AddRange(newRecords);
                    await SaveRecordsAsync(existingRecords, filePath);

                    logger?.LogInformation("Flush completed: {Count} new records saved", newRecords.Count);
                }
            }
            finally
            {
                _writeLock.Release();
            }
        }
        finally
        {
            _flushInProgress = false;
        }
    }

    private static async Task<List<FailureRecord>> LoadRecordsAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return new List<FailureRecord>();
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<FailureRecord>>(json) ?? new List<FailureRecord>();
        }
        catch
        {
            return new List<FailureRecord>();
        }
    }

    private static async Task SaveRecordsAsync(List<FailureRecord> records, string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(records, options);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception)
        {
            // Ignore write errors to not affect the main flow
        }
    }
}

public class FailureRecord
{
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public FailureOperationType OperationType { get; set; } = FailureOperationType.UnknownFailure;
    public string EntityId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public object? AdditionalData { get; set; }
    public int RetryCount { get; set; }
    public DateTime? LastRetryAttempt { get; set; }
    public FailureStatus Status { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}

public enum FailureStatus
{
    Pending,
    Resolved,
    Abandoned
}

public class FailureBuilder
{
    private FailureOperationType _operationType = FailureOperationType.UnknownFailure;
    private string _entityId = string.Empty;
    private string _errorMessage = string.Empty;
    private object? _additionalData;
    private ILogger? _logger;
    private string? _registryPath;

    /// <summary>
    /// Sets the type of operation that failed
    /// </summary>
    public FailureBuilder WithOperationType(FailureOperationType operationType)
    {
        _operationType = operationType;
        return this;
    }

    /// <summary>
    /// Sets the ID of the affected entity
    /// </summary>
    public FailureBuilder WithEntityId(string entityId)
    {
        _entityId = entityId;
        return this;
    }

    /// <summary>
    /// Sets the error message
    /// </summary>
    public FailureBuilder WithMessage(string errorMessage)
    {
        _errorMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Adds additional data to the failure
    /// </summary>
    public FailureBuilder WithAdditionalData(object additionalData)
    {
        _additionalData = additionalData;
        return this;
    }

    /// <summary>
    /// Sets the logger for the operation
    /// </summary>
    public FailureBuilder WithLogger(ILogger logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Sets the registry file path
    /// </summary>
    public FailureBuilder WithRegistryPath(string registryPath)
    {
        _registryPath = registryPath;
        return this;
    }

    /// <summary>
    /// Registers the failure with the configured settings
    /// </summary>
    public async Task RegisterAsync()
    {
        if (string.IsNullOrEmpty(_entityId))
            throw new ApplicationError("EntityId is required to register a failure");

        if (string.IsNullOrEmpty(_errorMessage))
            throw new ApplicationError("ErrorMessage is required to register a failure");

        await LocalRegistryHelper.RegisterFailureAsync(
            _operationType,
            _entityId,
            _errorMessage,
            _additionalData,
            _logger,
            _registryPath
        );
    }
}
