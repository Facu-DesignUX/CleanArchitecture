# Diagrama de Actividades: CU-CLI-001 (Alta de Cliente)

Este documento contiene el modelo en **PlantUML** del flujo de actividades para el caso de uso **CLI-001 (Alta de Cliente)**, detallando paso a paso la interacción entre las distintas capas de la **Clean Architecture** implementada en el proyecto `CommerX`.

Se incluyen los nombres de archivos involucrados, variables, propiedades, métodos y manejo de excepciones.

## Código PlantUML

Puedes copiar y pegar el siguiente bloque de código en cualquier visor de PlantUML o instalar la extensión correspondiente en tu IDE.

```plantuml
@startuml Diagrama_Actividad_CLI_001
skinparam style strictuml
skinparam DefaultFontName Inter, Arial
skinparam DefaultFontSize 12
skinparam ParticipantPadding 10
skinparam BoxPadding 10
skinparam WrapWidth 250
skinparam ActivityBackgroundColor #FEFECE
skinparam ActivityBorderColor #A80036

title "Activity Diagram: CU-CLI-001 Alta de Cliente"

|#F2F9FF| UI / Presentación |
|#FFFDF0| Application |
|#F0FFF0| Domain |
|#FFF0F5| Infrastructure / DB |

| UI / Presentación |
start
:El actor accede a "Alta de Cliente";
:El sistema muestra el formulario;
:El actor ingresa los datos requeridos:
* Nombre
* Apellido
* Documento
* Email
* Teléfono (opcional)
* Dirección (opcional)
* Fecha de Nacimiento;
:El actor confirma la operación (Ej: click en "Guardar");

:Mapeo de datos a **CreateCustomerRequest**;
note right
  **Archivo:** `CreateCustomerRequest.cs`
  **Capa:** Application / DTOs
  **Propiedades:**
  - string FirstName
  - string LastName
  - string Document
  - string Email
  - string Phone
  - string Address
  - DateOnly BirthDate
end note

:Invoca al puerto de entrada:
**ICreateCustomerInputPort.ExecuteAsync(request)**;

| Application |
:Inicia la ejecución en **CreateCustomerUseCase**;
note right
  **Archivo:** `CreateCustomerUseCase.cs`
  **Capa:** Application / UseCases
  **Variables inyectadas:**
  - ICustomerRepository _repository
  - ICreateCustomerOutputPort _outputPort
end note

:Verifica duplicidad de documento:
**await _repository.FindByDocumentAsync(request.Document)**;

| Infrastructure / DB |
:Consulta en la base de datos si existe el documento;
:Retorna entidad existente (**existing**) o **null**;

| Application |
if (¿existing no es nulo?) then (Sí, ya existe)
  :Llama al puerto de salida:
  **await _outputPort.HandleDuplicateAsync(request.Document)**;
  
  | UI / Presentación |
  :El presentador formatea la respuesta;
  :Muestra mensaje de error: "Cliente duplicado";
  stop
else (No, cliente nuevo)
  | Application |
  :Invoca creación de entidad de dominio:
  **Customer.Create(...)** 
  (pasando los parámetros de request);
  
  | Domain |
  :Inicia validaciones instanciando **Value Objects**;
  note right
    **Capa:** Domain / ValueObjects
    **Archivos y funciones (Fail Fast):**
    - FirstName.Create(request.FirstName)
    - LastName.Create(request.LastName)
    - Document.Create(request.Document)
    - EmailAddress.Create(request.Email)
    - Phone.Create(request.Phone)
    - Address.Create(request.Address)
    - BirthDate.Create(request.BirthDate)
  end note
  
  if (¿Las validaciones de Value Objects fallan?) then (Sí)
    :Lanza **DomainException**
    (Ej: `InvalidEmailException`, `InvalidAgeException`);
    
    | Application |
    :Bloque **catch (DomainException ex)** captura el error;
    :Llama al puerto de salida:
    **await _outputPort.HandleValidationErrorAsync(ex.Message)**;
    
    | UI / Presentación |
    :El presentador formatea la respuesta;
    :Muestra mensaje de error de validación;
    stop
  else (No, validaciones exitosas)
    | Domain |
    :Instancia y retorna la entidad **Customer**;
    note right
      **Archivo:** `Customer.cs`
      **Capa:** Domain / Entities
      **Propiedades asignadas:**
      - Guid Id (Generado aut.)
      - FirstName FirstName
      - LastName LastName
      - Document Document
      - EmailAddress Email
      - Phone Phone
      - Address Address
      - BirthDate BirthDate
    end note
    
    | Application |
    :Persiste el nuevo cliente:
    **await _repository.AddAsync(customer)**;
    
    | Infrastructure / DB |
    :Añade la entidad **Customer** a la Base de Datos;
    :Confirma persistencia (Ej. `SaveChangesAsync()`);
    
    | Application |
    :Construye la respuesta de éxito instanciando **CreateCustomerResponse**;
    note right
      **Archivo:** `CreateCustomerResponse.cs`
      **Capa:** Application / DTOs
      **Propiedad:**
      - Guid CustomerId (asignada como `customer.Id`)
    end note
    
    :Llama al puerto de salida notificando el éxito:
    **await _outputPort.HandleSuccessAsync(response)**;
    
    | UI / Presentación |
    :El presentador formatea la respuesta;
    :Sistema muestra mensaje de éxito con el ID generado;
    stop
  endif
endif

@enduml
```

## Resumen del Flujo por Capas

1. **UI / Presentación:** Responsable de capturar los datos del usuario, armar el DTO de solicitud (`CreateCustomerRequest`) y enviarlo al puerto de entrada (`ICreateCustomerInputPort`). También maneja los resultados del caso de uso a través del Output Port.
2. **Application (Aplicación):** Contiene el núcleo de orquestación (`CreateCustomerUseCase`). Llama al repositorio (abstracción) para comprobar si hay duplicados, luego llama al dominio para construir el modelo y, de no haber excepciones, ordena la persistencia y emite la respuesta (`CreateCustomerResponse`) mediante el Output Port.
3. **Domain (Dominio):** Centraliza la creación de los *Value Objects* (`FirstName`, `LastName`, `EmailAddress`, etc.), aplicando validaciones exhaustivas (Fail Fast). Si hay datos inválidos, arroja excepciones (`DomainException`). Si todo es correcto, crea la entidad `Customer` protegida por su Factory Method estático.
4. **Infrastructure / DB:** Es la implementación real del contrato `ICustomerRepository`, encargada de ir a la base de datos a revisar registros duplicados (`FindByDocumentAsync`) o guardar un nuevo registro (`AddAsync`).
