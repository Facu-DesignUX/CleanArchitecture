// CommerX.Application/Customers/UseCases/CreateCustomerUseCase.cs
using CommerX.Application.Common.Validation;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.Domain.Common.Exceptions;
using CommerX.Domain.Customers.Entities;
using CommerX.Domain.Customers.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace CommerX.Application.Customers.UseCases;

public sealed class CreateCustomerUseCase : ICreateCustomerInputPort
{
    private readonly ICustomerRepository _repository;
    private readonly ICreateCustomerOutputPort _outputPort;
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
        var errors = _validator.Validate(request).ToList();
        if (errors.Count > 0)
        {
            await _outputPort.ValidationErrorsAsync(errors);
            return;
        }

        try
        {
            var existing = await _repository.FindByDocumentAsync(request.Document);

            if (existing is not null)
            {
                await _outputPort.HandleDuplicateAsync(request.Document);
                return;
            }

            var customer = Customer.Create(
                request.FirstName,
                request.LastName,
                request.Document,
                request.Email,
                request.Phone,
                request.Address,
                request.BirthDate
            );

            await _repository.AddAsync(customer);

            var response = new CreateCustomerResponse
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
//oTRA FORMA. dECLARA CONSTRUCTOR PRIVADO Y FUNCUIN PUBLICA, 
//LA CUAL TRABAJA, 
//EFECTUA USO DE LOS VALUE OBJETS(SEGUNDA BARRERA TRAS LOS GUARDS)
//SIENTA EL CUSTOMER EN REPOSITY
//MANDA EL MENSAJE DE SUSCEFULL
//detalle, tambien hay parte dedicada que declara inicializados como null todos los atributos