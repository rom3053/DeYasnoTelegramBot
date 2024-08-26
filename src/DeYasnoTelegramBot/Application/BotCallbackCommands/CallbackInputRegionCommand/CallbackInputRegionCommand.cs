using MediatR;
using DeYasnoTelegramBot.Application.BotCallbackCommands.Base;
using Microsoft.EntityFrameworkCore;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using DeYasnoTelegramBot.Domain.Enums;

namespace DeYasnoTelegramBot.Application.BotCallbackCommands.CallbackInputRegionCommand;

/// <summary>
/// Save and Set region data and set next step Step_2
/// </summary>
public class CallbackInputRegionCommand : BaseCallbackCommand
{
}

public class CallbackInputRegionCommandHandler : IRequestHandler<CallbackInputRegionCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly OutageInputService _outageInputService;
    private readonly OutageScheduleStorage _outageScheduleStorage;

    public CallbackInputRegionCommandHandler(ApplicationDbContext context,
        OutageInputService outageInputService,
        OutageScheduleStorage outageScheduleStorage)
    {
        _context = context;
        _outageInputService = outageInputService;
        _outageScheduleStorage = outageScheduleStorage;
    }

    public async Task Handle(CallbackInputRegionCommand request, CancellationToken cancellationToken)
    {
        //var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == request.ChatId);

        //if (sub is not null)
        //{
        //    var step = sub.InputStep;

        //    if (step == OutageInputStep.Step_0)
        //    {
        //        return;
        //    }

        //    var result = await _outageInputService.InputRegion(request.ChatId, step, request.UserInput);

        //    if (result is not null)
        //    {
        //        //next step
        //        sub.InputStep = OutageInputStep.Step_2;
        //        sub.UserRegion = string.IsNullOrEmpty(result.UserRegion) ? sub.UserRegion : result.UserRegion;

        //        _context.Subscribers.Update(sub);
        //        await _context.SaveChangesAsync();
        //    }
        //}
    }
}