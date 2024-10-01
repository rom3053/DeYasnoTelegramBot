using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Application.Common.Exceptions;
using DeYasnoTelegramBot.Infrastructure.Services;
using MediatR;

namespace DeYasnoTelegramBot.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IBaseRequest
{
    private readonly ILogger<TRequest> _logger;
    private readonly TelegramBotClientSender _botClient;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger,
        TelegramBotClientSender botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (AutoInputException ex)
        {
            if (request is BaseCommand)
            {
                var baseCommand = (BaseCommand)(IRequest)request;
                await _botClient.SendMessageAsync(baseCommand.ChatId, ex.TelegramErrorMessage);
            }
            throw;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
