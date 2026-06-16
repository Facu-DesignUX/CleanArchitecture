# Mejora de Diseño: HandleNotFoundAsync vs ValidationErrorsAsync

Este documento explica una mejora arquitectónica aplicada en el caso de uso `CU-CLI-002` (Actualización de Cliente), específicamente en la definición del Output Port (`IUpdateCustomerOutputPort`), respecto a lo que solicitaba originalmente el checklist de la actividad.

## 1. El Diseño Original (Según el Checklist)

La consigna original solicitaba que el Output Port de actualización contuviera los siguientes métodos:
- `HandleSuccessAsync`
- `HandleDuplicateAsync`
- `HandleValidationErrorAsync`
- `ValidationErrorsAsync`

### ¿Cómo hubiera funcionado este código?
Si hubiéramos implementado la interfaz de forma literal, se habría visto así:

```csharp
public interface IUpdateCustomerOutputPort
{
    Task HandleSuccessAsync(UpdateCustomerResponse response);
    Task HandleDuplicateAsync(string email);
    Task HandleValidationErrorAsync(string message);
    
    // Método solicitado en la actividad (redundante)
    Task ValidationErrorsAsync(IEnumerable<string> errors); 
}
```

En el caso de uso (`UpdateCustomerUseCase`), el primer paso es buscar al cliente en la base de datos por su ID usando el Repositorio. Si el cliente **no existe**, el código habría tenido que improvisar y forzar ese escenario dentro de un error de validación genérico, de esta forma:

```csharp
var customer = await _repository.FindByIdAsync(request.CustomerId);
if (customer is null)
{
    // Forzando el error de "No encontrado" como si fuera un error de validación
    await _outputPort.HandleValidationErrorAsync("El cliente no fue encontrado.");
    return;
}
```

## 2. El Problema del Diseño Original

El diseño propuesto por el checklist presentaba dos problemas conceptuales importantes:

1. **Redundancia:** Tener `HandleValidationErrorAsync` (para un solo error de texto) y `ValidationErrorsAsync` (para una lista de errores) en la misma interfaz suele ser innecesario en implementaciones donde las excepciones de dominio ya encapsulan los motivos de fallo (como un formato de email inválido).
2. **Falta de Semántica (El mayor problema):** Un error de validación de dominio (ej: *"El cliente debe ser mayor de 18 años"*) es conceptualmente distinto a un recurso inexistente (*"El cliente que quieres editar no existe"*). Al meter ambos escenarios en la misma bolsa (`HandleValidationErrorAsync`), la capa que consume este Output Port (la Presentación) pierde contexto y no tiene cómo diferenciarlos fácilmente.

## 3. La Mejora Implementada

Decidimos eliminar la redundancia de `ValidationErrorsAsync` y cambiarlo por un método con significado exacto para una operación de **Actualización**: `HandleNotFoundAsync`.

### Así quedó nuestro código:

```csharp
public interface IUpdateCustomerOutputPort
{
    Task HandleSuccessAsync(UpdateCustomerResponse response);
    Task HandleDuplicateAsync(string email);
    Task HandleValidationErrorAsync(string message);
    
    // Nuestra mejora: Un método específico para "No encontrado"
    Task HandleNotFoundAsync(Guid customerId);
}
```

En nuestro `UpdateCustomerUseCase`, el flujo ahora es limpio, expresivo y exacto a lo que está sucediendo:

```csharp
var customer = await _repository.FindByIdAsync(request.CustomerId);
if (customer is null)
{
    // Se notifica explícitamente a la Presentación que el recurso no existe
    await _outputPort.HandleNotFoundAsync(request.CustomerId);
    return;
}
```

## 4. ¿Por qué esto es una mejora arquitectónica clave?

1. **Claridad para la Capa de Presentación (APIs):** Si el día de mañana construyes una Web API (Controladores) que consuma este caso de uso, el "Presentador" sabrá exactamente qué código HTTP devolver según el método llamado:
   - Si se llama a `HandleValidationErrorAsync` -> Devuelve un HTTP 400 (Bad Request).
   - Si se llama a `HandleNotFoundAsync` -> Devuelve un HTTP 404 (Not Found).
   Con el modelo original del profesor, la API hubiera devuelto un 400 (Bad Request) incluso cuando el recurso simplemente no existía. Tener que parsear o adivinar strings (ej: `if (message == "No encontrado")`) es una muy mala práctica.

2. **Vocabulario Exacto:** En Clean Architecture, las interfaces deben narrar claramente los posibles caminos que puede tomar el sistema. Para un caso de uso de actualización, "No encontré el registro" es un resultado esperado y fundamental que merece su propia vía de salida en el Output Port.
