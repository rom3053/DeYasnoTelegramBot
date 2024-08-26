using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Domain.Entities;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeYasnoTelegramBot.Infrastructure.Services;

public class OutageScheduleStorage
{
    public ConcurrentDictionary<string, CachedNotificationList> NotificationList = new ConcurrentDictionary<string, CachedNotificationList>();

    public OutageScheduleStorage()
    {
    }

    public static async Task InitCache(ApplicationDbContext context, OutageScheduleStorage outageScheduleStorage)
    {
        var subs = await context.Subscribers.Where(x => !string.IsNullOrEmpty(x.UserRegion) && !string.IsNullOrEmpty(x.UserCity)
                     && !string.IsNullOrEmpty(x.UserStreet) && !string.IsNullOrEmpty(x.UserHouse)
                     && x.OutageSchedules != null)
                 .ToListAsync();

        foreach (var sub in subs)
        {
            outageScheduleStorage.TryAdd(sub);
        }
    }

    public bool TryAdd(Subscriber subscriber)
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
}

public class CachedNotificationList
{
    public HashSet<long> ChatIds { get; set; } = new HashSet<long>();

    public List<OutageScheduleDayDto>? OutageSchedules { get; set; }
}
