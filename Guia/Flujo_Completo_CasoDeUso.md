# Anatomía de un Flujo Completo en Clean Architecture

Para entender realmente cómo viaja la información en el proyecto, vamos a seguirle la pista a un dato desde que el usuario lo tipea en el teclado hasta que se guarda en la base de datos (o explota en el intento). 

Vamos a usar tu archivo `CreateCustomerUseCase.cs` como ejemplo. El escenario es: **"El usuario quiere registrar un nuevo cliente en el sistema"**.

---

## 🚦 Paso 1: El Origen (El Mundo Exterior / Consola)

Todo empieza afuera de la "cebolla" arquitectónica. Supongamos que estamos en `ConsoleTest` o en el controlador de una API Web. 
El usuario ingresa sus datos. La consola no sabe NADA sobre Entidades ni Value Objects. Solo maneja `strings` (texto) y números.

La consola "empaqueta" esos textos crudos en nuestro sobre mensajero: el **Input DTO**.

```csharp
// Esto pasa en la capa 3 (Console / Web)
var requestDTO = new CreateCustomerRequest 
{
    FirstName = "Facundo",
    LastName = "Perez",
    Document = "12345678",
    Email = "facundo@email.com",
    // ... otros datos primitivos
};

// Se llama al Caso de Uso y se le pasa el sobre
await useCase.ExecuteAsync(requestDTO);
```

---

## 🎬 Paso 2: Entra en Acción el Caso de Uso (La capa Application)

La ejecución viaja a `CreateCustomerUseCase.cs`. 
Aquí comienza el método principal: `ExecuteAsync(CreateCustomerRequest request)`.

Este archivo es el **Director de Orquesta**. No hace el trabajo sucio, pero le dice a los demás qué hacer. Veamos el bloque de código línea por línea y sus posibles "caminos".

### 1. ¿Ya existe este cliente? (Verificación rápida)

```csharp
// 1. Le pregunto al repositorio (que se conecta a la BD) si existe ese documento.
var existing = await _repository.FindByDocumentAsync(request.Document);

if (existing is not null)
{
    // CAMINO ALTERNATIVO A: DUPLICADO
    // Detengo todo. Le aviso al exterior por el "intercomunicador" (OutputPort)
    await _outputPort.HandleDuplicateAsync(request.Document);
    return; // <-- CORTA LA EJECUCIÓN AQUÍ.
}
```
* **¿Qué pasó aquí?** Si el cliente ya existía, el flujo muere rápido. No se creó ninguna Entidad. El `OutputPort` es como un parlante que le dice a la consola: *"Hey, muestra un mensaje de error rojo diciendo que está duplicado"*.

### 2. Pasar la pelota al Dominio (Crear Entidad y Value Objects)

Si pasó la primera prueba (el documento no existe), el flujo sigue.

```csharp
// El bloque try-catch está aquí porque el Dominio es súper estricto.
try
{
    // 2. Le paso los strings crudos del DTO a la fábrica de mi Entidad.
    var customer = Customer.Create(
        request.FirstName,
        request.LastName,
        request.Document,
        request.Email,
        // ...
    );
```
* **¿Qué pasa en `Customer.Create(...)`?** 
Aquí viajamos rápidamente al **Dominio**. `Customer.Create` va a intentar construir los **Value Objects** (`EmailAddress.Create(email)`, `Document.Create(document)`).
* **CAMINO ALTERNATIVO B: DATO INVÁLIDO ("Fail Fast")**
Supongamos que en el DTO llegó `request.Email = "facundosin_arroba.com"`. El Value Object `EmailAddress` lo revisa, se da cuenta que está mal, e inmediatamente lanza un `DomainException("El correo es inválido")`.
Al lanzar la excepción, el `Customer` no se termina de crear y el flujo salta automáticamente al bloque `catch` del Caso de Uso.

```csharp
}
catch (DomainException ex)
{
    // Atrapamos la rabieta del Dominio.
    // Usamos el intercomunicador de nuevo para avisar del error.
    await _outputPort.HandleValidationErrorAsync(ex.Message);
}
```

### 3. El Happy Path (Todo Salió Bien)

Si los datos eran perfectos, `Customer.Create` termina con éxito y nos devuelve un objeto `customer` inmaculado, validado y puro.

```csharp
    // 3. Persistimos (Guardamos en BD)
    await _repository.AddAsync(customer);
```
El Caso de Uso le da el `customer` al Repositorio. El repositorio traduce esa Entidad a código SQL (o JSON, etc.) y la guarda.

### 4. El Final del Viaje (El DTO de Respuesta)

El cliente ya se guardó. Ahora hay que decirle a la Consola que todo salió bien.
Pero **NUNCA** le devolvemos la Entidad `customer`. Hacemos un "mapeo" a un nuevo DTO:

```csharp
    // 4. Construimos el sobre de respuesta (Output DTO)
    var response = new CreateCustomerResponse
    {
        CustomerId = customer.Id,
        // (Aquí podríamos agregar más datos como FechaCreacion, NombreCompleto, etc.)
    };

    // 5. Usamos el intercomunicador para dar las buenas noticias
    await _outputPort.HandleSuccessAsync(response);
```

---

## 🗺️ Resumen de los 3 Caminos Posibles

En un Caso de Uso bien hecho en Clean Architecture, siempre tienes que tener controlados todos los escenarios. En este ejemplo vimos 3:

1. **Camino A (Lógico / Base de Datos):** El Caso de Uso le preguntó al Repositorio y descubrió un documento duplicado. Se abortó el flujo y se llamó a `HandleDuplicateAsync`.
2. **Camino B (Reglas de Negocio / Dominio):** El Caso de Uso mandó a crear la Entidad, pero un Value Object explotó porque un dato estaba mal formateado. Se abortó el flujo, cayó en el `catch`, y se llamó a `HandleValidationErrorAsync`.
3. **Camino C (Happy Path):** Nada explotó. La entidad se creó, se guardó en el repositorio, se empaquetó el ID en un `CreateCustomerResponse` (DTO) y se llamó a `HandleSuccessAsync`.

### El papel estelar de los Ports (Puertos)
Fíjate que el Caso de Uso **NUNCA** hace un `Console.WriteLine()` ni devuelve un código HTTP `200 OK`. Simplemente "toca un botón" en su intercomunicador (`_outputPort`). 
Es trabajo de la Capa de Infraestructura decidir si `HandleSuccessAsync` pinta la pantalla de verde o manda un correo de bienvenida. ¡Esa es la verdadera magia de tener el código desacoplado!
