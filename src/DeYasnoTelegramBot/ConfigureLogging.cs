using DeYasnoTelegramBot.Background;
using DeYasnoTelegramBot.Infrastructure.Configurations;
using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace DeYasnoTelegramBot;

public static class ConfigureLogging
{
    public static IServiceCollection ConfigLogging(this WebApplicationBuilder builder)
    {
        var config = GetDeYasnoConfig(builder.Configuration);

        builder.Services.AddDbContext<LoggerDbContext>(options =>
            options.UseNpgsql(config.ConnectionStrings.LoggingConnection));

        builder.Services.AddHostedService<LoggerCleanerJob>();

        builder.UseSerilog(config);

        return builder.Services;
    }

    private static void UseSerilog(this WebApplicationBuilder builder, DeYasnoConfig config)
    {
        var minimumLevel = GetLogEventLevel(config.Logging.MinimumLogLevel);

        IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "id", new SinglePropertyColumnWriter("id", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
            { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
        };

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", GetLogEventLevel(config.Logging.Microsoft))
            .MinimumLevel.Override("System", GetLogEventLevel(config.Logging.System))
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", GetLogEventLevel(config.Logging.MicrosoftAspNetCoreAuthentication))
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler", GetLogEventLevel(config.Logging.MicrosoftAspNetCoreAuthenticationJwtBearerJwtBearerHandler))
            .Enrich.With<IdEnricher>()
            .Enrich.FromLogContext()
            .WriteTo.Async(
                x => x.PostgreSQL(
                    connectionString: config.ConnectionStrings.LoggingConnection,
                    columnOptions: columnWriters,
                    restrictedToMinimumLevel: minimumLevel,
                    tableName: "_logs",
                    needAutoCreateTable: true))
            .WriteTo.Console());
    }

    private class IdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var idProperty = new LogEventProperty("id", new ScalarValue(Ulid.NewUlid().ToString()));
            logEvent.AddPropertyIfAbsent(idProperty);
        }
    }

    private static DeYasnoConfig GetDeYasnoConfig(IConfiguration configuration)
    {
        var config = new DeYasnoConfig();
        configuration.GetSection(DeYasnoConfig.ConfigName).Bind(config);

        return config;
    }

    private static LogEventLevel GetLogEventLevel(string logLevel)
    {
        var minimumLogLevel =
            string.IsNullOrWhiteSpace(logLevel) ?
        LogEventLevel.Information : Enum.Parse<LogEventLevel>(logLevel);

        return minimumLogLevel;
    }
}
