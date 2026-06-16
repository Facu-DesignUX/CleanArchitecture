// ── Importaciones ─────────────────────────────────────────────────────────────
using CommerX.Application.Customers.DTOs;   // UpdateCustomerRequest y UpdateCustomerResponse: los datos que entran y salen del caso de uso
using CommerX.Application.Customers.Ports;  // IUpdateCustomerInputPort e IUpdateCustomerOutputPort: contratos de comunicación
using CommerX.Domain.Common.Exceptions;     // DomainException: excepción que lanzan los Value Objects si un dato es inválido
using CommerX.Domain.Customers.Repositories; // ICustomerRepository: contrato para acceder a la base de datos (definido en Domain)

namespace CommerX.Application.Customers.UseCases;

// sealed: esta clase no puede ser heredada, su comportamiento es definitivo e inamovible.
// Implementa IUpdateCustomerInputPort: ese contrato obliga a tener el método ExecuteAsync().
// La capa de UI (ConsoleTest, API, etc.) solo conoce la interfaz, nunca esta clase directamente.
public sealed class UpdateCustomerUseCase : IUpdateCustomerInputPort
{
    // Campo privado de solo lectura: guarda la referencia al repositorio inyectado.
    // readonly garantiza que no se pueda reasignar después del constructor.
    // ICustomerRepository es una interfaz definida en Domain → la capa de Application
    // depende de una abstracción, nunca de una implementación concreta (Inversión de Dependencias).
    private readonly ICustomerRepository _repository;

    // Puerto de salida: es la forma que tiene el caso de uso de "hablarle" al llamador.
    // En lugar de retornar un valor directo, llama a métodos del OutputPort según el resultado.
    // Esto desacopla el caso de uso de quién lo invoca (puede ser una API, consola, etc.).
    private readonly IUpdateCustomerOutputPort _outputPort;

    // Constructor: recibe las dependencias por parámetro (Inyección de Dependencias).
    // Nunca instanciamos el repositorio ni el outputPort aquí adentro con "new",
    // eso sería acoplamiento fuerte. Los recibimos ya construidos desde afuera.
    public UpdateCustomerUseCase(
        ICustomerRepository repository,           // implementación de BD que viene de Infrastructure
        IUpdateCustomerOutputPort outputPort)     // implementación del presenter que viene de UI
    {
        _repository  = repository;   // guardamos el repositorio en el campo privado
        _outputPort  = outputPort;   // guardamos el puerto de salida en el campo privado
    }

