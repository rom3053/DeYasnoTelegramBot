using DeYasnoTelegramBot.Infrastructure.Configurations;
using DeYasnoTelegramBot.Infrastructure.HttpClients;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using DeYasnoTelegramBot.Infrastructure.Services;
using DeYasnoTelegramBot.Infrastructure.Telegram.Handlers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace DeYasnoTelegramBot.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, TelegramBotClient botClient)
    {
        var config = GetConfig(configuration);
        services.AddSingleton(typeof(DeYasnoConfig), config);

        if (config.UseInMemoryDatabase)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("DeYasnoDb"));
        }
        else
        {
            var dataSource = new NpgsqlDataSourceBuilder(config.ConnectionStrings.DefaultConnection)
               .EnableDynamicJson()
               .Build();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(dataSource,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        //Services
        services.AddSingleton<OutageScheduleStorage>();
        services.AddSingleton<ITelegramBotClient>(botClient);
        services.AddScoped<IUpdateHandler, UpdateHandler>();
        services.AddScoped<OutageInputService>();
        services.AddScoped<OutageNotificationService>();
        services.AddScoped<TelegramBotClientSender>();
        services.AddScoped<ApplicationDbContextInitialiser>();
        //HttpClients
        services.AddHttpClient<YasnoWebScrapperHttpClient>(options =>
        {
            options.BaseAddress = new Uri(config.YasnoWebScrapperClient.ServiceUrl);
        });

        return services;
    }

    static DeYasnoConfig GetConfig(IConfiguration configuration)
    {
        var config = new DeYasnoConfig();
        configuration.GetSection(DeYasnoConfig.ConfigName).Bind(config);

        return config;
    }
}
