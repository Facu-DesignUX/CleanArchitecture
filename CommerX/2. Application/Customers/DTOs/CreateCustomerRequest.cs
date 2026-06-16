// CommerX.Application/Customers/DTOs/CreateCustomerRequest.cs
namespace CommerX.Application.Customers.DTOs;

// class no record: los DTOs son clases, no tipos de valor
public class CreateCustomerRequest
{
    // required: el compilador obliga a inicializar esta propiedad
    // init: solo puede asignarse durante la construcción del objeto
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Document { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string Address { get; init; }
    // DateOnly: representa solo una fecha, sin hora ni zona horaria
    public required DateOnly BirthDate { get; init; }
}