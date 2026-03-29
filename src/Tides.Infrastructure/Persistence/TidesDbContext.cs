using Microsoft.EntityFrameworkCore;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence;

public class TidesDbContext : DbContext
{
    public TidesDbContext(DbContextOptions<TidesDbContext> options) : base(options) { }

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Carnival> Carnivals => Set<Carnival>();
    public DbSet<EventDefinition> Events => Set<EventDefinition>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<Heat> Heats => Set<Heat>();
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<PointsTable> PointsTables => Set<PointsTable>();
    public DbSet<Protest> Protests => Set<Protest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TidesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
