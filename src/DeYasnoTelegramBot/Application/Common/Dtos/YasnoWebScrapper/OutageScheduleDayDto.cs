namespace DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;

public class OutageScheduleDayDto
{
    public string DayTitle { get; set; }

    public int NumberWeekDay { get; set; }

    public List<OutageHourDto> OutageHours { get; set; }
}
