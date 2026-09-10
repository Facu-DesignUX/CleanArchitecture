// CommerX.Application/Common/Validation/Guard.cs
namespace CommerX.Application.Common.Validation;

public static class Guard
{
    public static GuardBuilderString Against(string value, string paramName)
        => new GuardBuilderString(value, paramName);
}
