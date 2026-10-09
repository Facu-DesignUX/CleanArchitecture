// CommerX.Application/Common/Validation/IModelValidatorHub.cs
using System.Collections.Generic;

namespace CommerX.Application.Common.Validation;

public interface IModelValidatorHub<TModel>
{
    IEnumerable<ValidationError> Validate(TModel model);
}
