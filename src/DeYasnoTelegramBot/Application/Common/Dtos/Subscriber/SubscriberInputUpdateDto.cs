using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Domain.Enums;

namespace DeYasnoTelegramBot.Application.Common.Dtos.Subscriber;

public record SubscriberInputUpdateDto
{
    public string? BrowserSessionId { get; set; }

    public OutageInputStep InputStep { get; set; }

    public string? UserRegion { get; set; }

    public string? UserCity { get; set; }

    public string? UserStreet { get; set; }

    public string? UserHouse { get; set; }

    public List<OutageScheduleDayDto>? OutageSchedules { get; set; }
}
