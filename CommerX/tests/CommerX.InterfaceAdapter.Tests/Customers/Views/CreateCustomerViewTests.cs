using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using CommerX.Domain.Customers.Entities;
using CommerX.Domain.Customers.Repositories;
using CommerX.InterfaceAdapter.Views.Customers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CommerX.InterfaceAdapter.Tests.Customers.Views;

// renderiza la View real (con el caso de uso real y un repositorio en memoria) y la maneja como lo haría un usuario
public sealed class CreateCustomerViewTests : BunitContext
{
    private sealed class InMemoryCustomerRepository : ICustomerRepository
    {
        public readonly List<Customer> Stored = new();

        public Task<bool> ExistsByDocumentAsync(string document)
            => Task.FromResult(Stored.Any(c => c.Document.Value == document));

        public Task<bool> ExistsByEmailAsync(string email)
            => Task.FromResult(Stored.Any(c => c.Email.Value == email));

        public Task<Customer?> FindByDocumentAsync(string document)
            => Task.FromResult(Stored.FirstOrDefault(c => c.Document.Value == document));

        public Task<Customer?> FindByEmailAsync(string email)
            => Task.FromResult(Stored.FirstOrDefault(c => c.Email.Value == email));

        public Task<Customer?> FindByIdAsync(Guid id)
            => Task.FromResult(Stored.FirstOrDefault(c => c.Id == id));

        public Task AddAsync(Customer customer)
        {
            Stored.Add(customer);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Customer customer) => Task.CompletedTask;
    }

    private readonly InMemoryCustomerRepository _repository = new();

    public CreateCustomerViewTests()
    {
        // la misma composición que armará el host: use case + presenter + view model
        Services.AddSingleton<ICustomerRepository>(_repository);
        Services.AddCustomerUseCases();
        Services.AddCustomerPresenter();
        Services.AddCustomerViewModel();
    }

    private static void FillForm(IRenderedComponent<CreateCustomer> view, string document = "12345678", string email = "juan@example.com")
    {
        // tras cada Change() el componente se vuelve a renderizar: hay que buscar el input de nuevo
        view.FindAll("input")[0].Change("Juan");
        view.FindAll("input")[1].Change("Pérez");
        view.FindAll("input")[2].Change(document);
        view.FindAll("input")[3].Change(email);
        view.FindAll("input")[4].Change("542614123456");
        view.FindAll("input")[5].Change("Av. San Martín 1000");
        view.FindAll("input")[6].Change("1990-01-01");
    }

    [Fact]
    public void Render_ShowsTheSevenFieldsAndTheSaveButton()
    {
        var view = Render<CreateCustomer>();

        Assert.Equal(7, view.FindAll("input").Count);
        Assert.Equal("Guardar", view.Find("button").TextContent.Trim());
        Assert.Equal(string.Empty, view.Find("p").TextContent.Trim());
    }

    [Fact]
    public void ClickingSave_WithValidData_RegistersTheCustomerAndShowsTheSuccessMessage()
    {
        var view = Render<CreateCustomer>();
        FillForm(view);

        view.Find("button").Click();

        Assert.Equal("Cliente 'Juan Pérez' registrado correctamente.", view.Find("p").TextContent.Trim());
        var stored = Assert.Single(_repository.Stored);
        Assert.Equal("12345678", stored.Document.Value);
        Assert.False(view.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void ClickingSave_WithAnUnderageBirthDate_ShowsTheDomainErrorAndStoresNothing()
    {
        var view = Render<CreateCustomer>();
        FillForm(view);
        view.FindAll("input")[6].Change(DateTime.Today.AddYears(-10).ToString("yyyy-MM-dd"));

        view.Find("button").Click();

        Assert.NotEqual(string.Empty, view.Find("p").TextContent.Trim());
        Assert.DoesNotContain("registrado correctamente", view.Find("p").TextContent);
        Assert.Empty(_repository.Stored);
    }

    [Fact]
    public void ClickingSave_WithEmptyFields_ShowsValidationMessagesAndStoresNothing()
    {
        var view = Render<CreateCustomer>();

        view.Find("button").Click();

        Assert.NotEqual(string.Empty, view.Find("p").TextContent.Trim());
        Assert.Empty(_repository.Stored);
    }

    [Fact]
    public void ClickingSave_TwiceWithTheSameDocument_ShowsTheDuplicateMessageTheSecondTime()
    {
        var view = Render<CreateCustomer>();
        FillForm(view);
        view.Find("button").Click();

        view.FindAll("input")[3].Change("otro@example.com");
        view.Find("button").Click();

        Assert.Contains("12345678", view.Find("p").TextContent);
        Assert.Single(_repository.Stored);
    }

    [Fact]
    public void EachRenderedView_OwnsItsOwnViewModel()
    {
        var first = Render<CreateCustomer>();
        var second = Render<CreateCustomer>();

        first.FindAll("input")[0].Change("Juan");

        Assert.Equal("Juan", first.FindAll("input")[0].GetAttribute("value"));
        Assert.True(string.IsNullOrEmpty(second.FindAll("input")[0].GetAttribute("value")));
    }
}
