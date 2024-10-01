using System.Collections.Concurrent;
using DeYasnoTelegramBot.Application.Common.Extensions;
using DeYasnoTelegramBot.Application.Common.Helpers;
using Telegram.Bot;

namespace DeYasnoTelegramBot.Infrastructure.Services;

public class OutageNotificationService
{
    const int ZERO_HOUR = 0;
    const int TWENTY_TRHEE_HOUR = 23;

    private readonly OutageScheduleStorage _outageScheduleStorage;
    private readonly ILogger<OutageNotificationService> _logger;
    private readonly TelegramBotClientSender _botClientSender;

    public OutageNotificationService(
        OutageScheduleStorage outageScheduleStorage,
        ILogger<OutageNotificationService> logger,
        TelegramBotClientSender botClientSender)

    {
        _outageScheduleStorage = outageScheduleStorage;
        _logger = logger;
        _botClientSender = botClientSender;
    }

    public async Task NotifyIn5minAsync(HashSet<long> outageNotifed, HashSet<long> greyZoneNotifed, HashSet<long> powerOnNotifed)
    {
        var notificationList = _outageScheduleStorage.NotificationList;
        GetUkraineNotificationTimes(TimeSpan.FromMinutes(5), out var notificationTime, out var dateTimeNow, out var notificationHour, out var prevNotificationHour);

        await NotifyPowerOffV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, outageNotifed, greyZoneNotifed, NotificationMessages.Message_About_5min_PowerOff);
        await NotifyPossiblePowerOnV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, greyZoneNotifed, powerOnNotifed, NotificationMessages.Message_About_5min_PossiblePowerOn);
        await NotifyPowerOnV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, powerOnNotifed, outageNotifed, NotificationMessages.Message_About_5min_PowerOn);
    }

    public async Task NotifyIn15minAsync(HashSet<long> _outageNotifed, HashSet<long> _greyZoneNotifed, HashSet<long> _powerOnNotifed)
    {
        var notificationList = _outageScheduleStorage.NotificationList;
        GetUkraineNotificationTimes(TimeSpan.FromMinutes(15), out var notificationTime, out var dateTimeNow, out var notificationHour, out var prevNotificationHour);

        await NotifyPowerOffV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, _outageNotifed, _greyZoneNotifed, NotificationMessages.Message_About_15min_PowerOff);
        await NotifyPossiblePowerOnV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, _greyZoneNotifed, _powerOnNotifed, NotificationMessages.Message_About_15min_PossiblePowerOn);
        await NotifyPowerOnV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, _powerOnNotifed, _outageNotifed, NotificationMessages.Message_About_15min_PowerOn);
    }

    public async Task NotifyIn30minAsync(HashSet<long> _outageNotifed, HashSet<long> _greyZoneNotifed, HashSet<long> _powerOnNotifed)
    {
        var notificationList = _outageScheduleStorage.NotificationList;
        GetUkraineNotificationTimes(TimeSpan.FromMinutes(30), out var notificationTime, out var dateTimeNow, out var notificationHour, out var prevNotificationHour);

        await NotifyPowerOffV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, _outageNotifed, _greyZoneNotifed, NotificationMessages.Message_About_30min_PowerOff);
        await NotifyPossiblePowerOnV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, _greyZoneNotifed, _powerOnNotifed, NotificationMessages.Message_About_30min_PossiblePowerOn);
        await NotifyPowerOnV2Async(notificationList, notificationTime, dateTimeNow, notificationHour, prevNotificationHour, _powerOnNotifed, _outageNotifed, NotificationMessages.Message_About_30min_PowerOn);
    }

    private static void GetUkraineNotificationTimes(TimeSpan inTimeMinutes, out DateTime ukraineNotificationTime, out DateTime ukraineDateTimeNow, out int notificationHour, out int prevNotificationHour)
    {
        ukraineDateTimeNow = DateTimeHelper.GetUkraineTimeNow();
        ukraineNotificationTime = ukraineDateTimeNow + inTimeMinutes;

        notificationHour = ukraineNotificationTime.TimeOfDay.Hours;
        prevNotificationHour = notificationHour - 1;
    }

    private async Task NotifyPowerOffV2Async(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour, HashSet<long> powerOffNotificationFlags, HashSet<long> nextNotificationFlags, string notificationMessage)
    {
        var needNotify = sessions.Select(x => x.Value)
            .Select(a => new
            {
                a.ChatIds,
                a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                      .FirstOrDefault()?.OutageHours,
                PreviousDayOutageHour_23 = a.OutageSchedules?.WhereIf(notificationTime.DayOfWeek == DayOfWeek.Sunday, o => o.NumberWeekDay == (int)DayOfWeek.Saturday)
                                                        .WhereIf(notificationTime.DayOfWeek != DayOfWeek.Sunday, o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                        .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
            })
                //validate previous day and hour_23
                .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour < notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOff)
                            && x.PreviousDayOutageHour_23.Hour == prevHour && x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOn)
                .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour < notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOff)
                            && x.OutageHours.Any(sh => sh.Hour == prevHour && sh.Status == Domain.Enums.OutageStatus.PowerOn))
            .Select(r => new { r.ChatIds })
            .SelectMany(rr => rr.ChatIds)
            .ToHashSet();

        //remove already notified
        needNotify.ExceptWith(powerOffNotificationFlags);
        if (needNotify.Count != 0)
        {
            foreach (var chatId in needNotify)
            {
                await _botClientSender.SendMessageAsync(chatId, notificationMessage);
            }
            //add new notifed
            powerOffNotificationFlags.UnionWith(needNotify);
            //clean
            nextNotificationFlags.ExceptWith(needNotify);
        }
    }

    private async Task NotifyPossiblePowerOnV2Async(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour, HashSet<long> powerPossibleOnNotificationFlags, HashSet<long> nextNotificationFlags, string notificationMessage)
    {
        var needNotify = sessions.Select(x => x.Value)
            .Select(a => new
            {
                a.ChatIds,
                a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                      .FirstOrDefault()?.OutageHours,
                PreviousDayOutageHour_23 = a.OutageSchedules?.WhereIf(notificationTime.DayOfWeek == DayOfWeek.Sunday, o => o.NumberWeekDay == (int)DayOfWeek.Saturday)
                                                      .WhereIf(notificationTime.DayOfWeek != DayOfWeek.Sunday, o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                      .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
            })
                //validate previous day and hour_23
                .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour < notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerPossibleOn)
                            && x.PreviousDayOutageHour_23.Hour == prevHour && x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOff)
                .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour < notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerPossibleOn)
                            && x.OutageHours.Any(sh => sh.Hour == prevHour && sh.Status == Domain.Enums.OutageStatus.PowerOff))
            .Select(r => new { r.ChatIds })
            .SelectMany(rr => rr.ChatIds)
            .ToHashSet();

        //remove already notified
        needNotify.ExceptWith(powerPossibleOnNotificationFlags);
        if (needNotify.Count != 0)
        {
            foreach (var chatId in needNotify)
            {
                await _botClientSender.SendMessageAsync(chatId, notificationMessage);
            }
            //add new notifed
            powerPossibleOnNotificationFlags.UnionWith(needNotify);
            //clean
            nextNotificationFlags.ExceptWith(needNotify);
        }
    }

    private async Task NotifyPowerOnV2Async(ConcurrentDictionary<string, CachedNotificationList> sessions, DateTime notificationTime, DateTime realTime, int notificationHour, int prevHour, HashSet<long> powerOnNotificationFlags, HashSet<long> nextNotificationFlags, string notificationMessage)
    {
        var needNotify = sessions.Select(x => x.Value)
            .Select(a => new
            {
                a.ChatIds,
                a.OutageSchedules?.Where(o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek))
                                                      .FirstOrDefault()?.OutageHours,
                PreviousDayOutageHour_23 = a.OutageSchedules?.WhereIf(notificationTime.DayOfWeek == DayOfWeek.Sunday, o => o.NumberWeekDay == (int)DayOfWeek.Saturday)
                                                      .WhereIf(notificationTime.DayOfWeek != DayOfWeek.Sunday, o => o.NumberWeekDay == ((int)notificationTime.DayOfWeek - 1))
                                                      .FirstOrDefault()?.OutageHours.Where(x => x.Hour == TWENTY_TRHEE_HOUR).FirstOrDefault()
            })
            //validate previous day and hour_23
            .WhereIf(notificationHour == ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour < notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOn)
                        && x.PreviousDayOutageHour_23.Hour == prevHour && (x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerOff || x.PreviousDayOutageHour_23.Status == Domain.Enums.OutageStatus.PowerPossibleOn))
            .WhereIf(notificationHour != ZERO_HOUR, x => x.OutageHours.Any(oh => realTime.Hour < notificationHour && oh.Hour == notificationHour && oh.Status == Domain.Enums.OutageStatus.PowerOn)
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
                await _botClientSender.SendMessageAsync(chatId, notificationMessage);
            }
            //add new notifed
            powerOnNotificationFlags.UnionWith(needNotify);
            //clean
            nextNotificationFlags.ExceptWith(needNotify);
        }
    }
}
