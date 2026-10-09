// CommerX.Application/Common/Validation/Guard.cs
namespace CommerX.Application.Common.Validation;

public static class Guard
{
    public static GuardBuilderString Against(string value, string paramName)
        => new GuardBuilderString(value, paramName);
        //DATO UTIL PERO NO OBLIGATORIO DE IMPLEMENTAR
        //Plnateo agregando un int value, por ende tambien un guardbuilderinteger que contemple el guard dedicado a interger
        //Asi mismo en create customer se daria uso de esto. con validaciones llamadas donde pasamos los parametros y asi evaluamos y si hay fallas las colectamos en la lista
}
