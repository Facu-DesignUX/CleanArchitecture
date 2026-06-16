using CommerX.Domain.Common.Entities;
using CommerX.Domain.Customers.ValueObjects;

namespace CommerX.Domain.Customers.Entities;

// Customer es una entidad: tiene Id y ciclo de vida usa sealed class
public sealed class Customer : BaseEntity
{
    // propiedades tipadas con Value Objects ya no son strings primitivos
    public FirstName FirstName { get; private set; } = null!;
    public LastName LastName { get; private set; } = null!;
    public Document Document { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;
    public Phone Phone { get; private set; } = null!;
    public Address Address { get; private set; } = null!;
    public BirthDate BirthDate { get; private set; } = null!;

    // constructor privado vacío requerido por EF Core para materialización
    private Customer() { }

    // método estático de fábrica única puerta de entrada
    public static Customer Create(
        string firstName,
        string lastName,
        string document,
        string email,
        string phone,
        string address,
        DateOnly birthDate)
    {
        // cada Create() valida y normaliza su propio dato
        // Customer no repite ninguna validación de formato
        return new Customer
        {
            FirstName = FirstName.Create(firstName),
            LastName = LastName.Create(lastName),
            Document = Document.Create(document),
            Email = EmailAddress.Create(email),
            Phone = Phone.Create(phone),
            Address = Address.Create(address),
            BirthDate = BirthDate.Create(birthDate)
        };
    }
    // método de dominio para actualizar los datos permitidos del cliente
    public void Update(
        string email,      // nuevo email recibido como string primitivo desde la capa de aplicación
        string phone,      // nuevo teléfono
        string address,    // nueva dirección
        DateOnly birthDate) // nueva fecha de nacimiento
    {
        // ── Bloque 1: Validación ──────────────────────────────────────────────
        // Cada Create() construye el Value Object Y valida el dato internamente.
        // Si algún dato es inválido, lanza DomainException ANTES de modificar la entidad.
        // Así garantizamos que la entidad nunca quede en un estado inconsistente.
        // Usamos var porque el tipo ya queda implícito en el lado derecho (EmailAddress, Phone, etc.)
        var newEmail     = EmailAddress.Create(email);     // valida formato de email
        var newPhone     = Phone.Create(phone);            // valida formato de teléfono
        var newAddress   = Address.Create(address);        // valida que la dirección no esté vacía
        var newBirthDate = BirthDate.Create(birthDate);   // valida mayoría de edad

        // ── Bloque 2: Asignación ──────────────────────────────────────────────
        // Solo llegamos aquí si TODOS los Value Objects pasaron sus validaciones.
        // Recién en este punto se modifican las propiedades de la entidad.
        // Es una asignación atómica: o se actualizan todos juntos, o ninguno.
        Email     = newEmail;      // reemplaza el VO anterior por el nuevo ya validado
        Phone     = newPhone;      // ídem para teléfono
        Address   = newAddress;    // ídem para dirección
        BirthDate = newBirthDate;  // ídem para fecha de nacimiento

        // Nota: FirstName, LastName y Document NO se actualizan aquí.
        // Son datos de identidad del cliente: inmutables por regla de negocio.
        // Al no exponerlos en este método, el código queda blindado por diseño.
    }
}
