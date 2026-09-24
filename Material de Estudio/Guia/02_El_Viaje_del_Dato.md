# Capítulo 2: El Viaje del Dato (El Flujo Completo)

Vamos a seguirle la pista a un dato desde la consola hasta la base de datos usando `CreateCustomerUseCase.cs`.

## 🚦 Paso 1: El Origen (Consola / Exterior)
La consola empaqueta textos crudos en un DTO y se lo pasa al Caso de Uso.
```csharp
var requestDTO = new CreateCustomerRequest { FirstName = "Facundo", Document = "12345678" };
await useCase.ExecuteAsync(requestDTO);
```

## 🎬 Paso 2: El Director Toma el Control (Application)
En el Caso de Uso (`ExecuteAsync`):
1. **Verificación rápida:** ¿Existe el cliente?
```csharp
var existing = await _repository.FindByDocumentAsync(request.Document);
if (existing is not null) {
    // CAMINO A: DUPLICADO (Abortar y avisar)
    await _outputPort.HandleDuplicateAsync(request.Document);
    return;
}
```

2. **Delegar al Dominio (El Cerebro):**
```csharp
try {
    var customer = Customer.Create(request.FirstName, request.LastName, ...);
} catch (DomainException ex) {
    // CAMINO B: DATO INVÁLIDO ("Fail Fast")
    await _outputPort.HandleValidationErrorAsync(ex.Message);
    return;
}
```
*¿Qué pasa en `Customer.Create`?* El Dominio intenta crear los Value Objects (`FirstName.Create()`, etc.). Si algo está mal formateado, explota lanzando una excepción y cae en el `catch`.

3. **El Camino Feliz (Happy Path):**
```csharp
await _repository.AddAsync(customer); // Guardar
var response = new CreateCustomerResponse { CustomerId = customer.Id };
await _outputPort.HandleSuccessAsync(response); // Avisar éxito
```

## 💡 Mejora Arquitectónica en Actualizaciones (CU-002)
En lugar de tener puertos genéricos que confunden, los Puertos deben ser **semánticos**.
En `UpdateCustomerUseCase.cs`, si el cliente a editar no existe, en vez de mandar un "Error de validación genérico", creamos un puerto específico:
```csharp
var customer = await _repository.FindByIdAsync(request.CustomerId);
if (customer is null) {
    await _outputPort.HandleNotFoundAsync(request.CustomerId); // Mucho más claro
    return;
}
```
Esto permite que, el día de mañana, una Web API devuelva exactamente un `HTTP 404 Not Found` en lugar de un `HTTP 400 Bad Request`.
