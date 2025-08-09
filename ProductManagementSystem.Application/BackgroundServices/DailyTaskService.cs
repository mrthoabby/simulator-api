using ProductManagementSystem.Application.Domain.UserPlans.Services;

namespace ProductManagementSystem.Application.BackgroundServices;

public class DailyTaskService : BackgroundService
{
    private readonly ILogger<DailyTaskService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DailyTaskService(ILogger<DailyTaskService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () =>
        {
            var backgroundThreadId = Thread.CurrentThread.ManagedThreadId;
            _logger.LogInformation("🧵 DailyTaskService iniciado en hilo separado: {ThreadId}", backgroundThreadId);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nextRunTime = CalculateNextRunTime();
                    var delay = nextRunTime - DateTime.Now;

                    _logger.LogInformation("⏰ Próxima ejecución programada: {NextRun} (en {DelayHours:F1} horas)",
                        nextRunTime, delay.TotalHours);

                    await Task.Delay(delay, stoppingToken);

                    await ExecuteDailyTaskIsolated();

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🛑 DailyTaskService cancelado");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error en DailyTaskService. Reintentando en 1 hora...");
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

            _logger.LogInformation("🚀 Iniciando tarea diaria en hilo: {ThreadId} a las {StartTime}",
                taskThreadId, startTime);

            try
            {
                await ExecuteTasks();

                var duration = DateTime.Now - startTime;
                _logger.LogInformation("✅ Tarea diaria completada exitosamente en {Duration:F1} segundos",
                    duration.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error ejecutando tarea diaria");
                throw; // Re-lanzar para logging en el nivel superior
            }
        });
    }

    private async Task ExecuteTasks()
    {
        _logger.LogInformation("📋 Ejecutando tarea diaria de revisión de planes de facturación...");

        // 🎯 LÓGICA ESPECÍFICA DE BILLING
        using var scope = _serviceScopeFactory.CreateScope();
        var userPlanService = scope.ServiceProvider.GetRequiredService<IUserPlanService>();

        try
        {

            await ProcessPendingBillingPlans(userPlanService);

            await IdentifyOverDuePlans(userPlanService);

            await InactivateOverduePlans(userPlanService);

            await CleanupOldLogs();

            _logger.LogInformation("🎉 Tarea diaria de billing completada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error ejecutando tarea diaria de billing");
            throw;
        }
    }

    private DateTime CalculateNextRunTime()
    {
        var now = DateTime.Now;
        var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0); // 3:00 AM

        // Si ya pasó la hora hoy, programar para mañana
        if (now >= scheduledTime)
        {
            scheduledTime = scheduledTime.AddDays(1);
        }

        return scheduledTime;
    }

    #region Métodos específicos de Billing

    private async Task ProcessPendingBillingPlans(IUserPlanService userPlanService)
    {
        _logger.LogInformation("💰 Procesando planes pendientes de facturación...");

        try
        {
            var pendingPlans = await userPlanService.ProcessPendingPaymentsAsync();

            _logger.LogInformation("💰 Procesados {Count} planes pendientes de facturación", pendingPlans.SussesPayments);
            _logger.LogInformation("❌ Procesados {Count} planes pendientes de facturación", pendingPlans.FailedPayments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error procesando planes pendientes");
        }
    }

    private async Task IdentifyOverDuePlans(IUserPlanService userPlanService)
    {
        _logger.LogInformation("⚠️ Revisando planes vencidos...");

        try
        {
            await userPlanService.SetOverDuePlansAsync();

            _logger.LogInformation("✅ Planes vencidos identificados y actualizados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error revisando planes vencidos");
        }
    }

    private async Task InactivateOverduePlans(IUserPlanService userPlanService)
    {
        _logger.LogInformation("🚫 Inactivando planes muy vencidos");

        try
        {
            await userPlanService.InactivateOverduePlansAsync();

            _logger.LogInformation("✅ Planes vencidos inactivados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inactivando planes vencidos");
        }
    }

    private async Task CleanupOldLogs()
    {
        _logger.LogInformation("🧹 Limpiando logs antiguos...");

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
                    _logger.LogDebug("🗑️ Eliminado log antiguo: {File}", Path.GetFileName(file));
                }

                _logger.LogInformation("✅ Limpieza completada. {Count} archivos eliminados", oldFiles.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error durante limpieza de logs");
        }

        await Task.Delay(100);
    }

    #endregion
}
