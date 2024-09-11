using DeYasnoTelegramBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Subscriber> Subscribers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //ToDo: Investigate postgres jsonb and EF entity settings
        //Didn`t work to map sub-documents many in many
        //document with sub-documents you can map
        //but not documents with sub-documents
        builder.Entity<Subscriber>()
            .Property(p => p.OutageSchedules)
            .HasColumnType("jsonb");

        base.OnModelCreating(builder);

        //builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
