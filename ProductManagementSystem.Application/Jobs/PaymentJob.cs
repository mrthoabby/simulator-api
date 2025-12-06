using ProductManagementSystem.Application.AppEntities.UserPlans.Services;

namespace ProductManagementSystem.Application.Jobs;

public class PaymentJob(ILogger<PaymentJob> logger, IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () =>
        {
            var backgroundThreadId = Thread.CurrentThread.ManagedThreadId;
            logger.LogInformation("PaymentJob started in thread: {ThreadId}", backgroundThreadId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nextRunTime = CalculateNextRunTime();
                    var delay = nextRunTime - DateTime.Now;

                    logger.LogInformation("Next execution scheduled: {NextRun} (in {DelayHours:F1} hours)",
                        nextRunTime, delay.TotalHours);

                    await Task.Delay(delay, stoppingToken);

                    await ExecuteDailyTaskIsolated();

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    logger.LogInformation("PaymentJob canceled");
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error in PaymentJob. Retrying in 1 hour...");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }, stoppingToken);
    }

    private async Task ExecuteDailyTaskIsolated()
    {
        await Task.Run(async () =>
        {
            Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

            var taskThreadId = Thread.CurrentThread.ManagedThreadId;
            var startTime = DateTime.Now;

            logger.LogInformation("Starting daily task in thread: {ThreadId} at {StartTime}",
        taskThreadId, startTime);

            try
            {
                await ExecuteTasks();

                var duration = DateTime.Now - startTime;
                logger.LogInformation("Daily task completed successfully in {Duration:F1} seconds",
                    duration.TotalSeconds);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing daily task");
                throw;
            }
        });
    }

    private async Task ExecuteTasks()
    {
        logger.LogInformation("Executing daily task to review billing plans...");

        using var scope = serviceScopeFactory.CreateScope();
        var userPlanService = scope.ServiceProvider.GetRequiredService<IUserPlanService>();

        try
        {
            await ProcessPendingBillingPlans(userPlanService);
            await IdentifyOverDuePlans(userPlanService);
            await InactivateOverduePlans(userPlanService);
            await CleanupOldLogs();

            logger.LogInformation("Daily billing task completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing daily billing task");
            throw;
        }
    }

    private DateTime CalculateNextRunTime()
    {
        var now = DateTime.Now;
        var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0);

        if (now >= scheduledTime)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        return scheduledTime;
    }

    private async Task ProcessPendingBillingPlans(IUserPlanService userPlanService)
    {
        logger.LogInformation("Processing pending billing plans...");

        try
        {
            var pendingPlans = await userPlanService.ProcessPendingPaymentsAsync();

            logger.LogInformation("Processed {SuccessCount} successful and {FailedCount} failed billing plans",
                pendingPlans.SussesPayments, pendingPlans.FailedPayments);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing pending plans");
        }
    }

    private async Task IdentifyOverDuePlans(IUserPlanService userPlanService)
    {
        logger.LogInformation("Reviewing overdue plans...");

        try
        {
            await userPlanService.SetOverDuePlansAsync();
            logger.LogInformation("Overdue plans identified and updated");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reviewing overdue plans");
        }
    }

    private async Task InactivateOverduePlans(IUserPlanService userPlanService)
    {
        logger.LogInformation("Inactivating very overdue plans");

        try
        {
            await userPlanService.InactivateOverduePlansAsync();
            logger.LogInformation("Overdue plans inactivated");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inactivating overdue plans");
        }
    }

    private async Task CleanupOldLogs()
    {
        logger.LogInformation("Cleaning old logs...");

        try
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (Directory.Exists(logDirectory))
            {
                var cutoffDate = DateTime.Now.AddDays(-30);
                var oldFiles = Directory.GetFiles(logDirectory, "*.txt")
                    .Where(file => File.GetCreationTime(file) < cutoffDate)
                    .ToList();

                foreach (var file in oldFiles)
                {
                    File.Delete(file);
                    logger.LogDebug("Old log deleted: {File}", Path.GetFileName(file));
                }

                logger.LogInformation("Cleanup completed. {Count} files deleted", oldFiles.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error during log cleanup");
        }

        await Task.Delay(100);
    }
}
