using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;

public sealed record Address : ValueObject
{
    public string Value { get; }

    private Address(string value) => Value = value;

    public static Address Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidAddressException(value ?? string.Empty);

        var trimmed = value.Trim();

        // Longitud mínima 5 (ej: "Av. B") y máxima 200
        if (trimmed.Length < 5 || trimmed.Length > 200)
            throw new InvalidAddressException(trimmed);

        return new Address(trimmed);
    }
}
