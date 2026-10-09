using System;
using System.Threading.Tasks;
using CommerX.Application.Common.Results;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.InterfaceAdapter.Common.Presenters;

namespace CommerX.InterfaceAdapter.Customers.Presenters;

public sealed class UpdateCustomerPresenter : BasePresenter<UpdateCustomerResponse>, IUpdateCustomerOutputPort
{
    public Task HandleDuplicateAsync(string email)
    {
        Result = OperationResult<UpdateCustomerResponse>.Fail($"Ya existe un cliente usando el email: {email}");
        return Task.CompletedTask;
    }

    public Task HandleNotFoundAsync(Guid customerId)
    {
        Result = OperationResult<UpdateCustomerResponse>.Fail($"No se encontró el cliente con ID: {customerId}");
        return Task.CompletedTask;
    }
}
