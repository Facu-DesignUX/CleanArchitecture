namespace CommerX.Application.Customers.DTOs;

public sealed class UpdateCustomerResponse
{
    public Guid CustomerId { get; init; }
    public required string Email { get; init; }
    public required string Phone { get; init; }
    public required string Address { get; init; }
    public required DateOnly BirthDate { get; init; }

}
