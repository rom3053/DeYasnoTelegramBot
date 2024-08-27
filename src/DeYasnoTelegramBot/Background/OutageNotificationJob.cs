using DeYasnoTelegramBot.Infrastructure.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using DeYasnoTelegramBot.Application.Common.Extensions;
using System.Collections.Concurrent;
using DeYasnoTelegramBot.Application.Common.Helpers;
using Microsoft.FeatureManagement;

namespace DeYasnoTelegramBot.Background;

public class OutageNotificationJob : BackgroundService
{
    private readonly ILogger<OutageNotificationJob> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    //private readonly YasnoScrappingConfig _config;
    private readonly PeriodicTimer _periodicTimer;

    private static HashSet<long> _outageNotifed = [];
    private static HashSet<long> _greyZoneNotifed = [];
    private static HashSet<long> _powerOnNotifed = [];

    private static HashSet<long> _outageNotifed15min = [];
    private static HashSet<long> _greyZoneNotifed15min = [];
    private static HashSet<long> _powerOnNotifed15min = [];

    private static HashSet<long> _outageNotifed30min = [];
    private static HashSet<long> _greyZoneNotifed30min = [];
    private static HashSet<long> _powerOnNotifed30min = [];

    const int ZERO_HOUR = 0;
    const int TWENTY_TRHEE_HOUR = 23;

    public OutageNotificationJob(ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<OutageNotificationJob> logger)
    {
        _periodicTimer = new(TimeSpan.FromSeconds(20));
        _botClient = botClient;
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
                _logger.LogInformation("Background {JobName} service executed", nameof(OutageNotificationJob));
                //TODO add toogle feacture for deactivated notifications
                await using var scope = _serviceProvider.CreateAsyncScope();
                var cachedScheduleStorage = scope.ServiceProvider.GetRequiredService<OutageScheduleStorage>();
                var manager = scope.ServiceProvider.GetRequiredService<IFeatureManager>();
                var sessions = cachedScheduleStorage.NotificationList;

                
                if (!await manager.IsEnabledAsync("OutageNotification"))
                {
                    _logger.LogInformation("Background {JobName} service disabled", nameof(OutageNotificationJob));
                    return;
                }

                var ukraineDateTimeNow = DateTime.UtcNow;
                var dateTimeToNotificationNow = DateTime.UtcNow + TimeSpan.FromMinutes(5);
                // Define the Ukraine time zone (Kyiv time)

                string timeZoneId;

                if (OperatingSystem.IsWindows())
                {
                    timeZoneId = "FLE Standard Time";  // Windows time zone ID
                }
                else
                {
                    timeZoneId = "Europe/Kyiv";  // IANA time zone ID for Linux/macOS
                }

                // Find the time zone info based on the ID
                TimeZoneInfo ukraineTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

                ukraineDateTimeNow = TimeZoneInfo.ConvertTimeFromUtc(ukraineDateTimeNow, ukraineTimeZone);
                // Convert the UTC time to Ukraine time
                DateTime ukraineNotificationTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeToNotificationNow, ukraineTimeZone);

                //renamed
                var notificationHour = ukraineNotificationTime.TimeOfDay.Hours;
                var prevNotificationHour = notificationHour - 1;

