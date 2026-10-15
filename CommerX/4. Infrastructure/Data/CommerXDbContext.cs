using CommerX.Domain.Customers.Entities;
using CommerX.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CommerX.Infrastructure.Data;

public sealed class CommerXDbContext : DbContext
{
    public CommerXDbContext(DbContextOptions<CommerXDbContext> options)
        : base(options)
    {
    }

    // Set<T>() evita la advertencia de nulabilidad: EF Core resuelve el DbSet por su cuenta
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // el DbContext solo enumera las configuraciones; el detalle de cada entidad está en su clase
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }
}
