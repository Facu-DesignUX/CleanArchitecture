// CommerX.Application/Common/Validation/GuardBuilderBase.cs
namespace CommerX.Application.Common.Validation;

public abstract class GuardBuilderBase<TBuilder> where TBuilder : GuardBuilderBase<TBuilder> //recibo a cualquiera que sea SOLAMENTE HEREDERO MIO
{
    protected readonly List<ValidationError> _errors = new();
    protected readonly string _paramName;

    protected GuardBuilderBase(string paramName)
    {
        _paramName = paramName;
    }

    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();
    public bool IsValid => _errors.Count == 0;
    //tener en cuenta que el uso de "=>" es omision de {}, ademas esta forma puede ser distinta, por ejemplo aplicando "return _errors.Count == 0 en donde ya se pase true o false nativamente"
    
    protected void AddError(string message)
    {
        _errors.Add(new ValidationError(_paramName, message));
    }
}
