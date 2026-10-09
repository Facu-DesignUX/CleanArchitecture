using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommerX.Application.Common.Ports;
using CommerX.Application.Common.Results;
using CommerX.Application.Common.Validation;

namespace CommerX.InterfaceAdapter.Common.Presenters;

public abstract class BasePresenter<T> : IBaseOutputPort<T>
{
    // Almacena el resultado para que luego la UI (ViewModel/Controller) lo consulte
    public OperationResult<T>? Result { get; protected set; }

    public virtual Task HandleSuccessAsync(T response)
    {
        Result = OperationResult<T>.Ok(response);
        return Task.CompletedTask;
    }

    public virtual Task ValidationErrorsAsync(IEnumerable<ValidationError> errors)
    {
        var errorList = new List<string>();
        foreach (var error in errors)
        {
            errorList.Add($"[{error.PropertyName}] {error.ErrorMessage}");
        }
        Result = OperationResult<T>.Fail(errorList);
        return Task.CompletedTask;
    }

    public virtual Task HandleErrorAsync(string message)
    {
        Result = OperationResult<T>.Fail(message);
        return Task.CompletedTask;
    }
}
