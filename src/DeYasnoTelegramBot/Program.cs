using DeYasnoTelegramBot.Application;
using DeYasnoTelegramBot.Background;
using DeYasnoTelegramBot.Infrastructure;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
ConfigurationManager configuration = builder.Configuration;

TelegramBotClient bot = new TelegramBotClient(configuration["TelegramBotKey"]);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(configuration, bot);

builder.Services.AddHostedService<OutageNotificationJob>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Hosted services


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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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