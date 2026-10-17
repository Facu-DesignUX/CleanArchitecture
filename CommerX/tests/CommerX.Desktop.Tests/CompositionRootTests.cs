using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using CommerX.Application.Customers.Ports;
using CommerX.Domain.Customers.Repositories;
using CommerX.Infrastructure.Data;
using CommerX.InterfaceAdapter.Customers.Presenters;
using CommerX.InterfaceAdapter.Customers.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommerX.Desktop.Tests;

public sealed class CompositionRootTests
{
    private const string LocalDb = "Server=(localdb)\\MSSQLLocalDB;Database=CommerX_Tests;Trusted_Connection=True;TrustServerCertificate=True;";

    private static IConfiguration ConfigurationWith(string? connectionString)
    {
        var values = new Dictionary<string, string?>();

        if (connectionString is not null)
        {
            values["ConnectionStrings:CommerX"] = connectionString;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    // las mismas dos opciones de validación estricta que conviene usar para detectar errores de configuración
    private static ServiceProvider Build(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddCommerX(configuration);

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
    }

    [Fact]
    public void AddCommerX_WithoutTheConnectionString_FailsWithAClearMessage()
    {
        var services = new ServiceCollection();

        var error = Assert.Throws<InvalidOperationException>(
            () => services.AddCommerX(ConfigurationWith(null)));

        Assert.Contains("CommerX", error.Message);
        Assert.Contains("appsettings.json", error.Message);
    }

    [Fact]
    public void AddCommerX_BuildsAContainerWhereEveryRegistrationCanBeResolved()
    {
        // ValidateOnBuild revisa que cada servicio registrado tenga todas sus dependencias registradas
        using var provider = Build(ConfigurationWith(LocalDb));

        Assert.NotNull(provider);
    }

    [Fact]
    public void AddCommerX_ResolvesTheWholeChainOfACaseOfUseInsideAScope()
    {
        using var provider = Build(ConfigurationWith(LocalDb));
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICreateCustomerInputPort>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUpdateCustomerInputPort>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<CreateCustomerViewModel>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ICustomerRepository>());
    }

    [Fact]
    public void AddCommerX_GivesTheViewModelAndTheUseCaseTheSamePresenterInstanceWithinAScope()
    {
        using var provider = Build(ConfigurationWith(LocalDb));
        using var scope = provider.CreateScope();

        var outputPort = scope.ServiceProvider.GetRequiredService<ICreateCustomerOutputPort>();
        var presenter = scope.ServiceProvider.GetRequiredService<CreateCustomerPresenter>();

        Assert.Same(presenter, outputPort);
    }

    [Fact]
    public void AddCommerX_ConfiguresTheDbContextWithSqlServerAndTheConfiguredConnectionString()
    {
        using var provider = Build(ConfigurationWith(LocalDb));
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CommerXDbContext>();

        Assert.Equal("Microsoft.EntityFrameworkCore.SqlServer", context.Database.ProviderName);

        var expected = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = LocalDb };
        var actual = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = context.Database.GetConnectionString() };

        var expectedServer = expected.ContainsKey("Server") ? expected["Server"] : expected["Data Source"];
        var actualServer = actual.ContainsKey("Server") ? actual["Server"] : actual["Data Source"];
        Assert.Equal(expectedServer, actualServer);

        var expectedDb = expected.ContainsKey("Database") ? expected["Database"] : expected["Initial Catalog"];
        var actualDb = actual.ContainsKey("Database") ? actual["Database"] : actual["Initial Catalog"];
        Assert.Equal(expectedDb, actualDb);
    }

    [Fact]
    public void TheShippedAppsettingsFile_IsValidAndHasTheConnectionString()
    {
        // appsettings.json se copia al directorio de salida junto con el host
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

        var services = new ServiceCollection();
        services.AddCommerX(configuration);

        Assert.False(string.IsNullOrWhiteSpace(configuration.GetConnectionString("CommerX")));
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenTheServerIsUnreachable_ReportsNoConnectionInsteadOfThrowing()
    {
        // puerto 1 de la propia máquina: rechaza la conexión de inmediato
        const string unreachable = "Server=127.0.0.1,1;Database=CommerX;User Id=x;Password=x;Encrypt=False;Connect Timeout=2;";
        using var provider = Build(ConfigurationWith(unreachable));

        var healthCheck = provider.GetRequiredService<DatabaseHealthCheck>();

        Assert.False(await healthCheck.CanConnectAsync());
    }
}
