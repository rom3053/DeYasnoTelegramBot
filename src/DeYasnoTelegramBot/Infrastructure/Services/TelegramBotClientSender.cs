using DeYasnoTelegramBot.Application.Common.Dtos;
using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DeYasnoTelegramBot.Infrastructure.Services;

public class TelegramBotClientSender
{
    private readonly ITelegramBotClient _botClient;
    private readonly ApplicationDbContext _context;
    private readonly OutageScheduleStorage _outageScheduleStorage;
    private readonly ILogger<TelegramBotClientSender> _logger;

    public TelegramBotClientSender(ITelegramBotClient botClient,
        ApplicationDbContext context,
        ILogger<TelegramBotClientSender> logger,
        OutageScheduleStorage outageScheduleStorage)
    {
        _botClient = botClient;
        _context = context;
        _logger = logger;
        _outageScheduleStorage = outageScheduleStorage;
    }

    public async Task SendMessageAsync(long chatId, string text, string options = null, IReplyMarkup? replyMarkup = null)
    {
        try
        {
            if (options is not null)
            {
                text = $"{text}\n{options}";
            }

            var message = await _botClient.SendTextMessageAsync(chatId, text,
                parseMode: ParseMode.Html,
                protectContent: false,
                replyMarkup: replyMarkup);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("was blocked"))
            {
                var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == chatId);
                if (sub != null) 
                {
                    _outageScheduleStorage.TryRemove(sub);
                    _context.Subscribers.Remove(sub);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }

    public async Task SendPhotoAsync(long chatId, string caption, string fileName, MemoryStream? bytes)
    {
        try
        {
            var message = await _botClient.SendPhotoAsync(chatId,
                photo: InputFile.FromStream(bytes, fileName: fileName),
                caption: caption,
                parseMode: ParseMode.Html,
                protectContent: false);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("was blocked"))
            {
                var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == chatId);
                if (sub != null)
                {
                    _outageScheduleStorage.TryRemove(sub);
                    _context.Subscribers.Remove(sub);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
