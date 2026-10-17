using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommerX.Desktop;

// composition root — el único lugar de la aplicación que conoce todas las capas y arma las dependencias
public static class CompositionRoot
{
    public static IServiceCollection AddCommerX(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // la cadena de conexión viene de appsettings.json; si falta, se falla al arrancar con un mensaje claro
        var connectionString = configuration.GetConnectionString("CommerX")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'CommerX' en appsettings.json (sección ConnectionStrings).");

        // Application: casos de uso y validación
        services.AddCustomerUseCases();

        // InterfaceAdapter: presenters y view models
        services.AddCustomerPresenter();
        services.AddUpdateCustomerPresenter();
        services.AddCustomerViewModel();

        // Infrastructure: DbContext y repositorios
        services.AddInfrastructure(connectionString);

        // servicios propios del host
        services.AddSingleton<DatabaseHealthCheck>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
