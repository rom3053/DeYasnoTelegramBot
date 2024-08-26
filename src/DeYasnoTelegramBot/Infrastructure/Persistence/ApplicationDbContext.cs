using System.Reflection.Emit;
using DeYasnoTelegramBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeYasnoTelegramBot.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Subscriber> Subscribers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
        .Entity<Subscriber>()
        .OwnsOne(product => product.OutageSchedules, builder => { builder.ToJson(); });

        base.OnModelCreating(builder);

        //builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
