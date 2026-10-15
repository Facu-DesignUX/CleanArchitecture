using CommerX.Domain.Customers.Repositories;
using CommerX.Infrastructure.Customers;
using CommerX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

// namespace truco — extiende IServiceCollection sin agregar using en Program.cs
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        // DbContext con SQL Server — la cadena de conexión la entrega el host, no vive en el código
        services.AddDbContext<CommerXDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Scoped: el repositorio comparte el DbContext del scope en el que se resuelve
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        return services;
    }
}
