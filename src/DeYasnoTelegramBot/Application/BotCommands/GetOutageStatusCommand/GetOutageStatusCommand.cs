using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace DeYasnoTelegramBot.Application.BotCommands.GetOutageStatusCommand;

public class GetOutageStatusCommand : BaseCommand, IRequest<string>
{
}

public class GetOutageStatusCommandHandler : IRequestHandler<GetOutageStatusCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly ITelegramBotClient _botClient;

    public GetOutageStatusCommandHandler(ApplicationDbContext context,
        ITelegramBotClient botClient)
    {
        _context = context;
        _botClient = botClient;
    }

    public async Task Handle(GetOutageStatusCommand request, CancellationToken cancellationToken)
    {
        var dateTimeNow = DateTimeHelper.GetUkraineTimeNow();

        var schedule = await _context.Subscribers.Where(x => x.ChatId == request.ChatId)
            .Select(o => o.OutageSchedules)
            .FirstOrDefaultAsync(cancellationToken);

        var currentOutageHour = schedule.Where(s => s.NumberWeekDay == (int)dateTimeNow.DayOfWeek)
            .SelectMany(d => d.OutageHours)
            .Where(dd => dd.Hour == dateTimeNow.Hour)
            .FirstOrDefault();

        if (currentOutageHour != null)
        {
            var statusText = currentOutageHour.Status switch
            {
                Domain.Enums.OutageStatus.PowerOn => NotificationMessages.CommandMessages.GetOutageStatusCommand.PowerOn,
                Domain.Enums.OutageStatus.PowerOff => NotificationMessages.CommandMessages.GetOutageStatusCommand.PowerOff,
                Domain.Enums.OutageStatus.PowerPossibleOn => NotificationMessages.CommandMessages.GetOutageStatusCommand.PowerPossibleOn,
            };

            var message = await _botClient.SendTextMessageAsync(request.ChatId, statusText,
                parseMode: ParseMode.Html,
                protectContent: false);
        }

        return;
    }
}
