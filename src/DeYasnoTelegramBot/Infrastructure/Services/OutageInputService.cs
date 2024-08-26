using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using System.Text;
using DeYasnoTelegramBot.Domain.Entities;
using DeYasnoTelegramBot.Domain.Enums;
using DeYasnoTelegramBot.Infrastructure.HttpClients;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using DeYasnoTelegramBot.Application.Common.Dtos;
using DeYasnoTelegramBot.Application.Common.Dtos.Subscriber;

namespace DeYasnoTelegramBot.Infrastructure.Services;

public class OutageInputService
{
    private const string Message_About_Step_0 = "Тепер ви будете отримувати нотифікації про відключення світла.\nЗадопомогою команд:\n/schedule";
    private const string Message_About_Step_2 = "Зараз введіть назву Міста";
    private const string Message_About_Step_3 = "Відправте номер з вашим варіантом Міста";
    private const string Message_About_Step_4 = "Зараз введіть назву Вулиці";
    private const string Message_About_Step_5 = "Відправте номер з вашим варіантом Вулиці";
    private const string Message_About_Step_6 = "Зараз введіть номер будинку";
    private const string Message_About_Step_7 = "Відправте номер з вашим варіантом номеру будинку";

    private readonly YasnoWebScrapperHttpClient _yasnoWebScrapperHttpClient;
    private readonly ITelegramBotClient _botClient;

    public OutageInputService(YasnoWebScrapperHttpClient yasnoWebScrapperHttpClient,
        ITelegramBotClient botClient)
    {
        _yasnoWebScrapperHttpClient = yasnoWebScrapperHttpClient;
        _botClient = botClient;
    }

    //return dto about session and step and etc for update in command
    public async Task<SubscriberInputUpdateDto> InputRegion(long chatId, string userInput)
    {
        var session = await _yasnoWebScrapperHttpClient.InitSessionAsync();
        await _yasnoWebScrapperHttpClient.InputRegion(session.SessionId, userInput);

        //TODO: next telegram message about step 2
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
        //var session = await _yasnoWebScrapperHttpClient.InitSessionAsync();
        var options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputCity(sub.BrowserSessionId, userInput);
        string htmlList = ConvertToHtmlList(options);

        //TODO: next telegram message about step 3 and convert options to TG message
        await SendMessage(sub.ChatId, Message_About_Step_3, htmlList);

        return new SubscriberInputUpdateDto
        {
            //next step
            InputStep = OutageInputStep.Step_3,
        };
    }

    public async Task<SubscriberInputUpdateDto> InputStreet(Subscriber sub, string userInput)
    {
        //var session = await _yasnoWebScrapperHttpClient.InitSessionAsync();
        var options = await _yasnoWebScrapperHttpClient.GetOptionsAndInputStreet(sub.BrowserSessionId, userInput);
        string htmlList = ConvertToHtmlList(options);

        //TODO: next telegram message about step 3 and convert options to TG message
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

        //TODO: next telegram message about step 3 and convert options to TG message
        await SendMessage(chatId, Message_About_Step_7, htmlList);

        return new SubscriberInputUpdateDto
        {
            InputStep = OutageInputStep.Step_7,
        };
    }

    public async Task<SubscriberInputUpdateDto> SelectDropdownOption(long chatId, string browserSessionId, OutageInputStep inputStep, string userInput)
    {
        //rework to return selected Value
        //enum with input region,city and etc.
        var response = await _yasnoWebScrapperHttpClient.SelectDropdownOption(browserSessionId, userInput);

        //finnal step
        if (inputStep == OutageInputStep.Step_7)
        {
            var scheduleTableDto = await GetParsedTable(browserSessionId);
            await SendMessage(chatId, Message_About_Step_0);

            return new SubscriberInputUpdateDto
            {
                OutageSchedules = scheduleTableDto,
                InputStep = OutageInputStep.Step_0,
            };
        }
        else
        {
            inputStep++;
        }

        var nextStep = inputStep;

        var stepTask = nextStep switch
        {
            OutageInputStep.Step_0 => Task.CompletedTask,
            OutageInputStep.Step_4 => SendMessage(chatId, Message_About_Step_4),
            OutageInputStep.Step_6 => SendMessage(chatId, Message_About_Step_6),

            _ => Task.CompletedTask
        };
        await stepTask;

        return new SubscriberInputUpdateDto
        {
            UserRegion = response.SelectedOutageInputType == SelectedOutageInputType.SelectedRegion ? response.Text : default,
            UserCity = response.SelectedOutageInputType == SelectedOutageInputType.SelectedCity ? response.Text : default,
            UserStreet = response.SelectedOutageInputType == SelectedOutageInputType.SelectedStreet ? response.Text : default,
            UserHouse = response.SelectedOutageInputType == SelectedOutageInputType.SelectedHouseNumber ? response.Text : default,
            InputStep = nextStep,
        };
    }

    public async Task<FileDto> GetScheduleOutageScreenshotAsync(string browserSessionId)
    {
        var scheduleDto = await _yasnoWebScrapperHttpClient.GetScheduleScreenshot(browserSessionId);

        return scheduleDto;
    }

    public async Task<List<OutageScheduleDayDto>> GetParsedTable(string browserSessionId)
    {
        var scheduleTableDto = await _yasnoWebScrapperHttpClient.GetParsedTable(browserSessionId);

        return scheduleTableDto;
    }

    async Task SendMessage(long chatId, string text, string options = null)
    {
        try
        {
            if (options is not null)
            {
                text = $"{text}\n{options}";
            }

            var message = await _botClient.SendTextMessageAsync(chatId, text,
                parseMode: ParseMode.Html,
                protectContent: true);
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    static string ConvertToHtmlList(List<DropdownOptionDto> options)
    {
        StringBuilder htmlBuilder = new StringBuilder();

        foreach (var option in options)
        {
            htmlBuilder.AppendLine($"<b>{option.Index}. {option.Text}</b>");
        }

        return htmlBuilder.ToString();
    }
}
