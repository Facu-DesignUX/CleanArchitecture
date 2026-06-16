using CommerX.Domain.Common.Exceptions;

namespace CommerX.Domain.Customers.Exceptions;

public sealed class InvalidAgeException : DomainException
{
    // Recibe DateOnly para mostrar la fecha exacta que causó el error
    public InvalidAgeException(DateOnly birthDate)
        : base($"La fecha de nacimiento '{birthDate:dd/MM/yyyy}' no cumple el requisito de mayoría de edad (18 años).") { }
}