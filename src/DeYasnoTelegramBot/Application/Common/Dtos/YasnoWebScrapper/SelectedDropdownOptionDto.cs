using DeYasnoTelegramBot.Domain.Enums;

namespace DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;

public class SelectedDropdownOptionDto : DropdownOptionDto
{
    public SelectedOutageInputType SelectedOutageInputType { get; set; }
}
