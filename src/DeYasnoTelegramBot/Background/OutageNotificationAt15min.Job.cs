
using DeYasnoTelegramBot.Infrastructure.Services;
using Microsoft.FeatureManagement;
using System.Threading;
using Telegram.Bot;

namespace DeYasnoTelegramBot.Background;

public class OutageNotificationAt15min : BackgroundService
{
    private readonly ILogger<OutageNotificationAt5minJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    private static HashSet<long> _outageNotifed15min = [];
    private static HashSet<long> _greyZoneNotifed15min = [];
    private static HashSet<long> _powerOnNotifed15min = [];

    public OutageNotificationAt15min(
        IServiceProvider serviceProvider,
        ILogger<OutageNotificationAt5minJob> logger)
    {
        _periodicTimer = new(TimeSpan.FromMinutes(5));
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
                _logger.LogInformation("Background {JobName} service executed", nameof(OutageNotificationAt15min));
                //TODO add toogle feacture for deactivated notifications
                await using var scope = _serviceProvider.CreateAsyncScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<OutageNotificationService>();
                var manager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();

                if (!await manager.IsEnabledAsync("OutageNotification"))
                {
                    _logger.LogInformation("Background {JobName} service disabled", nameof(OutageNotificationAt15min));
                    return;
                }

                notificationService.NotifyIn15min(_outageNotifed15min, _greyZoneNotifed15min, _powerOnNotifed15min);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
