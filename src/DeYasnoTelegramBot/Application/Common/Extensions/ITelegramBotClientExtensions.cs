using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DeYasnoTelegramBot.Application.Common.Extensions;

public static class ITelegramBotClientExtensions
{
    public static async Task SendMessage(this ITelegramBotClient _botClient, long chatId, string messageText)
    {
        var message = await _botClient.SendTextMessageAsync(chatId,
            text: messageText,
            parseMode: ParseMode.Html);
    }

    public static async Task SendMessage(this ITelegramBotClient _botClient, long chatId, string text, string options = null)
    {
        try
        {
            if (options is not null)
            {
                text = $"{text}\n{options}";
            }

            var message = await _botClient.SendTextMessageAsync(chatId, text,
                parseMode: ParseMode.Html,
                protectContent: false);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
