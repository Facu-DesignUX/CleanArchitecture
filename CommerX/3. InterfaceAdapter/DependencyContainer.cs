using CommerX.Application.Customers.Ports;
using CommerX.InterfaceAdapter.Customers.Presenters;

// namespace truco: extiende IServiceCollection sin agregar using en Program.cs
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddCustomerPresenter(this IServiceCollection services)
    {
        // registrar el Presenter como Scoped - una instancia por request
        services.AddScoped<CreateCustomerPresenter>();

        // registrar el OutputPort como alias de la misma instancia del Presenter
        services.AddScoped<ICreateCustomerOutputPort>(
            sp => sp.GetRequiredService<CreateCustomerPresenter>());

        return services;
    }
}
