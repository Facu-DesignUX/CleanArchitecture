// CommerX.Application/Customers/DTOs/CreateCustomerResponse.cs
namespace CommerX.Application.Customers.DTOs;

public class CreateCustomerResponse
{
    // identificador único generado por el sistema
    public required Guid CustomerId { get; init; }
}
//Modificar y dejar solamente CustomerId como unico campo requerido, los demás campos pueden ser opcionales o no requeridos dependiendo de la necesidad de la aplicación.  