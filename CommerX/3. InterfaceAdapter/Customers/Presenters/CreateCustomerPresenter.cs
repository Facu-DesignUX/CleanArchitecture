using System.Threading.Tasks;
using CommerX.Application.Common.Results;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.InterfaceAdapter.Common.Presenters;

namespace CommerX.InterfaceAdapter.Customers.Presenters;

public sealed class CreateCustomerPresenter : BasePresenter<CreateCustomerResponse>, ICreateCustomerOutputPort
{
    public CreateCustomerResponse Response => Result?.Value ?? default!;

    public Task HandleDuplicateDocumentAsync(string document)
    {
        Result = OperationResult<CreateCustomerResponse>.Fail($"El documento {document} ya está registrado.");
        return Task.CompletedTask;
    }

    public Task HandleDuplicateEmailAsync(string email)
    {
        Result = OperationResult<CreateCustomerResponse>.Fail($"El email {email} ya está registrado.");
        return Task.CompletedTask;
    }
}
