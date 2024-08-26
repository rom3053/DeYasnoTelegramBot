namespace DeYasnoTelegramBot.Infrastructure.Configurations;

public class YasnoWebScrapperClientConfig
{
    public string ApiKeyHeader => "x-api-key";

    public string ServiceUrl { get; set; }

    public string ApiKey { get; set; }
}
