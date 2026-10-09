using System;
using System.Collections.Generic;

namespace CommerX.Application.Common.Results;

// clase sellada — no se puede heredar
public sealed class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }

    // constructor privado — solo Ok() y Fail() pueden crear instancias
    private OperationResult(bool isSuccess, T? value, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    // crea un resultado exitoso con el valor producido
    public static OperationResult<T> Ok(T value) 
        => new(true, value, Array.Empty<string>());

    // crea un resultado fallido con la lista de mensajes de error
    public static OperationResult<T> Fail(IReadOnlyList<string> errors) 
        => new(false, default, errors);

    // sobrecarga conveniente para un solo mensaje de error
    public static OperationResult<T> Fail(string error) 
        => new(false, default, new[] { error });
}
