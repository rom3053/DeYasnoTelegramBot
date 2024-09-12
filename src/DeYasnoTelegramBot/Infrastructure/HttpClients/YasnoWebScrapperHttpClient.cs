using System.Net.Mime;
using System.Text;
using DeYasnoTelegramBot.Application.Common.Constants.ApiActions;
using DeYasnoTelegramBot.Application.Common.Dtos;
using DeYasnoTelegramBot.Application.Common.Dtos.YasnoWebScrapper;
using DeYasnoTelegramBot.Infrastructure.HttpClients.Base;
using Newtonsoft.Json;

namespace DeYasnoTelegramBot.Infrastructure.HttpClients;

public class YasnoWebScrapperHttpClient : BaseHttpClient
{
    private readonly ILogger<YasnoWebScrapperHttpClient> _logger;

    public YasnoWebScrapperHttpClient(HttpClient httpClient,
        ILogger<YasnoWebScrapperHttpClient> logger) : base(httpClient)
    {
        _logger = logger;
    }

    #region Session
    public async Task<SessionDto> InitSessionAsync()
    {
        var url = GetUrl(YasnoScrapperApiActions.InitSession);

        var response = await GetAsync<SessionDto>(url, "Init browser session");

        return response;
    }

    public async Task<SessionDto> GetSessionAsync(string sessionId)
    {
        var url = GetUrl(YasnoScrapperApiActions.GetSession).Replace("{sessionId}", sessionId);

        var response = await GetAsync<SessionDto>(url, "Get browser session");

        return response;
    }
    #endregion

    public async Task InputRegion(string sessionId, string userInput)
    {
        var url = GetUrl(YasnoScrapperApiActions.Step_1_SelectRegion).Replace("{sessionId}", sessionId);

        var response = await PostAsync<object>(url, userInput, "Post Input Region");

        return;
    }

    public async Task<List<DropdownOptionDto>> GetOptionsAndInputCity(string sessionId, string userInput)
    {
        var url = GetUrl(YasnoScrapperApiActions.Step_2_InputCity).Replace("{sessionId}", sessionId);

        var response = await PostAsync<List<DropdownOptionDto>>(url, userInput, "Post Input City");

        return response;
    }

    public async Task<List<DropdownOptionDto>> GetOptionsAndInputStreet(string sessionId, string userInput)
    {
        var url = GetUrl(YasnoScrapperApiActions.Step_4_InputStreet).Replace("{sessionId}", sessionId);

        var response = await PostAsync<List<DropdownOptionDto>>(url, userInput, "Post Input Street");

        return response;
    }

    public async Task<List<DropdownOptionDto>> GetOptionsAndInputHouseNumber(string sessionId, string userInput)
    {
        var url = GetUrl(YasnoScrapperApiActions.Step_6_InputHouseNumber).Replace("{sessionId}", sessionId);

        var response = await PostAsync<List<DropdownOptionDto>>(url, userInput, "Post Input HouseNumber");

        return response;
    }

    public async Task<SelectedDropdownOptionDto> SelectDropdownOption(string sessionId, string userInput)
    {
        var url = GetUrl(YasnoScrapperApiActions.Step_3_5_7_SelectOption).Replace("{sessionId}", sessionId);

        var response = await PostAsync<SelectedDropdownOptionDto>(url, userInput, "Post Select Dropdown Option");

        return response;
    }

    public async Task<List<OutageScheduleDayDto>> GetParsedTable(string sessionId)
    {
        var url = GetUrl(YasnoScrapperApiActions.GetParsedTable).Replace("{sessionId}", sessionId);

        var response = await GetAsync<List<OutageScheduleDayDto>>(url, "Get parsed schedule");

        return response;
    }

    public async Task<FileDto> GetScheduleScreenshot(string sessionId, string cityName)
    {
        QueryString paramsQuery = new QueryString();
        paramsQuery = paramsQuery.Add("cityName", cityName);

        var url = GetUrl(YasnoScrapperApiActions.GetScheduleScreenshot).Replace("{sessionId}", sessionId) + paramsQuery;

        var response = await GetAsync<FileDto>(url, "Get schedule sreenshot");

        return response;
    }

    private async Task<T> GetAsync<T>(string url, string actionTitle)
    {
        _logger.LogInformation($"Sending to {url} action with title: {actionTitle}.");
        var response = await _httpClient.GetAsync(url);
        _logger.LogInformation($"Got response: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            throw new Exception($"Error during sending {actionTitle}: {response.ReasonPhrase}. Body: {responseBody}");
        }

        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }

    private async Task<T> PostAsync<T>(string url, object body, string actionTitle)
    {
        _logger.LogInformation($"Sending to {url} action with title: {actionTitle}.");
        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync(url, content);
        _logger.LogInformation($"Got response: {response.StatusCode}");

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            throw new Exception($"Error during sending {actionTitle}: {response.ReasonPhrase}. Body: {responseBody}");
        }

        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
    }
}
