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
        
        // Registrar Hubs de Validación
        services.AddScoped<
            CommerX.Application.Common.Validation.IModelValidatorHub<CommerX.Application.Customers.DTOs.CreateCustomerRequest>,
            CommerX.Application.Customers.Validation.CreateCustomerValidatorHub>();

        services.AddScoped<
            CommerX.Application.Common.Validation.IModelValidatorHub<CommerX.Application.Customers.DTOs.UpdateCustomerRequest>,
            CommerX.Application.Customers.Validation.UpdateCustomerValidatorHub>();
        
        return services;
    }
}
