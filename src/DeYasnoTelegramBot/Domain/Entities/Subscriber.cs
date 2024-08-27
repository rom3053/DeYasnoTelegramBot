using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Domain.Enums;

namespace DeYasnoTelegramBot.Domain.Entities;

public sealed class Subscriber
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long ChatId { get; set; }

    public string? BrowserSessionId { get; set; }

    public OutageInputStep InputStep { get; set; }

    public string? UserRegion { get; set; }

    public string? UserCity { get; set; }

    public string? UserStreet { get; set; }

    public string? UserHouseNumber { get; set; }

    public List<OutageScheduleDayDto>? OutageSchedules { get; set; }
}
