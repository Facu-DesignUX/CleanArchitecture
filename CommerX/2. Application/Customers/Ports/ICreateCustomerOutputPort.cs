// CommerX.Application/Customers/Ports/ICreateCustomerOutputPort.cs
using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Ports;

// puerto de salida: notifica el resultado al llamador
public interface ICreateCustomerOutputPort
{
    // el cliente fue creado exitosamente
    Task HandleSuccessAsync(CreateCustomerResponse response);

    // ya existe un cliente con ese documento - regla de unicidad violada
    Task HandleDuplicateAsync(string document);

    // una regla de dominio fue violada - mensaje descriptivo
    Task HandleValidationErrorAsync(string message);
}