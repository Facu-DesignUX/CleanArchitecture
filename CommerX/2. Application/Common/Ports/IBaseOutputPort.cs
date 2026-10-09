using System.Collections.Generic;
using System.Threading.Tasks;
using CommerX.Application.Common.Validation;

namespace CommerX.Application.Common.Ports;

public interface IBaseOutputPort<T>
{
    Task HandleSuccessAsync(T response);
    Task ValidationErrorsAsync(IEnumerable<ValidationError> errors);
    Task HandleErrorAsync(string message);
}
