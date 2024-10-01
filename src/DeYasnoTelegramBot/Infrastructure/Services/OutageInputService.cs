using System.Text;
using DeYasnoTelegramBot.Application.Common.Dtos;
using DeYasnoTelegramBot.Application.Common.Dtos.Subscriber;
using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Application.Common.Exceptions;
using DeYasnoTelegramBot.Application.Common.Extensions;
using DeYasnoTelegramBot.Application.Common.Helpers;
using DeYasnoTelegramBot.Domain.Entities;
using DeYasnoTelegramBot.Domain.Enums;
using DeYasnoTelegramBot.Infrastructure.HttpClients;
using Telegram.Bot;

namespace DeYasnoTelegramBot.Infrastructure.Services;

public class OutageInputService
{
    private const string Message_About_Step_0 = NotificationMessages.Message_About_Step_0;
    private const string Message_About_Step_2 = NotificationMessages.Message_About_Step_2;
    private const string Message_About_Step_3 = NotificationMessages.Message_About_Step_3;
    private const string Message_About_Step_4 = NotificationMessages.Message_About_Step_4;
    private const string Message_About_Step_5 = NotificationMessages.Message_About_Step_5;
    private const string Message_About_Step_6 = NotificationMessages.Message_About_Step_6;
    private const string Message_About_Step_7 = NotificationMessages.Message_About_Step_7;
    private const string Message_About_Incorrect_Input = NotificationMessages.Message_About_Incorrect_Input;

    private readonly YasnoWebScrapperHttpClient _yasnoWebScrapperHttpClient;
    private readonly TelegramBotClientSender _botClient;

    public OutageInputService(YasnoWebScrapperHttpClient yasnoWebScrapperHttpClient,
        TelegramBotClientSender botClient)
    {
        _yasnoWebScrapperHttpClient = yasnoWebScrapperHttpClient;
        _botClient = botClient;
    }

    public async Task<SubscriberInputUpdateDto> InputRegion(long chatId, string userInput)
    {
        var session = await _yasnoWebScrapperHttpClient.InitSessionAsync();
        await _yasnoWebScrapperHttpClient.InputRegion(session.SessionId, userInput);

        await SendMessage(chatId, Message_About_Step_2);

        return new SubscriberInputUpdateDto
        {
            BrowserSessionId = session.SessionId,
            UserRegion = userInput,
            InputStep = OutageInputStep.Step_2,
        };
    }

    public async Task<SubscriberInputUpdateDto> InputCity(Subscriber sub, string userInput)
    {
        var options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputCity(sub.BrowserSessionId, userInput);
        string htmlList = ConvertToHtmlList(options);

        await SendMessage(sub.ChatId, Message_About_Step_3, htmlList);

        return new SubscriberInputUpdateDto
        {
            //next step
            InputStep = OutageInputStep.Step_3,
        };
    }

    public async Task<SubscriberInputUpdateDto> InputStreet(Subscriber sub, string userInput)
    {
        var options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputStreet(sub.BrowserSessionId, userInput);
        string htmlList = ConvertToHtmlList(options);

        await SendMessage(sub.ChatId, Message_About_Step_5, htmlList);

        return new SubscriberInputUpdateDto
        {
            //next step
            InputStep = OutageInputStep.Step_5,
        };
    }

    public async Task<SubscriberInputUpdateDto> InputHouseNumber(long chatId, string browserSessionId, string userInput)
    {
        var options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputHouseNumber(browserSessionId, userInput);
        string htmlList = ConvertToHtmlList(options);

        await SendMessage(chatId, Message_About_Step_7, htmlList);

        return new SubscriberInputUpdateDto
        {
            InputStep = OutageInputStep.Step_7,
        };
    }

