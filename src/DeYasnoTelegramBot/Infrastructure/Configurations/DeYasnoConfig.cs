namespace DeYasnoTelegramBot.Infrastructure.Configurations;

public sealed class DeYasnoConfig
{
    public static string ConfigName => "DeYasno";

    public LoggingConfig Logging { get; set; } = null!;

    public bool UseInMemoryDatabase { get; set; }

    public ConnectionStringsConfig ConnectionStrings { get; set; } = null!;

    public YasnoWebScrapperClientConfig YasnoWebScrapperClient { get; set; } = null!;
}
