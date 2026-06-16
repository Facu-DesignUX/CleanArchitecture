# Diagrama de Actividades: CU-CLI-002 (Modificación de Cliente)

Este documento contiene el modelo en **PlantUML** del flujo de actividades para el caso de uso **CLI-002 (Modificación de Cliente)**, detallando paso a paso la interacción entre las distintas capas de la **Clean Architecture** implementada en el proyecto `CommerX`.

Se incluyen los nombres de archivos involucrados, variables, propiedades, métodos y manejo de excepciones.

## Código PlantUML

Puedes copiar y pegar el siguiente bloque de código en cualquier visor de PlantUML o instalar la extensión correspondiente en tu IDE.

```plantuml
@startuml Diagrama_Actividad_CLI_002
skinparam style strictuml
skinparam DefaultFontName Inter, Arial
skinparam DefaultFontSize 12
skinparam ParticipantPadding 10
skinparam BoxPadding 10
skinparam WrapWidth 250
skinparam ActivityBackgroundColor #FEFECE
skinparam ActivityBorderColor #A80036

title "Activity Diagram: CU-CLI-002 Modificación de Cliente"

|#F2F9FF| UI / Presentación |
|#FFFDF0| Application |
|#F0FFF0| Domain |
|#FFF0F5| Infrastructure / DB |

| UI / Presentación |
start
:El actor accede a "Editar Cliente";
:El sistema muestra el formulario precargado;
:El actor modifica los datos permitidos:
* Email
* Teléfono
* Dirección
* Fecha de Nacimiento;
:El actor confirma la operación (Ej: click en "Guardar");

:Mapeo de datos a **UpdateCustomerRequest**;
note right
  **Archivo:** `UpdateCustomerRequest.cs`
  **Capa:** Application / DTOs
  **Propiedades:**
  - Guid CustomerId
  - string Email
  - string Phone
  - string Address
  - DateOnly BirthDate
end note

:Invoca al puerto de entrada:
**IUpdateCustomerInputPort.ExecuteAsync(request)**;

| Application |
:Inicia la ejecución en **UpdateCustomerUseCase**;
note right
  **Archivo:** `UpdateCustomerUseCase.cs`
  **Capa:** Application / UseCases
  **Variables inyectadas:**
  - ICustomerRepository _repository
  - IUpdateCustomerOutputPort _outputPort
end note

:Verifica existencia del cliente:
**await _repository.FindByIdAsync(request.CustomerId)**;

| Infrastructure / DB |
:Consulta en la base de datos por el Id;
:Retorna entidad existente (**customer**) o **null**;

| Application |
if (¿customer es nulo?) then (Sí, no existe)
  :Llama al puerto de salida:
  **await _outputPort.HandleNotFoundAsync(request.CustomerId)**;
  
  | UI / Presentación |
  :El presentador formatea la respuesta;
  :Muestra mensaje de error: "Cliente no encontrado";
  stop
else (No, cliente encontrado)
  | Application |
  :Verifica si el nuevo email está en uso por otro cliente:
  **await _repository.FindByEmailAsync(request.Email)**;
  
  | Infrastructure / DB |
  :Consulta en la base de datos por el email;
  :Retorna entidad existente (**existingWithEmail**) o **null**;
  
  | Application |
  if (¿existingWithEmail no es nulo y el Id es diferente?) then (Sí, email en uso)
    :Llama al puerto de salida:
    **await _outputPort.HandleDuplicateAsync(request.Email)**;
    
    | UI / Presentación |
    :El presentador formatea la respuesta;
    :Muestra mensaje de error: "Email en uso";
    stop
  else (No, email disponible)
    | Application |
    if (¿Hay cambios reales en los datos?) then (No)
      :Construye respuesta sin cambios (noChangesResponse);
      :Llama al puerto de salida notificando éxito:
      **await _outputPort.HandleSuccessAsync(noChangesResponse)**;
      
      | UI / Presentación |
      :El presentador formatea la respuesta;
      :Muestra mensaje de éxito (sin realizar cambios en DB);
      stop
    else (Sí, hay cambios)
      | Application |
      :Invoca actualización en entidad de dominio:
      **customer.Update(...)** 
      (pasando los nuevos parámetros);
      
      | Domain |
      :Inicia validaciones instanciando **Value Objects**;
      note right
        **Archivo:** `Customer.cs`
        **Capa:** Domain / Entities
        **Validaciones (Fail Fast):**
        - EmailAddress.Create(email)
        - Phone.Create(phone)
        - Address.Create(address)
        - BirthDate.Create(birthDate)
      end note
      
      if (¿Las validaciones de Value Objects fallan?) then (Sí)
        :Lanza **DomainException**;
        
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
        :Asigna los nuevos Value Objects a las propiedades de la entidad;
        note right
          **Propiedades actualizadas:**
          - EmailAddress Email
          - Phone Phone
          - Address Address
          - BirthDate BirthDate
          *(FirstName, LastName y Document son inmutables)*
        end note
        
        | Application |
        :Persiste los cambios del cliente:
        **await _repository.UpdateAsync(customer)**;
        
        | Infrastructure / DB |
        :Actualiza la entidad **Customer** en la Base de Datos;
        :Confirma persistencia (Ej. `SaveChangesAsync()`);
        
        | Application |
        :Construye la respuesta de éxito instanciando **UpdateCustomerResponse**;
        note right
          **Archivo:** `UpdateCustomerResponse.cs`
          **Capa:** Application / DTOs
        end note
        
        :Llama al puerto de salida notificando el éxito:
        **await _outputPort.HandleSuccessAsync(response)**;
        
        | UI / Presentación |
        :El presentador formatea la respuesta;
        :Sistema muestra mensaje de éxito de modificación;
        stop
      endif
    endif
  endif
endif

@enduml
```

## Resumen del Flujo por Capas

1. **UI / Presentación:** Captura los datos del usuario a modificar, arma el DTO de solicitud (`UpdateCustomerRequest`) y lo envía al puerto de entrada (`IUpdateCustomerInputPort`). Recibe las respuestas del Output Port para mostrar el resultado al usuario.
2. **Application (Aplicación):** Contiene el núcleo de orquestación (`UpdateCustomerUseCase`). 
   - Llama al repositorio para validar la existencia del cliente (`FindByIdAsync`) y evitar duplicidad de emails (`FindByEmailAsync`).
   - Verifica si hubo cambios reales en los datos antes de proceder.
   - Llama a la entidad de dominio (`customer.Update()`) para aplicar los cambios y, si es exitoso, persiste los datos a través del repositorio (`UpdateAsync`) antes de devolver la respuesta (`UpdateCustomerResponse`).
3. **Domain (Dominio):** Realiza la validación a través de la creación de los *Value Objects* en el método `Update` de la entidad `Customer`. Si hay datos inválidos, lanza excepciones (`DomainException`). Solo si todo es correcto, asigna los nuevos *Value Objects* a las propiedades, respetando la inmutabilidad de la identidad del cliente (Nombre, Apellido, Documento).
4. **Infrastructure / DB:** Implementa el contrato `ICustomerRepository`, interactuando con la base de datos para buscar registros (`FindByIdAsync`, `FindByEmailAsync`) y actualizar un registro existente (`UpdateAsync`).
