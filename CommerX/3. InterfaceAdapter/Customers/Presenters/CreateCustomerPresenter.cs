using System.Threading.Tasks;
using CommerX.Application.Common.Results;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.InterfaceAdapter.Common.Presenters;

namespace CommerX.InterfaceAdapter.Customers.Presenters;

public sealed class CreateCustomerPresenter : BasePresenter<CreateCustomerResponse>, ICreateCustomerOutputPort
{
    public Task HandleDuplicateAsync(string document)
    {
        // Guardamos el error específico del UseCase en el resultado base
        Result = OperationResult<CreateCustomerResponse>.Fail($"Ya existe un cliente registrado con el documento: {document}");
        return Task.CompletedTask;
    }
}
