# Explicación del Código: Módulo 6 — Validación de Modelos (Hub)

Este documento explica los conceptos teóricos, principios de diseño y el funcionamiento del código implementado para el Módulo 6.

## 1. Concepto Teórico: Desacoplamiento y Single Responsibility Principle (SRP)

En el Módulo 5, desarrollamos unos potentes Guards para validar datos. Sin embargo, si empezamos a colocar todos esos Guards directamente adentro del `CreateCustomerUseCase`, estaríamos violando el **Principio de Responsabilidad Única (SRP)**. El UseCase no debería saber *cómo* se validan técnicamente los campos, su única responsabilidad debe ser coordinar el flujo del negocio (Validar -> Construir Entidad -> Guardar).

**La Solución: El Validator Hub.**
Extraemos toda la lógica de validación técnica de los DTOs y la encapsulamos en una clase dedicada: el *Hub de Validación*. De este modo, el UseCase solo tiene que preguntarle al Hub: *"¿Es válido este DTO?"* y actuar en consecuencia.

## 2. Inyección de Dependencias y Principio de Inversión de Dependencias (DIP)

No instanciamos el Hub manualmente dentro del UseCase. Usamos el contenedor de inyección de dependencias de .NET para hacerlo.
Siguiendo el **Principio de Inversión de Dependencias (DIP)** (la "D" de SOLID), el UseCase no depende de la clase concreta `CreateCustomerValidatorHub`, sino de una abstracción: la interfaz `IModelValidatorHub<T>`. 

Ventajas:
- **Testabilidad:** Podemos probar el UseCase aislando la validación (usando un *mock* de la interfaz).
- **Mantenibilidad:** Si cambian las reglas técnicas del DTO, el código del UseCase no se toca en absoluto.

## 3. Explicación de Clases y Funciones

### `IModelValidatorHub<TModel>.cs`
- **¿Qué hace?:** Interfaz genérica que define el contrato de validación.
- **Lógica:** Expone un único método `Validate(TModel model)` que devuelve un `IEnumerable<ValidationError>`. Usar `IEnumerable` significa que devolvemos la secuencia de errores para que el que llama decida si quiere procesarla, contarla (`.ToList()`) o ignorarla.

### `CreateCustomerValidatorHub.cs`
- **¿Qué hace?:** Es la implementación concreta de la interfaz anterior para el DTO `CreateCustomerRequest`.
- **Lógica:** Implementa el método `Validate`. Toma cada propiedad del DTO (como `FirstName`, `Email`) y le aplica el `Guard.Against(...)` que creamos en el Módulo 5. Utiliza la función de LINQ `.Concat()` para unir todas las colecciones de errores individuales de cada campo en una única súper-lista secuencial de errores. Si todos los campos están correctos, devuelve una lista vacía.

### `CreateCustomerUseCase.cs` (Actualizado)
- **¿Qué hace?:** Se le inyecta el `IModelValidatorHub<CreateCustomerRequest>` en su constructor.
- **Lógica:** Al comenzar su función principal (`ExecuteAsync`), la primera instrucción es llamar al Hub y materializar los errores con `.ToList()`.
  - Si la cantidad de errores es mayor a cero (`Count > 0`), se llama al método `ValidationErrorsAsync` del puerto de salida (`_outputPort`) y se hace un `return` para abortar la operación. El dominio nunca es invocado con datos sucios.
  - Si no hay errores, la ejecución continúa fluyendo hacia el chequeo de duplicados y la capa de Dominio.

### `DependencyContainer.cs`
- **¿Qué hace?:** Es el pegamento arquitectónico.
- **Lógica:** Aquí le decimos a .NET: *"Cada vez que alguien pida un `IModelValidatorHub<CreateCustomerRequest>`, quiero que le entregues una instancia de `CreateCustomerValidatorHub`."* Se registra como **Scoped** porque tiene sentido que nazca y muera junto con el flujo del request actual.
