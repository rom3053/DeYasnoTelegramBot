using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Application.BotCommands.DisableNotification;

public class ToggleNotificationCommand : BaseCommand, IRequest<string>
{
}

public class ToggleNotificationCommandHandler : IRequestHandler<ToggleNotificationCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly TelegramBotClientSender _botClient;

    public ToggleNotificationCommandHandler(ApplicationDbContext context,
        TelegramBotClientSender botClient)
    {
        _context = context;
        _botClient = botClient;
    }

    public async Task Handle(ToggleNotificationCommand request, CancellationToken cancellationToken)
    {
        var isDisableNotification = await _context.Subscribers.Where(x => x.ChatId == request.ChatId)
            .Select(o => o.IsDisableNotification)
            .FirstOrDefaultAsync(cancellationToken);

        var statusText = string.Empty;
        if (isDisableNotification)
        {
            await _context.Subscribers.Where(x => x.ChatId == request.ChatId)
                 .ExecuteUpdateAsync(x => x.SetProperty(d => d.IsDisableNotification, false));
            statusText = NotificationMessages.CommandMessages.ToggleNotificationCommand.EnableText;
        }
        else
        {
            await _context.Subscribers.Where(x => x.ChatId == request.ChatId)
                .ExecuteUpdateAsync(x => x.SetProperty(d => d.IsDisableNotification, true));
            statusText = NotificationMessages.CommandMessages.ToggleNotificationCommand.DisableText;
        }

        await _botClient.SendMessageAsync(request.ChatId, statusText);
    }
}