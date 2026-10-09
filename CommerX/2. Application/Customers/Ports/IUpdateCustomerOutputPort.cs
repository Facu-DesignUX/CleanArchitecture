using System;
using System.Threading.Tasks;
using CommerX.Application.Common.Ports;
using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Ports;

public interface IUpdateCustomerOutputPort : IBaseOutputPort<UpdateCustomerResponse>
{
    Task HandleDuplicateAsync(string email);
    Task HandleNotFoundAsync(Guid customerId);
}
