using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tides.Infrastructure.Persistence;

public class TidesDbContextFactory : IDesignTimeDbContextFactory<TidesDbContext>
{
    public TidesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TidesDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=tides;Username=tides;Password=tides_dev");

        return new TidesDbContext(optionsBuilder.Options);
    }
}
