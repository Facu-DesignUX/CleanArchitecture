using System;
using System.Collections.Generic;
using System.Text;

namespace CommerX.Domain.Common.Exceptions
{
    public abstract class DomainException : Exception
    {
        // protected: solo accesible desde clases hijas
        // Recibe el mensaje y lo delega a Exception de .NET
        protected DomainException(string message) : base(message) { }
    }
}
