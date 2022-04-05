using Microsoft.EntityFrameworkCore;

namespace CloudWeather.Precipitation.DataAccess;

public class PrecipitationDbContext : DbContext {
    public PrecipitationDbContext()
    {

    }

    public PrecipitationDbContext(DbContextOptions opts):base(opts)
    {

    }

    public DbSet<Precipitation> PrecipitationSet { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        SnakeCaseIdentityTableNames(modelBuilder);
    }

    private void SnakeCaseIdentityTableNames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Precipitation>(b => { b.ToTable("precipitation"); });
    }
}

