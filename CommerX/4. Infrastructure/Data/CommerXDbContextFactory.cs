using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CommerX.Infrastructure.Data;

// solo la usan las herramientas de EF (dotnet ef migrations / database update) en tiempo de diseño
// permite generar migraciones sin un proyecto host: el host real recibe su cadena de conexión por DI
public sealed class CommerXDbContextFactory : IDesignTimeDbContextFactory<CommerXDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=CommerX;Trusted_Connection=True;TrustServerCertificate=True;";

    public CommerXDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CommerXDbContext>()
            .UseSqlServer(DesignTimeConnectionString)
            .Options;

        return new CommerXDbContext(options);
    }
}
