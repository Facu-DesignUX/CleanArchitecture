using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommerX.Application.Common.Validation;
using CommerX.Application.Customers.DTOs;
using CommerX.InterfaceAdapter.Customers.Presenters;
using Xunit;

namespace CommerX.InterfaceAdapter.Tests.Customers.Presenters;

public sealed class UpdateCustomerPresenterTests
{
    [Fact]
    public async Task HandleSuccessAsync_SetsResultSuccess_AndExposesResponse()
    {
        var presenter = new UpdateCustomerPresenter();
        var response = new UpdateCustomerResponse
        {
            CustomerId = Guid.NewGuid(),
            Email = "juan.perez@example.com",
            Phone = "2614123456",
            Address = "Calle Falsa 123",
            BirthDate = new DateOnly(1995, 5, 20)
        };

        await presenter.HandleSuccessAsync(response);

        Assert.NotNull(presenter.Result);
        Assert.True(presenter.Result.IsSuccess);
        Assert.Equal(response.CustomerId, presenter.Response.CustomerId);
        Assert.Equal(response.Email, presenter.Response.Email);
        Assert.Equal(response.Phone, presenter.Response.Phone);
        Assert.Equal(response.Address, presenter.Response.Address);
        Assert.Equal(response.BirthDate, presenter.Response.BirthDate);
    }

    [Fact]
    public async Task HandleNotFoundAsync_SetsResultFailure_WithCustomerIdInErrors()
    {
        var presenter = new UpdateCustomerPresenter();
        var nonExistentId = Guid.NewGuid();

        await presenter.HandleNotFoundAsync(nonExistentId);

        Assert.NotNull(presenter.Result);
        Assert.False(presenter.Result.IsSuccess);
        Assert.Contains(presenter.Result.Errors, err => err.Contains(nonExistentId.ToString()));
    }

    [Fact]
    public async Task HandleDuplicateAsync_SetsResultFailure_WithEmailInErrors()
    {
        var presenter = new UpdateCustomerPresenter();
        const string duplicateEmail = "ya.existe@example.com";

        await presenter.HandleDuplicateAsync(duplicateEmail);

        Assert.NotNull(presenter.Result);
        Assert.False(presenter.Result.IsSuccess);
        Assert.Contains(presenter.Result.Errors, err => err.Contains(duplicateEmail));
    }

    [Fact]
    public async Task HandleErrorAsync_SetsResultFailure_WithDomainExceptionInErrors()
    {
        var presenter = new UpdateCustomerPresenter();
        const string domainError = "El customer debe ser mayor de 18 años.";

        await presenter.HandleErrorAsync(domainError);

        Assert.NotNull(presenter.Result);
        Assert.False(presenter.Result.IsSuccess);
        Assert.Contains(domainError, presenter.Result.Errors);
    }

    [Fact]
    public async Task ValidationErrorsAsync_SetsResultFailure_WithValidationErrors()
    {
        var presenter = new UpdateCustomerPresenter();
        var errors = new List<ValidationError>
        {
            new("Email", "El email no tiene un formato válido.")
        };

        await presenter.ValidationErrorsAsync(errors);

        Assert.NotNull(presenter.Result);
        Assert.False(presenter.Result.IsSuccess);
        Assert.Contains(presenter.Result.Errors, err => err.Contains("El email no tiene un formato válido."));
    }
}
