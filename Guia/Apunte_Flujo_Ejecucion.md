# El Viaje del Dato: Flujo de Ejecución en CommerX

Los apuntes anteriores te explican "qué es cada cosa" y "cómo se escribe". Este apunte te explica **"cómo se mueve"** la información en tu aplicación de principio a fin. 

Entender este flujo es clave para saber en qué archivo debes escribir código cuando te pidan agregar una nueva función.

---

## El Caso Práctico: "Crear un Cliente"

Imagina que ya conectaste tu `ConsoleTest` a la capa de `Application` y un usuario presiona "Enter" en su teclado para registrar a "Juan Pérez". Así es como viaja esa información a través de tus capas:

### Paso 1: El Exterior (`ConsoleTest` / Interfaz de Usuario)
1. La consola le pide al usuario que escriba su Nombre, Apellido, Email, etc.
2. Todo eso es texto puro (`string`).
3. El `ConsoleTest` empaca todos esos textos "sucios" en una caja de cartón llamada **DTO** (`CreateCustomerRequest`).
4. La consola llama al **Director de Orquesta** pasándole esa caja: `await useCase.ExecuteAsync(request);`

### Paso 2: El Director Toma el Control (`Application` -> `CreateCustomerUseCase.cs`)
1. El Caso de Uso recibe el DTO.
2. **Primera Verificación:** Le pide al Repositorio (*"Oye Base de Datos, ¿tienes a alguien con este DNI?"*).
3. **Falla temprana:** Si el Repositorio dice "Sí", el Caso de Uso usa su "Walkie-Talkie" (El `OutputPort`) para avisar a la consola que hubo un error (`HandleDuplicateAsync`) y el proceso **termina aquí**.
4. **Continúa:** Si el Repositorio dice "No existe", el Caso de Uso avanza al siguiente paso. Pasa la pelota al Dominio. Llama a `Customer.Create(...)`.

### Paso 3: El Guardián Valida Todo (`Domain` -> `Customer.cs` y Value Objects)
1. El Dominio recibe los datos sucios que le mandó la Aplicación.
2. Antes de crear al cliente, intenta crear los **Value Objects** (`EmailAddress.Create(...)`, `FullName.Create(...)`).
3. **Validación estricta (Fail Fast):** Si el email no tiene un arroba `@`, el método de `EmailAddress` lanza una "bomba" (`throw new InvalidEmailException`). 
4. **Protección:** Esta bomba "vuela" hacia atrás, la Aplicación la atrapa (con el `catch`), y le avisa a la consola por el OutputPort (`HandleValidationErrorAsync`). El cliente nunca se crea.
5. **Éxito del Dominio:** Si todos los Value Objects se crean perfectamente, se ensambla la Entidad `Customer` (que ahora es 100% válida e indestructible) y se la devuelve a la Aplicación.

### Paso 4: Guardar la Verdad (`Application` coordina de nuevo)
1. El Caso de Uso recibe la Entidad `Customer` perfecta desde el Dominio.
2. Ahora llama nuevamente al Repositorio: `await _repository.AddAsync(customer)`.
3. *(Aquí es donde, en la vida real, Entity Framework toma esa entidad y hace un `INSERT INTO Clientes...` en SQL Server).*

### Paso 5: El Final Feliz (`Application` -> `ConsoleTest`)
1. El Caso de Uso toma el ID del cliente recién creado.
2. Lo empaca en otra caja de cartón de salida (DTO llamado `CreateCustomerResponse`).
3. Usa su Walkie-Talkie (`OutputPort`) para gritar victoria: `await _outputPort.HandleSuccessAsync(response)`.
4. La Consola, que estaba escuchando ese puerto, recibe el mensaje e imprime en pantalla: *"¡Cliente creado exitosamente con ID 1!"*.

---

## Resumen del Viaje (Diagrama Mental)

1. **Usuario** -> *(Texto)* -> **ConsoleTest**
2. **ConsoleTest** -> *(Caja DTO)* -> **Application (Caso de Uso)**
3. **Application** -> *(Datos crudos)* -> **Domain (Entidades/Value Objects)**
4. **Domain** -> *(Valida y crea)* -> **Entidad Customer Perfecta**
5. **Domain** -> *(Devuelve Entidad)* -> **Application**
6. **Application** -> *(Manda Entidad)* -> **Repositorio (Base de Datos)**
7. **Application** -> *(Caja DTO)* -> **Output Port (ConsoleTest)**
8. **ConsoleTest** -> *(Muestra mensaje)* -> **Usuario**

Entendiendo este circuito de 8 pasos, ¡podrás construir cualquier función (Crear, Editar, Borrar) sin perderte nunca entre las carpetas!
