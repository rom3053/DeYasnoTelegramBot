using DeYasnoTelegramBot.Application.BotCommands.Base;
using DeYasnoTelegramBot.Domain.Enums;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Application.BotCommands.UserInputCommand;

public class UserInputCommand : BaseCommand
{
}

public class UserInputCommandHandler : IRequestHandler<UserInputCommand>
{
    private readonly ApplicationDbContext _context;
    private readonly OutageInputService _outageInputService;
    private readonly OutageScheduleStorage _outageScheduleStorage;

    public UserInputCommandHandler(ApplicationDbContext context,
        OutageInputService outageInputService,
        OutageScheduleStorage outageScheduleStorage)
    {
        _context = context;
        _outageInputService = outageInputService;
        _outageScheduleStorage = outageScheduleStorage;
    }

    public async Task Handle(UserInputCommand request, CancellationToken cancellationToken)
    {
        var sub = await _context.Subscribers.FirstOrDefaultAsync(x => x.ChatId == request.ChatId);

        if (sub is not null)
        {
            var step = sub.InputStep;
            //ToDo check session before
            if (step == OutageInputStep.Step_0)
            {
                return;
            }

            var result = step switch
            {
                OutageInputStep.Step_1 => await _outageInputService.InputRegion(request.ChatId, request.UserInput),
                OutageInputStep.Step_2 => await _outageInputService.InputCity(sub, request.UserInput),
                OutageInputStep.Step_3 => await _outageInputService.SelectDropdownOption(request.ChatId, sub.BrowserSessionId, step, request.UserInput),
                OutageInputStep.Step_4 => await _outageInputService.InputStreet(sub, request.UserInput),
                OutageInputStep.Step_5 => await _outageInputService.SelectDropdownOption(request.ChatId, sub.BrowserSessionId, step, request.UserInput),
                OutageInputStep.Step_6 => await _outageInputService.InputHouseNumber(request.ChatId, sub.BrowserSessionId, request.UserInput),
                OutageInputStep.Step_7 => await _outageInputService.SelectDropdownOption(request.ChatId, sub.BrowserSessionId, step, request.UserInput),
                _ => null,
            };

            if (result is not null)
            {
                sub.InputStep = result.InputStep;
                sub.OutageSchedules = result.OutageSchedules is null ? sub.OutageSchedules : result.OutageSchedules;
                sub.UserRegion = string.IsNullOrEmpty(result.UserRegion) ? sub.UserRegion : result.UserRegion;
                sub.UserCity = string.IsNullOrEmpty(result.UserCity) ? sub.UserCity : result.UserCity;
                sub.UserStreet = string.IsNullOrEmpty(result.UserStreet) ? sub.UserStreet : result.UserStreet;
                sub.UserHouseNumber = string.IsNullOrEmpty(result.UserHouse) ? sub.UserHouseNumber : result.UserHouse;
                sub.BrowserSessionId = string.IsNullOrEmpty(result.BrowserSessionId) ? sub.BrowserSessionId : result.BrowserSessionId;

                if (result.InputStep == OutageInputStep.Step_0)
                {
                    _outageScheduleStorage.TryAdd(sub);
                }

                _context.Subscribers.Update(sub);
                await _context.SaveChangesAsync();
            }
        }
    }
}