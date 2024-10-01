using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Domain.Entities;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeYasnoTelegramBot.Infrastructure.Services;

public class CachedNotificationList
{
    public HashSet<long> ChatIds { get; set; } = new HashSet<long>();

    public List<OutageScheduleDayDto>? OutageSchedules { get; set; }
}

public class OutageScheduleStorage
{
    public ConcurrentDictionary<string, CachedNotificationList> NotificationList = new ConcurrentDictionary<string, CachedNotificationList>();

    public OutageScheduleStorage()
    {
    }

    public static async Task InitCache(ApplicationDbContext context, OutageScheduleStorage outageScheduleStorage)
    {
        var subs = await context.Subscribers.Where(x => !string.IsNullOrEmpty(x.UserRegion) && !string.IsNullOrEmpty(x.UserCity)
                     && !string.IsNullOrEmpty(x.UserStreet) && !string.IsNullOrEmpty(x.UserHouseNumber)
                     && x.OutageSchedules != null)
                 .ToListAsync();

        foreach (var sub in subs)
        {
            outageScheduleStorage.TryAdd(sub);
        }
    }

    public bool TryAdd(Subscriber subscriber)
    {
        var newKey = GetKeyHashSchedule(subscriber);

        var isCached = NotificationList.TryGetValue(newKey, out var cachedValue);
        if (isCached)
        {
            cachedValue.ChatIds.Add(subscriber.ChatId);
            NotificationList[newKey] = cachedValue;
        }
        else
        {
            NotificationList.TryAdd(newKey, new CachedNotificationList
            {
                ChatIds = new HashSet<long> { subscriber.ChatId },
                OutageSchedules = subscriber.OutageSchedules,
            });
        }

        return true;
    }

    public bool TryRemove(Subscriber subscriber)
    {
        var key = GetKeyHashSchedule(subscriber);
        var isNotification = NotificationList.TryGetValue(key, out var notification);

        if (isNotification && notification != null)
        {
            notification.ChatIds.Remove(subscriber.ChatId);
            return true;
        }

        return false;
    }

    public int RemoveEmptySchedules()
    {
        var keysEmptySchedules = NotificationList.Where(x => x.Value.ChatIds.Any())
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keysEmptySchedules)
        {
            NotificationList.Remove(key, out var value);
        }

        return keysEmptySchedules.Count;
    }

    public string GetKeyHashSchedule(Subscriber subscriber)
    {
        // Serialize the object to a JSON string
        var jsonString = JsonConvert.SerializeObject(subscriber.OutageSchedules);

        // Compute the SHA256 hash
        byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
        byte[] hashBytes = SHA256.HashData(bytes);

        // Convert the byte array to a hexadecimal string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }

        var hashOfOutageSchedule = sb.ToString();

        var newKey = $"{subscriber.UserRegion}_{subscriber.UserCity}_{hashOfOutageSchedule}";

        return newKey;
    }
}

