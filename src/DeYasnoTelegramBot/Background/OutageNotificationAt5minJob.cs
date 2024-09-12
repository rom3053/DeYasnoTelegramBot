using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Services;
using Microsoft.FeatureManagement;
using Telegram.Bot;

namespace DeYasnoTelegramBot.Background;

public class OutageNotificationAt5minJob : BackgroundService
{
    private readonly ILogger<OutageNotificationAt5minJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    private static HashSet<long> _outageNotifed = [];
    private static HashSet<long> _greyZoneNotifed = [];
    private static HashSet<long> _powerOnNotifed = [];

    public OutageNotificationAt5minJob(ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<OutageNotificationAt5minJob> logger)
    {
        _periodicTimer = new(TimeSpan.FromSeconds(20));
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
       //ToDO add bool for deactivaion notifications

        while (await _periodicTimer.WaitForNextTickAsync(cancellationToken) &&
            !cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (CronHelper.IsTimeToExecute("55 * * * *"))
                {
                    _logger.LogInformation("Background {JobName} service executed", nameof(OutageNotificationAt5minJob));
                    //TODO add toogle feacture for deactivated notifications
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<OutageNotificationService>();
                    var manager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();

                    if (!await manager.IsEnabledAsync("OutageNotification"))
                    {
                        _logger.LogInformation("Background {JobName} service disabled", nameof(OutageNotificationAt5minJob));
                        return;
                    }

                    await notificationService.NotifyIn5min(_outageNotifed, _greyZoneNotifed, _powerOnNotifed);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.InnerException);
            }
        }
    }
}
