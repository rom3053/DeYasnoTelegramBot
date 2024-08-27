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
        var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == request.ChatId);
        //ToDO validation aboit all data of user
        if (sub is not null)
        {
            //TODO need to rework for init all scrapper steps and return or save into DB and return bytes
            var fileDto = await _outageInputService.GetScheduleOutageScreenshotAsync(sub.BrowserSessionId);

            if (fileDto != null)
            {
                using var ms = new MemoryStream(fileDto.Bytes);

                var message = await _botClient.SendPhotoAsync(sub.ChatId,
                    photo: InputFile.FromStream(ms, fileName: fileDto.Name),
                    caption: NotificationMessages.CommandMessages.GetScheduleScreenshotCommand,
                    parseMode: ParseMode.Html,
                    protectContent: false);
            }
        }
    }
}