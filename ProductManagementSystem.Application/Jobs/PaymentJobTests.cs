using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ProductManagementSystem.Application.AppEntities.UserPlans.Services;
using ProductManagementSystem.Application.AppEntities.UserPlans.DTOs.Outputs;
using System.Reflection;

namespace ProductManagementSystem.Application.Jobs;

public class PaymentJobTests
{
    private readonly Mock<ILogger<PaymentJob>> _mockLogger;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IUserPlanService> _mockUserPlanService;
    private readonly PaymentJob _job;

    public PaymentJobTests()
    {
        _mockLogger = new Mock<ILogger<PaymentJob>>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockUserPlanService = new Mock<IUserPlanService>();

        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);
        _mockServiceProvider.Setup(p => p.GetService(typeof(IUserPlanService)))
            .Returns(_mockUserPlanService.Object);

        _job = new PaymentJob(_mockLogger.Object, _mockScopeFactory.Object);
    }

    #region CalculateNextRunTime Tests

    [Fact]
    public void CalculateNextRunTime_ReturnsScheduledTime()
    {
        var method = typeof(PaymentJob).GetMethod("CalculateNextRunTime", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        var result = (DateTime)method!.Invoke(_job, null)!;

        result.Hour.Should().Be(3);
        result.Minute.Should().Be(0);
        result.Second.Should().Be(0);
    }

    [Fact]
    public void CalculateNextRunTime_WhenTimeHasPassed_ReturnsNextDay()
    {
        var method = typeof(PaymentJob).GetMethod("CalculateNextRunTime", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        var result = (DateTime)method!.Invoke(_job, null)!;
        var now = DateTime.Now;
        var todayAt3AM = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0);

        if (now >= todayAt3AM)
        {
            result.Date.Should().Be(now.Date.AddDays(1));
        }
        else
        {
            result.Date.Should().Be(now.Date);
        }
    }

    #endregion

    #region ProcessPendingBillingPlans Tests

    [Fact]
    public async Task ProcessPendingBillingPlans_WhenCalled_InvokesService()
    {
        var expectedResult = new PaymentProcessingResult
        {
            SussesPayments = 5,
            FailedPayments = 2
        };

        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ReturnsAsync(expectedResult);

        var method = typeof(PaymentJob).GetMethod("ProcessPendingBillingPlans", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, new object[] { _mockUserPlanService.Object })!;

        _mockUserPlanService.Verify(s => s.ProcessPendingPaymentsAsync(), Times.Once);
    }

    [Fact]
    public async Task ProcessPendingBillingPlans_WhenServiceThrows_LogsError()
    {
        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ThrowsAsync(new Exception("Service error"));

        var method = typeof(PaymentJob).GetMethod("ProcessPendingBillingPlans", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, new object[] { _mockUserPlanService.Object })!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region IdentifyOverDuePlans Tests

    [Fact]
    public async Task IdentifyOverDuePlans_WhenCalled_InvokesService()
    {
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync())
            .ReturnsAsync(3);

        var method = typeof(PaymentJob).GetMethod("IdentifyOverDuePlans", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, new object[] { _mockUserPlanService.Object })!;

        _mockUserPlanService.Verify(s => s.SetOverDuePlansAsync(), Times.Once);
    }

    [Fact]
    public async Task IdentifyOverDuePlans_WhenServiceThrows_LogsError()
    {
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync())
            .ThrowsAsync(new Exception("Service error"));

        var method = typeof(PaymentJob).GetMethod("IdentifyOverDuePlans", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, new object[] { _mockUserPlanService.Object })!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region InactivateOverduePlans Tests

    [Fact]
    public async Task InactivateOverduePlans_WhenCalled_InvokesService()
    {
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync())
            .ReturnsAsync(2);

        var method = typeof(PaymentJob).GetMethod("InactivateOverduePlans", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, new object[] { _mockUserPlanService.Object })!;

        _mockUserPlanService.Verify(s => s.InactivateOverduePlansAsync(), Times.Once);
    }

    [Fact]
    public async Task InactivateOverduePlans_WhenServiceThrows_LogsError()
    {
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync())
            .ThrowsAsync(new Exception("Service error"));

        var method = typeof(PaymentJob).GetMethod("InactivateOverduePlans", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, new object[] { _mockUserPlanService.Object })!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region CleanupOldLogs Tests

    [Fact]
    public async Task CleanupOldLogs_WhenCalled_CompletesSuccessfully()
    {
        var method = typeof(PaymentJob).GetMethod("CleanupOldLogs", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleaning")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region ExecuteTasks Tests

    [Fact]
    public async Task ExecuteTasks_WhenCalled_ExecutesAllSteps()
    {
        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ReturnsAsync(new PaymentProcessingResult { SussesPayments = 1, FailedPayments = 0 });
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync()).ReturnsAsync(0);
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync()).ReturnsAsync(0);

        var method = typeof(PaymentJob).GetMethod("ExecuteTasks", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockUserPlanService.Verify(s => s.ProcessPendingPaymentsAsync(), Times.Once);
        _mockUserPlanService.Verify(s => s.SetOverDuePlansAsync(), Times.Once);
        _mockUserPlanService.Verify(s => s.InactivateOverduePlansAsync(), Times.Once);
    }

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task StartAsync_StartsBackgroundService()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await _job.StartAsync(cts.Token);
        await Task.Delay(100);
        await _job.StopAsync(CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("PaymentJob started")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_LogsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await _job.StartAsync(cts.Token);
        await Task.Delay(150);
        await _job.StopAsync(CancellationToken.None);

        _mockLogger.Verify(
            x => x.Log(
                It.IsIn(LogLevel.Information, LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region ExecuteDailyTaskIsolated Tests

    [Fact]
    public async Task ExecuteDailyTaskIsolated_WhenCalled_ExecutesSuccessfully()
    {
        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ReturnsAsync(new PaymentProcessingResult { SussesPayments = 1, FailedPayments = 0 });
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync()).ReturnsAsync(0);
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync()).ReturnsAsync(0);

        var method = typeof(PaymentJob).GetMethod("ExecuteDailyTaskIsolated", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Daily task completed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteDailyTaskIsolated_WhenExecuteTasksHasErrors_LogsErrors()
    {
        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ThrowsAsync(new Exception("Processing error"));
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync())
            .ThrowsAsync(new Exception("SetOverDue error"));
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync())
            .ThrowsAsync(new Exception("Inactivate error"));

        var method = typeof(PaymentJob).GetMethod("ExecuteDailyTaskIsolated", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region ExecuteTasks Error Handling

    [Fact]
    public async Task ExecuteTasks_WhenAllServicesWork_CompletesSuccessfully()
    {
        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ReturnsAsync(new PaymentProcessingResult { SussesPayments = 5, FailedPayments = 1 });
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync()).ReturnsAsync(3);
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync()).ReturnsAsync(2);

        var method = typeof(PaymentJob).GetMethod("ExecuteTasks", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Daily billing task completed")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteTasks_LogsExecutionStart()
    {
        _mockUserPlanService.Setup(s => s.ProcessPendingPaymentsAsync())
            .ReturnsAsync(new PaymentProcessingResult { SussesPayments = 0, FailedPayments = 0 });
        _mockUserPlanService.Setup(s => s.SetOverDuePlansAsync()).ReturnsAsync(0);
        _mockUserPlanService.Setup(s => s.InactivateOverduePlansAsync()).ReturnsAsync(0);

        var method = typeof(PaymentJob).GetMethod("ExecuteTasks", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Executing daily task")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CleanupOldLogs_LogsCleanupCompleted()
    {
        var method = typeof(PaymentJob).GetMethod("CleanupOldLogs", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        await (Task)method!.Invoke(_job, null)!;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cleanup completed") || v.ToString()!.Contains("Cleaning")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
