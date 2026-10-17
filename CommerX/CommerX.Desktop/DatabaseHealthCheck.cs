using System.Threading.Tasks;
using CommerX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerX.Desktop;

// comprueba que la base de datos responde; vive como Singleton y por eso abre su propio scope
public sealed class DatabaseHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseHealthCheck(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> CanConnectAsync()
    {
        // el DbContext es Scoped: un Singleton no puede recibirlo por constructor, así que se crea un scope
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CommerXDbContext>();

        return await context.Database.CanConnectAsync();
    }
}
