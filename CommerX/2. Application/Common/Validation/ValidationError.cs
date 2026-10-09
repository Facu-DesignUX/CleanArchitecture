// CommerX.Application/Common/Validation/ValidationError.cs
namespace CommerX.Application.Common.Validation;

public class ValidationError(string propertyName, string message)
{
    public string PropertyName => propertyName;
    public string ErrorMessage => message;
}
