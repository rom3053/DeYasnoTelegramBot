using DeYasnoTelegramBot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Background;

public class LoggerCleanerJob : BackgroundService
{
    private readonly ILogger<LoggerCleanerJob> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PeriodicTimer _periodicTimer;

    public LoggerCleanerJob(ILogger<LoggerCleanerJob> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _periodicTimer = new(TimeSpan.FromHours(24));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _periodicTimer.WaitForNextTickAsync(cancellationToken) &&
            !cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Background {JobName} service executed", nameof(LoggerCleanerJob));

                await using var scope = _serviceProvider.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<LoggerDbContext>();
                var dateTimeNow = DateTime.UtcNow.AddDays(-3);
                await context.LogRecords.Where(x => x.RaiseDate.Value < dateTimeNow).ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
