// CommerX.Application/Customers/UseCases/UpdateCustomerUseCase.cs
using CommerX.Application.Common.Validation;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.Domain.Common.Exceptions;
using CommerX.Domain.Customers.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace CommerX.Application.Customers.UseCases;

public sealed class UpdateCustomerUseCase : IUpdateCustomerInputPort
{
    private readonly ICustomerRepository _repository;
    private readonly IUpdateCustomerOutputPort _outputPort;
    private readonly IModelValidatorHub<UpdateCustomerRequest> _validator;

    public UpdateCustomerUseCase(
        ICustomerRepository repository,
        IUpdateCustomerOutputPort outputPort,
        IModelValidatorHub<UpdateCustomerRequest> validator)
    {
        _repository = repository;
        _outputPort = outputPort;
        _validator = validator;
    }

    public async Task ExecuteAsync(UpdateCustomerRequest request)
    {
        var errors = _validator.Validate(request).ToList();
        if (errors.Count > 0)
        {
            await _outputPort.ValidationErrorsAsync(errors);
            return;
        }

        try
        {
            var customer = await _repository.GetByIdAsync(request.CustomerId);

            if (customer is null)
            {
                await _outputPort.HandleNotFoundAsync(request.CustomerId);
                return;
            }

            var existingEmail = await _repository.FindByEmailAsync(request.Email);
            if (existingEmail is not null && existingEmail.Id != customer.Id)
            {
                await _outputPort.HandleDuplicateAsync(request.Email);
                return;
            }

            customer.Update(
                request.Email,
                request.Phone,
                request.Address,
                request.BirthDate
            );

            await _repository.UpdateAsync(customer);

            var response = new UpdateCustomerResponse
            {
                CustomerId = customer.Id,
            };

            await _outputPort.HandleSuccessAsync(response);
        }
        catch (DomainException ex)
        {
            await _outputPort.HandleErrorAsync(ex.Message);
        }
    }
}
