// CommerX.Application/Customers/UseCases/CreateCustomerUseCase.cs
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.Domain.Common.Exceptions;
using CommerX.Domain.Customers.Entities;
using CommerX.Domain.Customers.Repositories;

namespace CommerX.Application.Customers.UseCases;

// sealed: no puede heredarse - su comportamiento es definitivo
public sealed class CreateCustomerUseCase : ICreateCustomerInputPort
{
    // repositorio inyectado - contrato definido en Domain
    private readonly ICustomerRepository _repository;

    // puerto de salida inyectado - notifica el resultado al llamador
    private readonly ICreateCustomerOutputPort _outputPort;

    public CreateCustomerUseCase(
        ICustomerRepository repository,
        ICreateCustomerOutputPort outputPort)
    {
        _repository = repository;
        _outputPort = outputPort;
    }

    public async Task ExecuteAsync(CreateCustomerRequest request)
    {
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