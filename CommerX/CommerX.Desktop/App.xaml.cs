using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CommerX.Desktop;

// se califica System.Windows.Application: sin eso choca con el namespace CommerX.Application
public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        // el host genérico trae configuración (appsettings.json), inyección de dependencias y logging
        _host = Host.CreateDefaultBuilder()
            // appsettings.json se busca junto al ejecutable, sin importar desde dónde se lance
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices((context, services) => services.AddCommerX(context.Configuration))
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // la ventana se resuelve del contenedor, no se crea con new: así recibe sus dependencias
        _host.Services.GetRequiredService<MainWindow>().Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
