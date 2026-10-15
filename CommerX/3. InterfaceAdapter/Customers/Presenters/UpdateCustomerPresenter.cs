using System.Threading.Tasks;
using CommerX.Application.Common.Results;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.InterfaceAdapter.Common.Presenters;

namespace CommerX.InterfaceAdapter.Customers.Presenters;

public sealed class UpdateCustomerPresenter : BasePresenter<UpdateCustomerResponse>, IUpdateCustomerOutputPort
{
    public UpdateCustomerResponse Response => Result?.Value ?? default!;

    public Task HandleDuplicateAsync(string email)
    {
        Result = OperationResult<UpdateCustomerResponse>.Fail($"El email {email} ya está registrado.");
        return Task.CompletedTask;
    }

    public Task HandleNotFoundAsync(Guid customerId)
    {
        Result = OperationResult<UpdateCustomerResponse>.Fail($"No se encontró ningún cliente con el Id {customerId}.");
        return Task.CompletedTask;
    }
}
