using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RealFenixFailures.Infrastructure.Persistence;

public class RealFenixDbContextFactory : IDesignTimeDbContextFactory<RealFenixDbContext>
{
    public RealFenixDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RealFenixDbContext>();
        optionsBuilder.UseSqlite("Data Source=realfenixfailures.db");

        return new RealFenixDbContext(optionsBuilder.Options);
    }
}
