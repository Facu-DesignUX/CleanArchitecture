using System.Collections.Generic;
using System.Linq;
using CommerX.Application.Common.Validation;
using CommerX.Application.Customers.DTOs;

namespace CommerX.Application.Customers.Validation;

public class UpdateCustomerValidatorHub : IModelValidatorHub<UpdateCustomerRequest>
{
    public IEnumerable<ValidationError> Validate(UpdateCustomerRequest model)
    {
        return Enumerable.Empty<ValidationError>()
            .Concat(Guard.Against(model.Email, nameof(model.Email)).NotNullOrEmpty().InvalidEmail().Errors)
            .Concat(Guard.Against(model.Phone, nameof(model.Phone)).NotNullOrEmpty().MaxLength(20).Errors)
            .Concat(Guard.Against(model.Address, nameof(model.Address)).NotNullOrEmpty().MaxLength(200).Errors);
    }
}
