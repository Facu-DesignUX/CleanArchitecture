// CommerX.Application/Customers/Ports/ICreateCustomerInputPort.cs
using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Ports;

// puerto de entrada: recibe la solicitud del mundo exterior
public interface ICreateCustomerInputPort
{
    // ejecuta el caso de uso de forma asincrónica
    Task ExecuteAsync(CreateCustomerRequest request);
}