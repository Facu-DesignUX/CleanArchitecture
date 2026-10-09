// CommerX.Application/Customers/DTOs/CreateCustomerResponse.cs
namespace CommerX.Application.Customers.DTOs;

public class CreateCustomerResponse
{
    // identificador único generado por el sistema
    public required Guid CustomerId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}