    public async Task<SubscriberInputUpdateDto> SelectDropdownOption(long chatId, string browserSessionId, OutageInputStep inputStep, string userInput)
    {
        var isIndexInput = int.TryParse(userInput, out var index);
        if (!isIndexInput)
        {
            await SendMessage(chatId, Message_About_Incorrect_Input);
            return null;
        }

        var response = await _yasnoWebScrapperHttpClient.SelectDropdownOption(browserSessionId, (index - 1).ToString());

        //finnal step send notification and skip incrementing
        if (inputStep == OutageInputStep.Step_7)
        {
            await SendMessage(chatId, Message_About_Step_0);
        }
        else
        {
            inputStep++;
        }

        var stepTask = inputStep switch
        {
            OutageInputStep.Step_0 => Task.CompletedTask,
            OutageInputStep.Step_4 => SendMessage(chatId, Message_About_Step_4),
            OutageInputStep.Step_6 => SendMessage(chatId, Message_About_Step_6),

            _ => Task.CompletedTask
        };
        await stepTask;

        return new SubscriberInputUpdateDto
        {
            OutageSchedules = inputStep == OutageInputStep.Step_7 ? await GetParsedTable(browserSessionId) : default,
            UserRegion = response.SelectedOutageInputType == SelectedOutageInputType.SelectedRegion ? response.Text : default,
            UserCity = response.SelectedOutageInputType == SelectedOutageInputType.SelectedCity ? response.Text : default,
            UserStreet = response.SelectedOutageInputType == SelectedOutageInputType.SelectedStreet ? response.Text : default,
            UserHouse = response.SelectedOutageInputType == SelectedOutageInputType.SelectedHouseNumber ? response.Text : default,
            InputStep = inputStep == OutageInputStep.Step_7 ? OutageInputStep.Step_0 : inputStep,
        };
    }

    public async Task<FileDto> GetScheduleOutageScreenshotAsync(string browserSessionId, string cityName)
    {
        var scheduleDto = await _yasnoWebScrapperHttpClient.GetScheduleScreenshot(browserSessionId, cityName);

        return scheduleDto;
    }

    public async Task<List<OutageScheduleDayDto>> GetParsedTable(string browserSessionId)
    {
        var scheduleTableDto = await _yasnoWebScrapperHttpClient.GetParsedTable(browserSessionId);

        return scheduleTableDto;
    }

    public async Task<SessionDto> InitNewSessionAutoInput(string userRegion, string userCity, string userStreet, string userHouseNumber)
    {
        var session = await _yasnoWebScrapperHttpClient.InitSessionAsync();
        await _yasnoWebScrapperHttpClient.InputRegion(session.SessionId, userRegion);

        var options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputCity(session.SessionId, userCity);
        var optionIndex = options.Where(x => x.Text.Contains(userCity)).Select(x => x.Index).FirstOrDefault(-1);
        ValidateAutoInput(optionIndex);
        var response = await _yasnoWebScrapperHttpClient.SelectDropdownOption(session.SessionId, optionIndex.ToString());

        options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputStreet(session.SessionId, userStreet);
        optionIndex = options.Where(x => x.Text.Contains(userStreet)).Select(x => x.Index).FirstOrDefault(-1);
        ValidateAutoInput(optionIndex);
        response = await _yasnoWebScrapperHttpClient.SelectDropdownOption(session.SessionId, optionIndex.ToString());

        options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputHouseNumber(session.SessionId, userHouseNumber);
        optionIndex = options.Where(x => x.Text.Contains(userHouseNumber)).Select(x => x.Index).FirstOrDefault(-1);
        ValidateAutoInput(optionIndex);
        response = await _yasnoWebScrapperHttpClient.SelectDropdownOption(session.SessionId, optionIndex.ToString());

        return session;
    }

    private async Task SendMessage(long chatId, string text, string options = null)
    {
       await _botClient.SendMessageAsync(chatId, text, options);
    }

    private static string ConvertToHtmlList(List<DropdownOptionDto> options)
    {
        StringBuilder htmlBuilder = new StringBuilder();

        foreach (var option in options)
        {
            htmlBuilder.AppendLine($"<b>{option.Index + 1}. {option.Text}</b>");
        }

        return htmlBuilder.ToString();
    }

    private static void ValidateAutoInput(int index)
    {
        if (index == -1)
        {
            throw new AutoInputException();
        }
    }
}
