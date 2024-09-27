using DeYasnoTelegramBot.Infrastructure.Configurations;
using MassTransit;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace DeYasnoTelegramBot;

public static class ConfigureLogging
{
    public static IServiceCollection ConfigLogging(this WebApplicationBuilder builder)
    {
        var config = GetDeYasnoConfig(builder.Configuration);

        builder.UseSerilog(config);

        return builder.Services;
    }

    private static void UseSerilog(this WebApplicationBuilder builder, DeYasnoConfig config)
    {
        var minimumLevel = GetLogEventLevel(config.Logging.MinimumLogLevel);

        IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
        {
            { "id", new SinglePropertyColumnWriter("id", PropertyWriteMethod.ToString, NpgsqlDbType.Text) },
            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
            { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
        };

        builder.Host.UseSerilog((context, configuration) => configuration
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", GetLogEventLevel(config.Logging.Microsoft))
            .MinimumLevel.Override("System", GetLogEventLevel(config.Logging.System))
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", GetLogEventLevel(config.Logging.MicrosoftAspNetCoreAuthentication))
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler", GetLogEventLevel(config.Logging.MicrosoftAspNetCoreAuthenticationJwtBearerJwtBearerHandler))
            .Enrich.FromLogContext()
            .Enrich.WithProperty("id", NewId.NextGuid())
            .WriteTo.Async(
                x => x.PostgreSQL(
                    connectionString: config.ConnectionStrings.LoggingConnection,
                    columnOptions: columnWriters,
                    restrictedToMinimumLevel: minimumLevel,
                    tableName: "_logs",
                    needAutoCreateTable: true))
            .WriteTo.Console());
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
