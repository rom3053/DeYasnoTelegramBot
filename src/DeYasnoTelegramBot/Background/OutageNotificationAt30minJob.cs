using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Services;
using Microsoft.FeatureManagement;

namespace DeYasnoTelegramBot.Background;

public class OutageNotificationAt30minJob : BackgroundService
{
    private readonly ILogger<OutageNotificationAt30minJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    private static HashSet<long> _outageNotifed30min = [];
    private static HashSet<long> _greyZoneNotifed30min = [];
    private static HashSet<long> _powerOnNotifed30min = [];

    public OutageNotificationAt30minJob(
        IServiceProvider serviceProvider,
        ILogger<OutageNotificationAt30minJob> logger)
    {
        _periodicTimer = new(TimeSpan.FromSeconds(20));
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(cancellationToken) &&
            !cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (CronHelper.IsTimeToExecute("30 * * * *"))
                {
                    _logger.LogInformation("Background {JobName} service executed", nameof(OutageNotificationAt30minJob));
                    //TODO add toogle feacture for deactivated notifications
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<OutageNotificationService>();
                    //var manager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();

                    //if (!await manager.IsEnabledAsync("OutageNotification"))
                    //{
                    //    _logger.LogInformation("Background {JobName} service disabled", nameof(OutageNotificationAt30minJob));
                    //    return;
                    //}

                    await notificationService.NotifyIn30min(_outageNotifed30min, _greyZoneNotifed30min, _powerOnNotifed30min);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
