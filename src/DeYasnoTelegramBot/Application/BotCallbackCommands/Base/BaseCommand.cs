using MediatR;

namespace DeYasnoTelegramBot.Application.BotCallbackCommands.Base;

public class BaseCallbackCommand : IRequest
{
    public long ChatId { get; set; }

    public string UserInput { get; set; }
}
