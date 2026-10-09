// CommerX.Application/Customers/Validation/CreateCustomerValidatorHub.cs
using System.Collections.Generic;
using System.Linq;
using CommerX.Application.Common.Validation;
using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Validation;

public class CreateCustomerValidatorHub : IModelValidatorHub<CreateCustomerRequest>
{
    public IEnumerable<ValidationError> Validate(CreateCustomerRequest model)
    {
        var gFirstName = Guard.Against(model.FirstName, "FirstName")
            .NotNullOrEmpty().MinLength(2).MaxLength(100);

        var gLastName = Guard.Against(model.LastName, "LastName")
            .NotNullOrEmpty().MinLength(2).MaxLength(100);

        var gDocument = Guard.Against(model.Document, "Document")
            .NotNullOrEmpty().MinLength(7).MaxLength(8);

        var gEmail = Guard.Against(model.Email, "Email")
            .NotNullOrEmpty().InvalidEmail();

        var gPhone = Guard.Against(model.Phone, "Phone")
            .NotNullOrEmpty().MaxLength(20);

        var gAddress = Guard.Against(model.Address, "Address")
            .NotNullOrEmpty().MaxLength(200);

        return gFirstName.Errors
            .Concat(gLastName.Errors)
            .Concat(gDocument.Errors)
            .Concat(gEmail.Errors)
            .Concat(gPhone.Errors)
            .Concat(gAddress.Errors);
    }
}
