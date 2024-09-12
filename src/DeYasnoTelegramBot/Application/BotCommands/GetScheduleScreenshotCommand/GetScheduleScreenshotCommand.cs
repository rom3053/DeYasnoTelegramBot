using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using DeYasnoTelegramBot.Application.Common.Helpers;

namespace DeYasnoTelegramBot.Application.BotCommands.GetScheduleScreenshotCommand;

public class GetScheduleScreenshotCommand : BaseCommand, IRequest<string>
{
}

public class GetScheduleScreenshotCommandHandler : IRequestHandler<GetScheduleScreenshotCommand>
{
    private readonly OutageInputService _outageInputService;
    private readonly ApplicationDbContext _context;
    private readonly ITelegramBotClient _botClient;

    public GetScheduleScreenshotCommandHandler(OutageInputService outageInputService,
        ApplicationDbContext context,
        ITelegramBotClient botClient)
    {
        _outageInputService = outageInputService;
        _context = context;
        _botClient = botClient;
    }

    public async Task Handle(GetScheduleScreenshotCommand request, CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(x => x.ChatId == request.ChatId, cancellationToken);

        if (subInfo is not null)
        {
            var session = await _outageInputService.InitNewSessionAutoInput(subInfo.UserRegion, subInfo.UserCity, subInfo.UserStreet, subInfo.UserHouseNumber);
            var fileDto = await _outageInputService.GetScheduleOutageScreenshotAsync(session.SessionId, subInfo.UserCity);

            if (fileDto != null)
            {
                using var ms = new MemoryStream(fileDto.Bytes);

                var message = await _botClient.SendPhotoAsync(subInfo.ChatId,
                    photo: InputFile.FromStream(ms, fileName: fileDto.Name),
                    caption: NotificationMessages.CommandMessages.GetScheduleScreenshotCommand,
                    parseMode: ParseMode.Html,
                    protectContent: false);
            }
        }
    }
}