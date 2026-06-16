using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Ports;

public interface IUpdateCustomerOutputPort
{
    // el cliente fue actualizado exitosamente
    Task HandleSuccessAsync(UpdateCustomerResponse response);

    // ya existe otro cliente con ese email
    Task HandleDuplicateAsync(string email);

    // una regla de dominio fue violada
    Task HandleValidationErrorAsync(string message);

    // el cliente a actualizar no fue encontrado
    Task HandleNotFoundAsync(Guid customerId);
}
