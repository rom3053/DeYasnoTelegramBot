using DeYasnoTelegramBot.Application.BotCommands.GetScheduleScreenshotCommand;
using DeYasnoTelegramBot.Application.BotCommands.StartCommand;
using DeYasnoTelegramBot.Application.BotCommands.UserInputCommand;
using DeYasnoTelegramBot.Application.Common.Constants;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DeYasnoTelegramBot.Infrastructure.Telegram.Handlers;

public class UpdateHandler : IUpdateHandler
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateHandler> _logger;

    public UpdateHandler(IMediator mediator,
        ILogger<UpdateHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            Message? userMessage = update.Message;
            string? userMessageText = userMessage?.Text;
            long? chatId = userMessage?.Chat?.Id;
            int? messageId = update?.Message?.MessageId;

            if (update.Type == UpdateType.CallbackQuery)
            {
                var chatId2 = update.CallbackQuery.Message.Chat.Id;
                userMessageText = update.CallbackQuery.Data;
                await HandleCallbackMessage(chatId2, userMessageText, messageId);
                return;
            }

            if (userMessage is null)
                return;

            //ToDo validate user step of input info and selectors
            if (userMessageText is not null)
            {
                string handledText = userMessageText
                    .Replace("*", @"\*")
                    .Replace(",", @"\,")
                    .Replace(",", @"\,")
                    .Replace("~", @"\~")
                    .Replace("`", @"\`")
                    .Replace(">", @"\>")
                    .Replace("<", @"\<")
                    .Replace("#", @"\#")
                    .Replace("+", @"\+")
                    .Replace("=", @"\=")
                    .Replace("|", @"\|")
                    .Replace("{", @"\}")
                    .Replace("{", @"\{")
                    .Replace("!", @"\!")
                    .Replace(".", @"\.")
                    .Replace("-", @"\-");
                await HandleCommandMessage(chatId.Value, handledText, messageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private Task HandleCommandMessage(long chatId, string userMessage, int? messageId)
    {
        return userMessage switch
        {
            BotCommands.Start => _mediator.Send(new StartCommand { ChatId = chatId, MessageId = messageId }),
            BotCommands.GetScheduleScreenshot => _mediator.Send(new GetScheduleScreenshotCommand { ChatId = chatId }),
            _ when !string.IsNullOrWhiteSpace(userMessage) => _mediator.Send(new UserInputCommand { ChatId = chatId, UserInput = userMessage }),
            //BotCommands.HelpCommand => HandleErrorMessage(chatId),
            _ => Task.CompletedTask
        };
    }

    private async Task HandleCallbackMessage(long chatId, string userMessage, int? messageId)
    {
        var command = userMessage switch
        {
            BotCommands.CallbackCommands.SelectedKiev => _mediator.Send(new UserInputCommand { ChatId = chatId, UserInput = "Київ" }),
            BotCommands.CallbackCommands.SelectedDnipro => _mediator.Send(new UserInputCommand { ChatId = chatId, UserInput = "Дніпро" }),
            _ => Task.CompletedTask
        };
        await command;
    }
}
