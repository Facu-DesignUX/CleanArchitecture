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
        return Enumerable.Empty<ValidationError>()
            .Concat(Guard.Against(model.FirstName, nameof(model.FirstName)).NotNullOrEmpty().MinLength(2).MaxLength(100).Errors)
            .Concat(Guard.Against(model.LastName, nameof(model.LastName)).NotNullOrEmpty().MinLength(2).MaxLength(100).Errors)
            .Concat(Guard.Against(model.Document, nameof(model.Document)).NotNullOrEmpty().MinLength(7).MaxLength(8).Errors)
            .Concat(Guard.Against(model.Email, nameof(model.Email)).NotNullOrEmpty().InvalidEmail().Errors)
            .Concat(Guard.Against(model.Phone, nameof(model.Phone)).NotNullOrEmpty().MaxLength(20).Errors)
            .Concat(Guard.Against(model.Address, nameof(model.Address)).NotNullOrEmpty().MaxLength(200).Errors);
    }
}