                NotifyOutage(sessions, ukraineNotificationTime, notificationHour, prevNotificationHour, ukraineDateTimeNow);
                NotifyGreyZone(sessions, ukraineNotificationTime, ukraineDateTimeNow, notificationHour, prevNotificationHour);
                NotifyPowerOn(sessions, ukraineNotificationTime, ukraineDateTimeNow, notificationHour, prevNotificationHour);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        void NotifyOutage(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, int notificationHour, int prevHour, DateTime realTime)
        {
            var needNotify = sessions.Select(x => x.Value)
                .Select(a => new
                {
                    a.ChatIds,
                    a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                          .FirstOrDefault()?.OutageHours,
                    PreviousDayOutageHour_23 = a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                          .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
                })
                //validate previous day and hour_23
                .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOff)
                            && x.PreviousDayOutageHour_23.Hour == prevHour && x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOn)
                .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOff)
                            && x.OutageHours.Any(sh => sh.Hour == prevHour && sh.Status == Domain.Enums.OutageStatus.PowerOn))
                .Select(r => new { r.ChatIds })
                .SelectMany(rr => rr.ChatIds)
                .ToHashSet();

            //remove already notified
            needNotify.ExceptWith(_outageNotifed);
            if (needNotify.Count != 0)
            {
                foreach (var chatId in needNotify)
                {
                    SendMessage(chatId, NotificationMessages.Message_About_5min_PowerOff);
                }
                //add new notifed
                _outageNotifed.UnionWith(needNotify);
                //clean next notification step
                _greyZoneNotifed.ExceptWith(needNotify);
            }
        }

        void NotifyGreyZone(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour)
        {
            var needNotify = sessions.Select(x => x.Value)
                .Select(a => new
                {
                    a.ChatIds,
                    a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                          .FirstOrDefault()?.OutageHours,
                    PreviousDayOutageHour_23 = a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                          .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
                })
                //validate previous day and hour_23
                .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerPossibleOn)
                            && x.PreviousDayOutageHour_23.Hour == prevHour && x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOff)
                .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerPossibleOn)
                            && x.OutageHours.Any(sh => sh.Hour == prevHour && sh.Status == Domain.Enums.OutageStatus.PowerOff))
                .Select(r => new { r.ChatIds })
                .SelectMany(rr => rr.ChatIds)
                .ToHashSet();

            //ToDo maybe add range from to about gray zone

            //remove already notified
            needNotify.ExceptWith(_greyZoneNotifed);
            if (needNotify.Count != 0)
            {
                foreach (var chatId in needNotify)
                {
                    //ToDO move to some class
                    SendMessage(chatId, NotificationMessages.Message_About_5min_PossiblePowerOn);
                }
                //add new notifed
                _greyZoneNotifed.UnionWith(needNotify);
                //clean next notification step
                _powerOnNotifed.ExceptWith(needNotify);
            }
        }

        void NotifyPowerOn(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour)
        {
            var needNotify = sessions.Select(x => x.Value)
                .Select(a => new
                {
                    a.ChatIds,
                    a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                          .FirstOrDefault()?.OutageHours,
                    PreviousDayOutageHour_23 = a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                          .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
                })
                //validate previous day and hour_23
                .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOn)
                            && x.PreviousDayOutageHour_23.Hour == prevHour && (x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOff || x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerPossibleOn))
                .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOn)
                            && x.OutageHours.Any(sh => sh.Hour == prevHour && (sh.Status == Domain.Enums.OutageStatus.PowerPossibleOn || sh.Status == Domain.Enums.OutageStatus.PowerOff)))
                .Select(r => new { r.ChatIds })
                .SelectMany(rr => rr.ChatIds)
                .ToHashSet();

            //remove already notified
            needNotify.ExceptWith(_powerOnNotifed);
            if (needNotify.Count != 0)
            {
                foreach (var chatId in needNotify)
                {
                    SendMessage(chatId, NotificationMessages.Message_About_5min_PowerOn);
                }
                //add new notifed
                _powerOnNotifed.UnionWith(needNotify);
                //clean
                _outageNotifed.ExceptWith(needNotify);
            }
        }

        //ToDo variant with 5 15 30 
        void NotifyAllPowerOnV2(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour)
        {
            NotifyPowerOnV2(sessions, notificationTime + TimeSpan.FromMinutes(5), realTime, notificationHour, prevHour, _powerOnNotifed, _outageNotifed);
            NotifyPowerOnV2(sessions, notificationTime + TimeSpan.FromMinutes(15), realTime, notificationHour, prevHour, _powerOnNotifed15min, _outageNotifed15min);
            NotifyPowerOnV2(sessions, notificationTime + TimeSpan.FromMinutes(30), realTime, notificationHour, prevHour, _powerOnNotifed30min, _outageNotifed30min);
        }

        //split to different backgound jobs with defferent inverval 15 every 5min 10 min av
        void NotifyPowerOnV2(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour, HashSet<long> powerOnNotificationFlags, HashSet<long> nextNotificationFlags)
        {
            var needNotify = sessions.Select(x => x.Value)
                .Select(a => new
                {
                    a.ChatIds,
                    a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                          .FirstOrDefault()?.OutageHours,
                    PreviousDayOutageHour_23 = a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                          .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
                })
                //validate previous day and hour_23
                .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOn)
                            && x.PreviousDayOutageHour_23.Hour == prevHour && (x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOff || x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerPossibleOn))
                .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour > notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOn)
                            && x.OutageHours.Any(sh => sh.Hour == prevHour && (sh.Status == Domain.Enums.OutageStatus.PowerPossibleOn || sh.Status == Domain.Enums.OutageStatus.PowerOff)))
                .Select(r => new { r.ChatIds })
                .SelectMany(rr => rr.ChatIds)
                .ToHashSet();

            //remove already notified
            needNotify.ExceptWith(powerOnNotificationFlags);
            if (needNotify.Count != 0)
            {
                foreach (var chatId in needNotify)
                {
                    SendMessage(chatId, "Світло буде приблизно через 5 хвилин.");
                }
                //add new notifed
                powerOnNotificationFlags.UnionWith(needNotify);
                //clean
                nextNotificationFlags.ExceptWith(needNotify);
            }
        }
    }

    async Task SendMessage(long chatId, string text)
    {
        try
        {
            var message = await _botClient.SendTextMessageAsync(chatId, text,
                parseMode: ParseMode.Html,
                protectContent: false);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }
}
