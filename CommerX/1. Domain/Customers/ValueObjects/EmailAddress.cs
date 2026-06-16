using System.Text.RegularExpressions;
using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;

// Value Object que encapsula la validación de formato de email
public sealed record EmailAddress : ValueObject
{
    // Regex compilada: mejor rendimiento en llamadas repetidas
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static EmailAddress Create(string value)
    {
        // Fail Fast 1: nulo o vacío
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailException(value ?? string.Empty);

        // Normalización: sin espacios, todo en minúsculas
        var normalized = value.Trim().ToLowerInvariant();

        // Fail Fast 2: formato inválido excepción de dominio específica
        if (!EmailRegex.IsMatch(normalized))
            throw new InvalidEmailException(normalized);

        // Valor válido y normalizado: se construye el VO
        return new EmailAddress(normalized);
    }
}