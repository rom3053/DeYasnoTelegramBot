using DeYasnoTelegramBot;
using DeYasnoTelegramBot.Application;
using DeYasnoTelegramBot.Background;
using DeYasnoTelegramBot.Infrastructure;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.FeatureManagement;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting the service");
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    ConfigurationManager configuration = builder.Configuration;

    TelegramBotClient bot = new TelegramBotClient(configuration["TelegramBotKey"]);

    //ToDo: background for cleaning empty schedules
    //ToDo: background for updating all users schedules
    //ToDo: some validations
    //ToDo: maybe reuse browser session
    //ToDo: diasbale notification command
    //ToDo:
    builder.ConfigLogging();
    builder.Services
        .AddFeatureManagement(configuration.GetSection("DeYasno:FeatureFlags"));
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(configuration, bot);

    builder.Services.AddHostedService<OutageNotificationAt5minJob>();
    builder.Services.AddHostedService<OutageNotificationAt15minJob>();
    builder.Services.AddHostedService<OutageNotificationAt30minJob>();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    WebApplication app = builder.Build();

    using var scope = app.Services.CreateScope();
    var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
    await initialiser.InitialiseAsync();

    bot.StartReceiving(
        HandleUpdateAsync,
        HandlePollingErrorAsync,
        new ReceiverOptions(),
        new CancellationToken());

    // Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();

    //app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();


    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateHandler>();

        await updateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
    }

    async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        string errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        await Task.CompletedTask;
    }

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
