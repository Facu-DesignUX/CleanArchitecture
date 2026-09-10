// CommerX.Application/Customers/UseCases/CreateCustomerUseCase.cs
using CommerX.Application.Common.Validation;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.Domain.Common.Exceptions;
using CommerX.Domain.Customers.Entities;
using CommerX.Domain.Customers.Repositories;
using System.Linq;

namespace CommerX.Application.Customers.UseCases;

// sealed: no puede heredarse - su comportamiento es definitivo
public sealed class CreateCustomerUseCase : ICreateCustomerInputPort
{
    // repositorio inyectado - contrato definido en Domain
    private readonly ICustomerRepository _repository;

    // puerto de salida inyectado - notifica el resultado al llamador
    private readonly ICreateCustomerOutputPort _outputPort;

    // hub de validación - valida las precondiciones técnicas del DTO
    private readonly IModelValidatorHub<CreateCustomerRequest> _validator;

    public CreateCustomerUseCase(
        ICustomerRepository repository,
        ICreateCustomerOutputPort outputPort,
        IModelValidatorHub<CreateCustomerRequest> validator)
    {
        _repository = repository;
        _outputPort = outputPort;
        _validator = validator;
    }

    public async Task ExecuteAsync(CreateCustomerRequest request)
    {
        // PASO 1 — Guard Hub: precondiciones técnicas del DTO
        var errors = _validator.Validate(request).ToList();
        if (errors.Count > 0)
        {
            // notifica todos los errores juntos — el dominio no es invocado
            await _outputPort.ValidationErrorsAsync(errors);
            return;
        }

        try
        {
            // verificamos que no exista un cliente con el mismo documento
            var existing = await _repository.FindByDocumentAsync(request.Document);

            if (existing is not null)
            {
                // notificamos la duplicidad y detenemos la ejecución
                await _outputPort.HandleDuplicateAsync(request.Document);
                return;
            }

            // el dominio valida las reglas - puede lanzar DomainException
            var customer = Customer.Create(
                request.FirstName,
                request.LastName,
                request.Document,
                request.Email,
                request.Phone,
                request.Address,
                request.BirthDate
            );

            // persistimos el nuevo cliente
            await _repository.AddAsync(customer);

            // construimos el DTO de respuesta y notificamos el éxito
            var response = new CreateCustomerResponse
            {
                CustomerId = customer.Id,

            };

            await _outputPort.HandleSuccessAsync(response);
        }
        catch (DomainException ex)
        {
            // cualquier excepción de dominio redirige al puerto de salida
            await _outputPort.HandleValidationErrorAsync(ex.Message);
        }
    }
}