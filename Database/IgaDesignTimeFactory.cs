using Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database;

public sealed class IgaDesignTimeFactory : IDesignTimeDbContextFactory<IgaDbContext>
{
    public IgaDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<IgaDbContext> options = new DbContextOptionsBuilder<IgaDbContext>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=IGA_Dev;Trusted_Connection=True;").Options;

        return new(options);
    }
}