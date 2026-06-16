using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;

// sealed record: inmutable, igualdad por valor, no heredable
public sealed record FirstName : ValueObject
{
    // Propiedad de solo lectura: inmutabilidad total
    public string Value { get; }

    // Constructor privado: solo Create() puede instanciar
    private FirstName(string value) => Value = value;

    // Método factory: validación + normalización + construcción
    public static FirstName Create(string value)
    {
        // Fail Fast: vacío o nulo excepción inmediata
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidFirstNameException(value ?? string.Empty);

        // Normalización: eliminar espacios extremos
        var trimmed = value.Trim();

        // Fail Fast: longitud fuera de rango excepción de dominio
        if (trimmed.Length < 2 || trimmed.Length > 100)
            throw new InvalidFirstNameException(trimmed);

        // Solo llega aquí si el valor es completamente válido
        return new FirstName(trimmed);
    }
}