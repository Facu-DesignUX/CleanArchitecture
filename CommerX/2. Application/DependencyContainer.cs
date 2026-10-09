using CommerX.Application.Customers.Ports;
using CommerX.Application.Customers.UseCases;
using Microsoft.Extensions.DependencyInjection;

// namespace truco: evita agregar using en Program.cs
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddCustomerUseCases(this IServiceCollection services)
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
