namespace DeYasnoTelegramBot.Application.Common.Helpers;

public static class DateTimeHelper
{
    const string WINDOWS_TIME_ZONE_ID = "FLE Standard Time";
    const string LINUX_macOS_TIME_ZONE_ID = "Europe/Kyiv";

    public static DateTime GetUkraineTimeNow()
    {
        TimeZoneInfo ukraineTimeZone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? WINDOWS_TIME_ZONE_ID : LINUX_macOS_TIME_ZONE_ID);

        var ss = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ukraineTimeZone);
        return ss;
    }
}
