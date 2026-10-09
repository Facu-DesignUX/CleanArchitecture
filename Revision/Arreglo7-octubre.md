# Revisión y Observaciones - 7 de Octubre

## 1. Sobre el apunte en CreateCustomerUseCase
El apunte que dejaste al final de `CreateCustomerUseCase.cs`:
> // oTRA FORMA. dECLARA CONSTRUCTOR PRIVADO Y FUNCUIN PUBLICA, LA CUAL TRABAJA, EFECTUA USO DE LOS VALUE OBJETS (SEGUNDA BARRERA TRAS LOS GUARDS), SIENTA EL CUSTOMER EN REPOSITY, MANDA EL MENSAJE DE SUSCEFULL, detalle, tambien hay parte dedicada que declara inicializados como null todos los atributos

**Análisis:**
Este apunte no describe una "forma alternativa", sino que es el resumen exacto de la teoría aplicada sobre cómo interactúa el caso de uso con la entidad del Dominio (`Customer`) bajo los lineamientos de Clean Architecture y Domain-Driven Design:
*   **Constructor privado y función pública (Factory Method):** Se refiere a `private Customer() { }` y `public static Customer Create(...)` en la entidad `Customer`.
*   **Inicialización en null:** `public FirstName FirstName { get; private set; } = null!;` para evitar warnings del ORM (Entity Framework).
*   **Value Objects como segunda barrera:** La llamada a `FirstName.Create()`, `Document.Create()`, etc., dentro del Factory Method aseguran las invariantes de dominio tras haber pasado la primera validación técnica (Guards).
*   **UseCase:** Todo culmina cuando el UseCase guarda la entidad en el repo (`await _repository.AddAsync(customer);`) y notifica el resultado exitoso al puerto (`await _outputPort.HandleSuccessAsync(response);`).

**Conclusión:** Tu apunte es conceptualmente correcto y describe el diseño implementado por el profesor. No requiere que refactorices o cambies el caso de uso por este motivo.

---

## 2. Diferencias del proyecto frente a la Actividad Práctica del Módulo 7
Si nos apegamos **estrictamente a la actividad práctica obligatoria** del Módulo 7 (`modulo-07-interfaceadapter (presenters)-actividad.pdf`), hay ajustes puntuales que faltan realizar en el proyecto para ser 100% fieles al ejercicio:

### A. ICreateCustomerOutputPort.cs (Páginas 4 y 5 del PDF de práctica)
*   **Actualmente en el proyecto:** Tiene un único método de error por duplicado: `Task HandleDuplicateAsync(string document);`.
*   **Lo que exige la práctica:** Debe extender `IBaseOutputPort<CreateCustomerResponse>` y separar las validaciones de las reglas de unicidad (RN-001 y RN-003 del documento CU-CLI-001) en dos métodos diferentes:
    ```csharp
    public interface ICreateCustomerOutputPort : IBaseOutputPort<CreateCustomerResponse>
    {
        Task HandleDuplicateDocumentAsync(string document);
        Task HandleDuplicateEmailAsync(string email);
    }
    ```

### B. CreateCustomerUseCase.cs
*   **Actualmente en el proyecto:** Al verificar el documento duplicado llama a `await _outputPort.HandleDuplicateAsync(request.Document);`.
*   **Lo que exige la práctica:** Se debe corregir el nombre del método llamado para que sea `await _outputPort.HandleDuplicateDocumentAsync(request.Document);`. (Y opcionalmente, según se haya extendido, incluir la verificación del Email).

### C. CreateCustomerPresenter.cs (Páginas 7 y 8 del PDF de práctica)
*   **Actualmente en el proyecto:** Implementa `HandleDuplicateAsync(string document)`.
*   **Lo que exige la práctica:** Debe heredar explícitamente de `BasePresenter<CreateCustomerResponse>`, exponer la propiedad `Response` y completar los dos canales de error de unicidad guardando los mensajes de fallo en el `_result`:
    ```csharp
    public sealed class CreateCustomerPresenter : BasePresenter<CreateCustomerResponse>, ICreateCustomerOutputPort
    {
        public CreateCustomerResponse Response => _result?.Value ?? default!;

        public Task HandleDuplicateDocumentAsync(string document)
        {
            _result = OperationResult<CreateCustomerResponse>.Fail($"El documento {document} ya está registrado.");
            return Task.CompletedTask;
        }

        public Task HandleDuplicateEmailAsync(string email)
        {
            _result = OperationResult<CreateCustomerResponse>.Fail($"El email {email} ya está registrado.");
            return Task.CompletedTask;
        }
    }
    ```

### D. DependencyContainer.cs en InterfaceAdapter (Páginas 8 y 9 del PDF de práctica)
*   **Actualmente en el proyecto:** Faltó crear este archivo en la capa `CommerX.InterfaceAdapter`.
*   **Lo que exige la práctica (Parte E):** Crear este archivo para usar el Factory Delegate (que vimos en el Módulo 7). Esto asegura que el `CreateCustomerPresenter` sea la misma instancia compartida (`Scoped`) cuando lo pida el Controller y cuando lo use el UseCase:
    ```csharp
    namespace Microsoft.Extensions.DependencyInjection;

    public static class DependencyContainer
    {
        public static IServiceCollection AddCustomerPresenter(this IServiceCollection services)
        {
            services.AddScoped<CreateCustomerPresenter>();
            
            services.AddScoped<ICreateCustomerOutputPort>(
                sp => sp.GetRequiredService<CreateCustomerPresenter>());

            return services;
        }
    }
    ```

## 3. Próximos Pasos Recomendados
Para tener el proyecto idéntico a lo que solicitó la Actividad Práctica (y no salirnos del marco de lo estipulado por los PDFs obligatorios de "Material Principal"), sugiero aplicar las modificaciones A, B, C y D documentadas en el Punto 2.
