# Revisión General de Fidelidad - Módulos 3 al 7

He realizado una revisión exhaustiva de todo el proyecto `CommerX` comparándolo estrictamente con los PDFs de la carpeta `Material Principal`. 

**Nota sobre la evolución del código:** Como bien notaste, el código de `CommerX` es vivo y evoluciona. Lo que se implementa en el Módulo 3 (`modulo-3b-excepciones-implementacion.pdf`) o en el Módulo 4, frecuentemente se refactoriza o se sobrescribe en los Módulos 5, 6 y 7. 
*Por ejemplo:* En el Módulo 4 se creó el `OutputPort` con 4 métodos, pero en el Módulo 7 se refactorizó para que herede de `IBaseOutputPort` y queden solo 2 métodos propios. 
**Esta revisión tiene eso en cuenta:** Las desviaciones marcadas abajo son errores estructurales que *nunca* se corrigieron en módulos posteriores, o bien, características de los últimos módulos que te faltó incorporar. No te preocupes, no vamos a sobreescribir código nuevo con código viejo.

---

## El Dilema del DTO: `CreateCustomerResponse` (Id vs. 3 Propiedades)

Planteaste una duda muy importante: *¿Qué es más lógico devolver al finalizar el caso de uso? ¿Solo el `CustomerId` o también `FirstName` y `LastName`?*

1. **La pureza arquitectónica (CQRS estricto): Solo el `Id`.**
   En un sistema estricto, un comando de creación (`CreateCustomer`) solo hace la acción y devuelve el identificador de lo que creó. Si necesitas más datos, hacés una consulta (Query) separada. Esta es la razón lógica por la que en tu código dejaste el comentario: *"Modificar y dejar solamente CustomerId como unico campo requerido..."*
2. **El pragmatismo del mundo real (y de la cátedra): Las 3 propiedades.**
   Para ahorrarle una petición extra al frontend (y poder mostrar un cartel *"Cliente Juan Perez creado"* inmediatamente), es muy común devolver un resumen. 
   **Lo que dicta el PDF:** En el Módulo 4 (Pág. 3) y en el Módulo 6 (Pág. 6), el profesor **exige explícitamente el uso de las 3 propiedades** (`CustomerId`, `FirstName`, `LastName`). 

> **Decisión Tomada:** Tras consultarlo con el profesor, optaste por el enfoque pragmático del PDF. Se implementarán las 3 propiedades (`CustomerId`, `FirstName`, `LastName`) en el `CreateCustomerResponse`.

---

A continuación detallo el plan definitivo de modificaciones, módulo por módulo:

## Módulo 4: Capa Application (Casos de Uso, Puertos y DTOs)

1. **`ICustomerRepository.cs` (Domain)**
   * **Actual:** Declara `FindByDocumentAsync`, `FindByIdAsync`, `FindByEmailAsync`, `AddAsync` y `UpdateAsync`.
   * **Problema:** En el Módulo 4 (Pág. 2) el profesor explica el Principio de Segregación de Interfaces (ISP) aplicándolo al repositorio. Pide usar métodos de existencia (`ExistsBy...` devolviendo `bool`) y no métodos de búsqueda que devuelven la entidad completa `Customer?` solo para validar unicidad:
     - `Task<bool> ExistsByDocumentAsync(string document);`
     - `Task<bool> ExistsByEmailAsync(string email);`
     - `Task AddAsync(Customer customer);`

2. **`DependencyContainer.cs` (Application)**
   * **Actual:** Usa `namespace CommerX.Application;` y el método `AddApplication`.
   * **Pide la práctica:** Debe usar el truco de namespace dictado: `namespace Microsoft.Extensions.DependencyInjection;` y el método debe llamarse `AddCustomerUseCases`. (Pág. 14 del PDF).

## Módulo 5: Guards (Precondiciones)

1. **`ValidationError.cs`**
   * **Actual:** Usa propiedades tradicionales y la propiedad se llama `Message`.
   * **Pide la práctica:** Debe usar el *primary constructor* de C# 12 y las propiedades deben ser tipo flecha (`=>`). El mensaje debe llamarse `ErrorMessage`:
     ```csharp
     public class ValidationError(string propertyName, string message) {
         public string PropertyName => propertyName;
         public string ErrorMessage => message;
     }
     ```

2. **`GuardBuilderString.cs`**
   * **Actual:** Los métodos (`NotNullOrEmpty`, `MinLength`, etc.) no aceptan argumentos opcionales y tienen mensajes fijos.
   * **Pide la práctica:** Cada método debe aceptar un parámetro opcional `string? mensaje = null` y formatear el mensaje dinámicamente con el nombre de la propiedad usando `_paramName`. (Pág. 4).

## Módulo 6: Validación de Modelos (Hubs)

1. **`CreateCustomerValidatorHub.cs`**
   * **Actual:** Retorna combinando con `Enumerable.Empty<ValidationError>().Concat(...)`.
   * **Pide la práctica:** Exige explícitamente guardar cada validación en variables locales (`var gFirstName = Guard.Against(...)`) y luego devolverlas encadenadas (`return gFirstName.Errors.Concat(gLastName.Errors)...`). (Pág. 6 y 7 del PDF).

2. **`CreateCustomerUseCase.cs` (Flujo de validación)**
   * **Actual:** Usa `FindByDocumentAsync(request.Document)` y no chequea la unicidad del email.
   * **Pide la práctica:** Tras validar con el Hub, el UseCase debe llamar a `await _repository.ExistsByDocumentAsync(...)` y también a `await _repository.ExistsByEmailAsync(...)` devolviendo booleano. (Pág. 6 del PDF).

## Módulo 7: InterfaceAdapter (Presenters)

1. **`ICreateCustomerOutputPort.cs`**
   * **Actual:** Solo declara `HandleDuplicateAsync`.
   * **Pide la práctica:** Tras refactorizar y heredar de `IBaseOutputPort`, el Módulo 7 pide tener dos canales específicos: `HandleDuplicateDocumentAsync(string document)` y `HandleDuplicateEmailAsync(string email)`.

2. **`CreateCustomerPresenter.cs`**
   * **Actual:** Solo implementa el duplicado genérico.
   * **Pide la práctica:** Implementar explícitamente `HandleDuplicateDocumentAsync` y `HandleDuplicateEmailAsync`, poblar el `_result` (de `BasePresenter`), y exponer la propiedad `Response`.

3. **`DependencyContainer.cs` (InterfaceAdapter)**
   * **Actual:** No existe.
   * **Pide la práctica:** Se debe crear en la capa `InterfaceAdapter` usando el *Factory Delegate* (`sp => sp.GetRequiredService<CreateCustomerPresenter>()`) para asegurar que el Scope comparta la misma instancia. (Pág. 8 y 9 del PDF).

---

### Próximos pasos
El diagnóstico está completo y es 100% consciente de la evolución cronológica del código del profesor. Cuando quieras, empezamos a arreglar las desviaciones archivo por archivo.
