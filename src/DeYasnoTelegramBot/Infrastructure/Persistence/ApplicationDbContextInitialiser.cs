using DeYasnoTelegramBot.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Infrastructure.Persistence;

public class ApplicationDbContextInitialiser
{
    private readonly ApplicationDbContext _context;
    private readonly OutageScheduleStorage _outageScheduleStorage;
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;

    public ApplicationDbContextInitialiser(ApplicationDbContext context,
        OutageScheduleStorage outageScheduleStorage,
        ILogger<ApplicationDbContextInitialiser> logger)
    {
        _context = context;
        _outageScheduleStorage = outageScheduleStorage;
        _logger = logger;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsNpgsql())
            {
                //await _context.Database.EnsureDeletedAsync();
                await _context.Database.MigrateAsync();
                await OutageScheduleStorage.InitCache(_context, _outageScheduleStorage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}
