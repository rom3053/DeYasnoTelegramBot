using Cronos;

namespace DeYasnoTelegramBot.Application.Common.Helpers;

public static class CronHelper
{
    public static bool IsTimeToExecute(string cronExpression)
    {
        var timeZoneId = string.Empty;
        if (OperatingSystem.IsWindows())
        {
            timeZoneId = "FLE Standard Time";  // Windows time zone ID
        }
        else
        {
            timeZoneId = "Europe/Kyiv";  // IANA time zone ID for Linux/macOS
        }

        CronExpression expression = CronExpression.Parse(cronExpression);
        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        DateTime now = DateTime.UtcNow;
        DateTime? nextOccurrence = expression.GetNextOccurrence(now.AddMinutes(-1), timeZone);

        // Check if the next occurrence is the current minute
        return nextOccurrence.Value.Minute == now.Minute && nextOccurrence.Value.Hour == now.Hour;
    }
}
