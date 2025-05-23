﻿using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Services;
using Microsoft.FeatureManagement;

namespace DeYasnoTelegramBot.Background.OutageNotificationJobs;

public class OutageNotificationAt15minJob : BackgroundService
{
    private readonly ILogger<OutageNotificationAt15minJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    private static HashSet<long> _outageNotifed15min = [];
    private static HashSet<long> _greyZoneNotifed15min = [];
    private static HashSet<long> _powerOnNotifed15min = [];

    public OutageNotificationAt15minJob(
        IServiceProvider serviceProvider,
        ILogger<OutageNotificationAt15minJob> logger)
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
                if (CronHelper.IsTimeToExecute("45 * * * *"))
                {
                    _logger.LogInformation("Background {JobName} service executed", nameof(OutageNotificationAt15minJob));
                    //TODO add toogle feacture for deactivated notifications
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<OutageNotificationService>();
                    //var manager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();

                    //if (!await manager.IsEnabledAsync("OutageNotification"))
                    //{
                    //    _logger.LogInformation("Background {JobName} service disabled", nameof(OutageNotificationAt15minJob));
                    //    return;
                    //}

                    await notificationService.NotifyIn15minAsync(_outageNotifed15min, _greyZoneNotifed15min, _powerOnNotifed15min);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
