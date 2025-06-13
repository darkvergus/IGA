using Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database.Infrastructure;

public sealed class IgaDesignTimeFactory : IDesignTimeDbContextFactory<IgaDbContext>
{
    public IgaDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<IgaDbContext> options = new DbContextOptionsBuilder<IgaDbContext>().UseSqlServer("Server=localhost;Database=IGA_Dev;Trusted_Connection=True;TrustServerCertificate=True;").Options;

        return new IgaDbContext(options);
    }
}