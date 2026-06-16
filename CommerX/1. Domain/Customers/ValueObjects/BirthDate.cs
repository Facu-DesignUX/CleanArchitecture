using CommerX.Domain.Common.ValueObjects;
using CommerX.Domain.Customers.Exceptions;

namespace CommerX.Domain.Customers.ValueObjects;

// VO que encapsula la regla de mayoría de edad del dominio CU-CLI-001
public sealed record BirthDate : ValueObject
{
    // Constante: la regla de negocio está nombrada, no es un número mágico
    private const int MinimumAge = 18;

    public DateOnly Value { get; }

    private BirthDate(DateOnly value) => Value = value;

    public static BirthDate Create(DateOnly value)
    {
        // Fecha de hoy como referencia para calcular la edad
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Fail Fast: fecha futura es inválida como fecha de nacimiento
        if (value > today)
            throw new InvalidAgeException(value);

        // Cálculo base: diferencia de años
        var age = today.Year - value.Year;

        // Ajuste preciso: si el cumpleaños de este año aún no ocurrió
        if (value.AddYears(age) > today) age--;

        // Fail Fast: regla de mayoría de edad - CU-CLI-001
        if (age < MinimumAge)
            throw new InvalidAgeException(value);

        return new BirthDate(value);
    }
}