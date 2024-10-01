using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Services;

namespace DeYasnoTelegramBot.Background;

public class RemoveScheduleWithoutSubsJob : BackgroundService
{
    private readonly ILogger<RemoveScheduleWithoutSubsJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    public RemoveScheduleWithoutSubsJob(ILogger<RemoveScheduleWithoutSubsJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _periodicTimer = new(TimeSpan.FromSeconds(30));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(cancellationToken) &&
            !cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (CronHelper.IsTimeToExecute("10 1 * * *"))
                {
                    _logger.LogInformation("Background {JobName} service executed", nameof(RemoveScheduleWithoutSubsJob));

                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var notificationStorage = scope.ServiceProvider.GetRequiredService<OutageScheduleStorage>();
                    notificationStorage.RemoveEmptySchedules();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
