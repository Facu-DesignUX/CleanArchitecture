using CommerX.Domain.Common.Exceptions;

namespace CommerX.Domain.Customers.Exceptions;

public sealed class InvalidLastNameException : DomainException
{
    public InvalidLastNameException(string value)
        : base($"El apellido '{value}' no es válido. Debe tener entre 2 y 100 caracteres.") { }
}