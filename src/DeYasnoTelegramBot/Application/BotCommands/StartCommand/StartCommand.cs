using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Domain.Enums;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DeYasnoTelegramBot.Application.BotCommands.StartCommand;

public class StartCommand : BaseCommand
{
    public int? MessageId { get; set; }
}

public class StartCommandHandler : IRequestHandler<StartCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly TelegramBotClientSender _botClient;
    private readonly OutageScheduleStorage _outageScheduleStorage;

    public StartCommandHandler(ApplicationDbContext applicationDbContext,
        TelegramBotClientSender botClient,
        OutageScheduleStorage outageScheduleStorage)
    {
        _context = applicationDbContext;
        _botClient = botClient;
        _outageScheduleStorage = outageScheduleStorage;
    }

    public async Task Handle(StartCommand request, CancellationToken cancellationToken)
    {
        //add user to DB
        var chatId = request.ChatId;
        var isExist = await _context.Subscribers.AnyAsync(x => x.ChatId == chatId);

        if (!isExist)
        {
            await _context.Subscribers.AddAsync(new Domain.Entities.Subscriber
            {
                ChatId = chatId,
                InputStep = OutageInputStep.Step_1,
                BrowserSessionId = default,
                UserStreet = default,
                UserCity = default,
                UserHouseNumber = default,
                UserRegion = default,
            });
            await _context.SaveChangesAsync();
        }
        else
        {
            var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == chatId);

            sub.ChatId = chatId;
            sub.InputStep = OutageInputStep.Step_1;
            sub.BrowserSessionId = default;
            sub.UserStreet = default;
            sub.UserCity = default;
            sub.UserHouseNumber = default;
            sub.UserRegion = default;
            sub.OutageSchedules = default;
            _outageScheduleStorage.TryRemove(sub);

            _context.Subscribers.Update(sub);
            await _context.SaveChangesAsync();
        }

        var inlineMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
        {
            InlineKeyboardButton.WithCallbackData(NotificationMessages.CommandMessages.StartCommand.Kiev, Common.Constants.BotCommands.CallbackCommands.SelectedKiev),
            InlineKeyboardButton.WithCallbackData(NotificationMessages.CommandMessages.StartCommand.Dnipro, Common.Constants.BotCommands.CallbackCommands.SelectedDnipro),
        });

        await _botClient.SendMessageAsync(chatId, NotificationMessages.CommandMessages.StartCommand.CommandText,
            replyMarkup: inlineMarkup);

        return;
    }
}
