namespace CommerX.Application.Customers.DTOs;

// DTO (Data Transfer Object): contenedor de datos plano, sin lógica de negocio.
// Su único trabajo es transportar los datos del llamador (UI, API, etc.) hasta el UseCase.
// sealed: no puede heredarse porque un DTO es un contrato fijo, no una base para extender.
public sealed class UpdateCustomerRequest
{
    // Guid: identificador único universal del cliente que queremos modificar.
    // No usamos int porque en sistemas distribuidos los ints pueden colisionar entre servidores;
    // el Guid fue generado al crear el cliente y nunca cambia.
    // No tiene 'required' porque Guid es un struct (tipo de valor): nunca puede ser null,
    // su peor caso es Guid.Empty, y eso lo detecta el UseCase al no encontrar el cliente en BD.
    // init: solo se puede asignar al construir el objeto (new UpdateCustomerRequest { CustomerId = ... }).
    // Una vez creado el DTO nadie puede cambiar el CustomerId desde afuera → inmutabilidad garantizada.
    public Guid CustomerId { get; init; }

    // string primitivo y NO EmailAddress (Value Object del dominio): esto es intencional.
    // El DTO es la frontera entre la UI y el dominio. La UI manda strings crudos;
    // es el dominio (Customer.Update → EmailAddress.Create) quien convierte y valida.
    // Si usáramos EmailAddress aquí, el DTO quedaría acoplado al Domain → rompe la arquitectura.
    // required: el compilador obliga a que quien construya este objeto asigne este campo.
    // Si olvidás poner el Email al crear el Request, el código directamente no compila.
    // Evita crear un UpdateCustomerRequest incompleto por descuido.
    // init: mismo razonamiento que CustomerId → el DTO es inmutable una vez construido.
    public required string Email { get; init; }

    // Mismo razonamiento que Email: string primitivo, required para forzar su asignación,
    // init para que nadie lo modifique después de construir el objeto.
    public required string Phone { get; init; }

    // Ídem para Address.
    public required string Address { get; init; }

    // DateOnly y no DateTime: solo nos interesa la fecha de nacimiento, no la hora.
    // DateOnly es más semántico y evita bugs de zona horaria que DateTime puede traer.
    // required + init: mismo razonamiento que los campos anteriores.
    public required DateOnly BirthDate { get; init; }

    // ── ¿Por qué NO están FirstName, LastName ni Document? ────────────────────
    // Esos tres son datos de identidad del cliente: inmutables por regla de negocio.
    // Una vez que el cliente fue creado, su nombre y documento no pueden modificarse.
    // Al no incluirlos en este DTO es físicamente imposible enviarlos al UseCase de Update:
    // el sistema queda blindado por diseño, no por convención ni por disciplina del programador.
}