    // Método principal del caso de uso. Es async porque interactúa con la base de datos (I/O).
    // Recibe un UpdateCustomerRequest: DTO con los datos nuevos que el usuario quiere guardar.
    // Task indica que es una operación asincrónica que no devuelve valor directo.
    public async Task ExecuteAsync(UpdateCustomerRequest request)
    {
        // try/catch envuelve todo: si el dominio lanza DomainException la capturamos abajo
        // sin dejar que explote sin control hacia la capa superior.
        try
        {
            // ── Paso 1: ¿Existe el cliente que queremos actualizar? ────────────────
            // Buscamos en la BD al cliente usando el Id que llegó en el request.
            // await suspende la ejecución hasta que la BD responda (sin bloquear el hilo).
            // var infiere el tipo Customer? (puede ser null si no existe en la BD).
            var customer = await _repository.FindByIdAsync(request.CustomerId);

            // Chequeamos si el repositorio devolvió null (cliente no encontrado).
            // Usamos "is null" en lugar de "== null": es el patrón moderno en C#.
            if (customer is null)
            {
                // Notificamos al OutputPort que no se encontró el cliente.
                // El OutputPort decidirá qué hacer (mostrar un mensaje, devolver 404, etc.)
                // según la capa que lo implemente. El caso de uso no sabe ni le importa.
                await _outputPort.HandleNotFoundAsync(request.CustomerId);

                // return corta la ejecución aquí: no tiene sentido continuar si no existe.
                return;
            }

            // ── Paso 2: ¿El nuevo email ya está en uso por OTRO cliente? ──────────
            // Buscamos si alguien más en la BD tiene el email que nos llegó en el request.
            var existingWithEmail = await _repository.FindByEmailAsync(request.Email);

            // La condición tiene dos partes unidas con &&:
            //   - existingWithEmail is not null → alguien tiene ese email
            //   - existingWithEmail.Id != customer.Id → pero NO es el mismo cliente que estamos editando
            // Esto permite que el cliente mantenga su propio email sin ser marcado como duplicado.
            if (existingWithEmail is not null && existingWithEmail.Id != customer.Id)
            {
                // El email ya está registrado en otro cliente: violación de unicidad.
                // Notificamos la duplicidad al OutputPort y cortamos la ejecución.
                await _outputPort.HandleDuplicateAsync(request.Email);
                return;
            }

            // ── Paso 3: ¿Hay cambios reales respecto a los datos actuales? ────────
            // Comparamos cada campo del request con el valor actual del cliente en BD.
            // .Value extrae el string primitivo del Value Object para poder compararlo.
            // Si los cuatro valores son idénticos, no tiene sentido persistir nada.
            if (customer.Email.Value    == request.Email    &&
                customer.Phone.Value    == request.Phone    &&
                customer.Address.Value  == request.Address  &&
                customer.BirthDate.Value == request.BirthDate)
            {
                // No hay ningún cambio: armamos la respuesta con los datos actuales
                // y notificamos éxito de todas formas (para no confundir al usuario).
                // No llamamos a _repository.UpdateAsync() porque no hay nada que guardar.
                var noChangesResponse = new UpdateCustomerResponse
                {
                    CustomerId = customer.Id,          // Id de la entidad (Guid generado al crearla)
                    Email      = customer.Email.Value,     // extraemos el string del Value Object
                    Phone      = customer.Phone.Value,
                    Address    = customer.Address.Value,
                    BirthDate  = customer.BirthDate.Value
                };

                // Enviamos la respuesta al OutputPort como si todo fuera bien.
                await _outputPort.HandleSuccessAsync(noChangesResponse);
                return; // cortamos: no hay nada más que hacer
            }

            // ── Paso 4: Actualizar la entidad a través de su método de dominio ───
            // Le delegamos al dominio la responsabilidad de aplicar los cambios.
            // customer.Update() primero valida (Bloque 1 en Customer.cs) y después asigna (Bloque 2).
            // Si algún dato es inválido, lanza DomainException → cae en el catch de abajo.
            // El caso de uso NO repite las validaciones: esa es responsabilidad del dominio.
            customer.Update(
                request.Email,      // nuevo email (string primitivo → Customer.Update() lo convierte a VO)
                request.Phone,      // nuevo teléfono
                request.Address,    // nueva dirección
                request.BirthDate   // nueva fecha de nacimiento
            );

            // ── Paso 5: Persistir los cambios en la base de datos ────────────────
            // Solo llegamos aquí si el dominio aceptó todos los cambios sin lanzar excepciones.
            // UpdateAsync() guarda la entidad modificada. La implementación real está en Infrastructure.
            await _repository.UpdateAsync(customer);

            // ── Paso 6: Construir la respuesta y notificar el éxito ───────────────
            // Armamos el DTO de salida con los datos YA actualizados de la entidad.
            // Usamos customer.Email.Value (no request.Email) para garantizar que devolvemos
            // exactamente lo que quedó persistido (por si el VO normalizó el dato, ej: lowercase).
            var response = new UpdateCustomerResponse
            {
                CustomerId = customer.Id,
                Email      = customer.Email.Value,
                Phone      = customer.Phone.Value,
                Address    = customer.Address.Value,
                BirthDate  = customer.BirthDate.Value
            };

            // Notificamos el éxito al OutputPort con el DTO de respuesta.
            // El OutputPort hará lo que corresponda según la capa: mostrar en consola, responder HTTP 200, etc.
            await _outputPort.HandleSuccessAsync(response);
        }
        catch (DomainException ex)
        {
            // Si customer.Update() lanzó DomainException (email mal formado, menor de edad, etc.)
            // la capturamos aquí y la redirigimos al OutputPort como error de validación.
            // ex.Message trae el mensaje legible definido en el Value Object (ej: "El email no es válido").
            // Nunca relanzamos la excepción: el OutputPort decide cómo mostrar el error.
            await _outputPort.HandleValidationErrorAsync(ex.Message);
        }
    }
}
