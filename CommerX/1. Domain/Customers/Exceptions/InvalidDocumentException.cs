using CommerX.Domain.Common.Exceptions;

namespace CommerX.Domain.Customers.Exceptions;

public sealed class InvalidDocumentException : DomainException
{
    // Documento: solo dígitos, entre 7 y 15 caracteres
    public InvalidDocumentException(string value)
        : base($"El documento '{value}' no es válido. Solo dígitos, entre 7 y 15 caracteres.")
    { }
}
