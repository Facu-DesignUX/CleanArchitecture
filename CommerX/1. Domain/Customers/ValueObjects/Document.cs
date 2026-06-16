using System.Text.RegularExpressions;
using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;
public sealed record Document : ValueObject
{
    public string Value { get; }

    private Document(string value) => Value = value;

    public static Document Create(string value)
    {
        // Fail Fast: vacío
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidDocumentException(value ?? string.Empty);

        var trimmed = value.Trim();

        // Reglas: longitud entre 7 y 15, y solo letras o dígitos
        if (trimmed.Length < 7 || trimmed.Length > 15 || !trimmed.All(char.IsLetterOrDigit))
            throw new InvalidDocumentException(trimmed);

        // Normalización CLAVE a mayúsculas
        return new Document(trimmed.ToUpper());
    }
}