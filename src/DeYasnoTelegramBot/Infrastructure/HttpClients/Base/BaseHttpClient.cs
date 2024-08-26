namespace DeYasnoTelegramBot.Infrastructure.HttpClients.Base;

public abstract class BaseHttpClient
{
    protected readonly HttpClient _httpClient;

    protected BaseHttpClient(HttpClient client) => _httpClient = client;

    protected string GetUrl(string url)
    {
        return $"{_httpClient.BaseAddress}{url}";
    }
}
