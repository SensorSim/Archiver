using archiver.Models;
using Microsoft.EntityFrameworkCore;

namespace archiver.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Measurement> Measurements => Set<Measurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Measurement>()
            .HasIndex(m => new { m.SensorId, m.Timestamp });

        base.OnModelCreating(modelBuilder);
    }
}
