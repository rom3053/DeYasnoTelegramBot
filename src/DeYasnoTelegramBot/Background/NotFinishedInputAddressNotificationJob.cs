using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Background;

public class NotFinishedInputAddressNotificationJob : BackgroundService
{
    private readonly ILogger<RemoveScheduleWithoutSubsJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    public NotFinishedInputAddressNotificationJob(ILogger<RemoveScheduleWithoutSubsJob> logger,
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
                if (CronHelper.IsTimeToExecute("* 9 * * *"))
                {
                    _logger.LogInformation("Background {JobName} service executed", nameof(NotFinishedInputAddressNotificationJob));

                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var botClient = scope.ServiceProvider.GetRequiredService<TelegramBotClientSender>();

                    var subs = await context.Subscribers.Where(x => x.InputStep != Domain.Enums.OutageInputStep.Step_0)
                        .Select(x => x.ChatId)
                        .ToListAsync();

                    foreach (var chatId in subs)
                    {
                        await botClient.SendMessageAsync(chatId, NotificationMessages.Message_About_NotFinishedInputAddress);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(60));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

        }
    }
}
