using DeYasnoTelegramBot.Domain.Enums;

namespace DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;

public class OutageHourDto
{
    public int Hour { get; set; }

    public OutageStatus Status { get; set; }
}
