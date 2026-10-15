using CommerX.Domain.Customers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerX.Infrastructure.Data.Configurations;

// toda la configuración de Fluent API del Customer vive acá, no en el DbContext
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> customer)
    {
        // tabla y clave primaria (el índice único del Id ya lo crea la clave primaria)
        customer.ToTable("Customers");
        customer.HasKey(c => c.Id);

        // auditoría de BaseEntity — CreatedAt solo tiene getter y por convención EF Core no mapea
        // propiedades de solo lectura: sin esta línea se regeneraría con la hora actual al cargar el cliente
        customer.Property(c => c.CreatedAt).IsRequired();
        customer.Property(c => c.UpdatedAt).IsRequired();

        // cada Value Object vive como columna dentro de Customers (Owned Type)
        // y su propiedad Value se configura con la longitud que valida el propio Value Object
        customer.OwnsOne(c => c.FirstName, vo =>
            vo.Property(x => x.Value).HasMaxLength(100).IsRequired());

        customer.OwnsOne(c => c.LastName, vo =>
            vo.Property(x => x.Value).HasMaxLength(100).IsRequired());

        // el documento y el email son únicos: el índice se declara dentro del Owned Type
        customer.OwnsOne(c => c.Document, vo =>
        {
            vo.Property(x => x.Value).HasMaxLength(8).IsRequired();
            vo.HasIndex(x => x.Value).IsUnique();
        });

        customer.OwnsOne(c => c.Email, vo =>
        {
            vo.Property(x => x.Value).HasMaxLength(254).IsRequired();
            vo.HasIndex(x => x.Value).IsUnique();
        });

        customer.OwnsOne(c => c.Phone, vo =>
            vo.Property(x => x.Value).HasMaxLength(20).IsRequired());

        customer.OwnsOne(c => c.Address, vo =>
            vo.Property(x => x.Value).HasMaxLength(200).IsRequired());

        customer.OwnsOne(c => c.BirthDate, vo =>
            vo.Property(x => x.Value).IsRequired());

        // un Customer siempre tiene sus siete Value Objects: ninguna columna queda nullable
        customer.Navigation(c => c.FirstName).IsRequired();
        customer.Navigation(c => c.LastName).IsRequired();
        customer.Navigation(c => c.Document).IsRequired();
        customer.Navigation(c => c.Email).IsRequired();
        customer.Navigation(c => c.Phone).IsRequired();
        customer.Navigation(c => c.Address).IsRequired();
        customer.Navigation(c => c.BirthDate).IsRequired();
    }
}
