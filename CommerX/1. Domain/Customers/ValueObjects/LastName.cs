using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;

public sealed record LastName : ValueObject
{
    public string Value { get; }

    private LastName(string value) => Value = value;

    public static LastName Create(string value)
    {
        // Fail Fast: vacío o nulo -> excepción de dominio
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidLastNameException(value ?? string.Empty);

        var trimmed = value.Trim();

        // Fail Fast: longitud fuera de rango -> excepción de dominio
        if (trimmed.Length < 2 || trimmed.Length > 100)
            throw new InvalidLastNameException(trimmed);

        return new LastName(trimmed);
    }
}