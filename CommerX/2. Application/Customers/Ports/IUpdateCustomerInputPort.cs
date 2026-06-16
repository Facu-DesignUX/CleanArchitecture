using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Ports;

public interface IUpdateCustomerInputPort
{
    Task ExecuteAsync(UpdateCustomerRequest request);
}
