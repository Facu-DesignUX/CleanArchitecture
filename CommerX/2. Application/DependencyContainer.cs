using CommerX.Application.Customers.Ports;
using CommerX.Application.Customers.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace CommerX.Application;

public static class DependencyContainer
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar Casos de Uso
        services.AddScoped<ICreateCustomerInputPort, CreateCustomerUseCase>();
        services.AddScoped<IUpdateCustomerInputPort, UpdateCustomerUseCase>();
        
        return services;
    }
}
