using System.Text.RegularExpressions;
using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;

public sealed record Phone : ValueObject
{
    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Phone Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPhoneException(value ?? string.Empty);

        var trimmed = value.Trim();

        // Solo dígitos, entre 7 y 15 caracteres
        if (!Regex.IsMatch(trimmed, @"^\d{8,18}$"))
            throw new InvalidPhoneException(trimmed);
        //Conrespecto a numeraciones en argentina se comienza desde 8 a 18 digitos
        return new Phone(trimmed);
    }
}
