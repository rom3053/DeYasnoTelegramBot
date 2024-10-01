using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DeYasnoTelegramBot.Application.BotCommands.UpdateOwnScheduleCommand;

public class UpdateOwnScheduleCommand : BaseCommand
{

}

public class UpdateOwnScheduleCommandHanlder : IRequestHandler<UpdateOwnScheduleCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly TelegramBotClientSender _botClient;
    private readonly OutageInputService _outageInputService;
    private readonly OutageScheduleStorage _outageScheduleStorage;

    public UpdateOwnScheduleCommandHanlder(ApplicationDbContext applicationDbContext,
        TelegramBotClientSender botClient,
        OutageInputService outageInputService,
        OutageScheduleStorage outageScheduleStorage)
    {
        _context = applicationDbContext;
        _botClient = botClient;
        _outageInputService = outageInputService;
        _outageScheduleStorage = outageScheduleStorage;
    }

    public async Task Handle(UpdateOwnScheduleCommand request, CancellationToken cancellationToken)
    {
        var subInfo = await _context.Subscribers.Where(x => !string.IsNullOrEmpty(x.UserRegion) &&
            !string.IsNullOrEmpty(x.UserCity) && !string.IsNullOrEmpty(x.UserStreet) &&
            !string.IsNullOrEmpty(x.UserHouseNumber))
            .Select(x => new
            {
                x.ChatId,
                x.UserRegion,
                x.UserCity,
                x.UserStreet,
                x.UserHouseNumber,
            })
            .FirstOrDefaultAsync(x => x.ChatId == request.ChatId);

        if (subInfo != null)
        {
            var session = await _outageInputService.InitNewSessionAutoInput(subInfo.UserRegion, subInfo.UserCity, subInfo.UserStreet, subInfo.UserHouseNumber);
            var outageScheduleDto = await _outageInputService.GetParsedTable(session.SessionId);

            var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == request.ChatId);

            _outageScheduleStorage.TryRemove(sub);

            //update and add a new schedule
            sub.OutageSchedules = outageScheduleDto;
            _outageScheduleStorage.TryAdd(sub);
            _context.Subscribers.Update(sub);
            await _context.SaveChangesAsync();

            await _botClient.SendMessageAsync(sub.ChatId,
                text: NotificationMessages.CommandMessages.UpdateOwnScheduleCommand.CommandSuccessText);

            return;
        }
    }
}
