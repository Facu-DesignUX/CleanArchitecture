// CommerX.Application/Common/Validation/GuardBuilderBase.cs
namespace CommerX.Application.Common.Validation;

public abstract class GuardBuilderBase<TBuilder> where TBuilder : GuardBuilderBase<TBuilder>
{
    protected readonly List<ValidationError> _errors = new();
    protected readonly string _paramName;

    protected GuardBuilderBase(string paramName)
    {
        _paramName = paramName;
    }

    public IReadOnlyList<ValidationError> Errors => _errors.AsReadOnly();
    public bool IsValid => _errors.Count == 0;

    protected void AddError(string message)
    {
        _errors.Add(new ValidationError(_paramName, message));
    }
}
