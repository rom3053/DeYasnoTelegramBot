using MediatR;

namespace DeYasnoTelegramBot.Application.BotCommands.Base;

public class BaseCommand : IRequest
{
    public long ChatId { get; set; }

    public string UserInput { get; set; }
}
