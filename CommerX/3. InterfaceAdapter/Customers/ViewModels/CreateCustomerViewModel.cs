using System;
using System.Threading.Tasks;
using CommerX.Application.Customers.DTOs;
using CommerX.Application.Customers.Ports;
using CommerX.InterfaceAdapter.Customers.Presenters;

namespace CommerX.InterfaceAdapter.Customers.ViewModels;

// clase sellada — orquesta la llamada al UseCase, no valida reglas de negocio
public sealed class CreateCustomerViewModel
{
    private readonly ICreateCustomerInputPort _useCase;
    private readonly CreateCustomerPresenter _presenter;

    // propiedades bindables — una por campo del formulario CU-CLI-001
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // estado de resultado — lo lee la View después de GuardarAsync()
    public string Mensaje { get; private set; } = string.Empty;
    public bool EnCurso { get; private set; }

    // el contenedor de DI inyecta ambas dependencias desde el mismo scope de pantalla
    public CreateCustomerViewModel(
        ICreateCustomerInputPort useCase,
        CreateCustomerPresenter presenter)
    {
        _useCase = useCase;
        _presenter = presenter;
    }

    // comando que la View dispara desde el botón "Guardar"
    public async Task GuardarAsync()
    {
        EnCurso = true;

        try
        {
            // construye el Request a partir del estado bindable — DTO inmutable, object initializer
            var request = new CreateCustomerRequest
            {
                FirstName = FirstName,
                LastName = LastName,
                Document = Document,
                Email = Email,
                Phone = Phone,
                Address = Address,
                BirthDate = BirthDate
            };

            await _useCase.ExecuteAsync(request);

            // lee el Presenter — nunca el UseCase ni el OutputPort directamente
            Mensaje = _presenter.Result!.IsSuccess
                ? $"Cliente '{FirstName} {LastName}' registrado correctamente."
                : string.Join("; ", _presenter.Result.Errors);
        }
        finally
        {
            // se libera siempre, aunque el caso de uso falle: así el botón nunca queda deshabilitado
            EnCurso = false;
        }
    }
}
