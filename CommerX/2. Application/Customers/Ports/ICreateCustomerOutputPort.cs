using System.Threading.Tasks;
using CommerX.Application.Common.Ports;
using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Ports;

public interface ICreateCustomerOutputPort : IBaseOutputPort<CreateCustomerResponse>
{
    Task HandleDuplicateDocumentAsync(string document);
    Task HandleDuplicateEmailAsync(string email);
}