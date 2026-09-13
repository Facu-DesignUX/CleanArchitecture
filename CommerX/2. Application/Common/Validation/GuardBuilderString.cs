// CommerX.Application/Common/Validation/GuardBuilderString.cs
using System.Text.RegularExpressions;

namespace CommerX.Application.Common.Validation;

public class GuardBuilderString : GuardBuilderBase<GuardBuilderString>
{
    private readonly string _value;

    public GuardBuilderString(string value, string paramName) : base(paramName)
    {
        _value = value;
    }

    public GuardBuilderString NotNullOrEmpty()
    {
        if (string.IsNullOrWhiteSpace(_value))
        {
            AddError("El campo no puede ser nulo, vacío o contener solo espacios en blanco.");
        }
        return this;
    }
    //codigo visto en clase (COMPARAR Y BUSCAR PORQUE TERMINO ASI)
    // public GuardBuilderString NotNullOrEmpty(string? mensaje = null)
    //if (string.IsNullOrEmpty(_value))
    //    AddError(mensaje ?? $"El campo no puede ser nulo o vacío.");  

    public GuardBuilderString MinLength(int min)
    {
        if (_value?.Length < min)
        {
            AddError($"El campo debe tener al menos {min} caracteres de longitud.");
        }
        return this;
    }

    public GuardBuilderString MaxLength(int max)
    {
        if (_value?.Length > max)
        {
            AddError($"El campo no debe superar los {max} caracteres de longitud.");
        }
        return this;
    }

    public GuardBuilderString InvalidEmail()
    {
        if (_value is not null && !Regex.IsMatch(_value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            AddError("El campo debe ser una dirección de correo electrónico válida.");
        }
        return this;
        //En el caso de mail este puso ser un value objet
        //COMPARAR CON EXpRESION REGEX VISTA EN CLASE
        //
    }
}
