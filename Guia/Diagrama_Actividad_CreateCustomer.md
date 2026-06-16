# Diagrama de Flujo: Create Customer Use Case a través de las Capas

Este documento explica de forma visual y textual cómo la información viaja saltando entre las distintas capas (Presentación, Aplicación, Dominio e Infraestructura) durante el proceso de registrar un nuevo cliente.

## Explicación de la Danza entre Capas

1. **Capa Externa (Presentación/Consola):** El usuario escribe sus datos. La consola los agrupa en un "paquete" (Input DTO) que no tiene lógica. Se llama al "botón de inicio" del Caso de Uso (el Input Port).
2. **Capa de Aplicación (Caso de Uso):** Recibe el paquete. Su primera tarea es orquestar: le pide a la capa de Infraestructura que revise si el documento ya existe en la Base de Datos.
3. **Capa de Infraestructura (Repositorio):** Va a la base de datos y responde.
4. **Capa de Aplicación:** Si ya existe, usa un "intercomunicador" (Output Port) para decirle a la Presentación que aborte la misión y muestre un error. Si no existe, llama a la Capa de Dominio pasándole los datos crudos.
5. **Capa de Dominio (Entities/Value Objects):** Evalúa los datos con lupa. Si un Value Object detecta que el email no tiene arroba, explota (Fail Fast) lanzando un `DomainException`.
6. **Capa de Aplicación:** Si explotó, la atrapa y le dice a la Presentación (vía Output Port) que muestre un error de formato. Si todo salió bien en el Dominio, el Dominio devuelve un Objeto Cliente puro y válido.
7. **Capa de Aplicación:** Agarra ese Objeto Cliente válido y se lo entrega a Infraestructura para que lo guarde.
8. **Capa de Infraestructura:** Guarda los datos en la BD.
9. **Capa de Aplicación:** Genera un nuevo "paquete de respuesta" (Output DTO) para no revelar el Cliente entero, y le avisa a Presentación (vía Output Port) que todo salió bien.
10. **Capa Externa:** Muestra los fuegos artificiales de éxito al usuario.

---

## Código PlantUML (Diagrama de Actividad)

Puedes copiar y pegar este bloque de código en cualquier visor de PlantUML (como [PlantText](https://www.planttext.com/) o una extensión de VSCode) para generar un diagrama visual con "carriles" (swimlanes) que demuestran qué capa hace qué en cada momento.

```plantuml
@startuml
skinparam style strictuml
skinparam activity {
  BackgroundColor lightblue
  BorderColor darkblue
}

|Presentación (Consola / API)|
start
:1. Usuario ingresa datos crudos;
:2. Empaquetar datos en\n""CreateCustomerRequest"" (DTO);
:3. Llamar al Input Port\n""ExecuteAsync(dto)"";

|Aplicación (Caso de Uso)|
:4. Recibe el DTO;
:5. Llamar a Repositorio:\n""FindByDocumentAsync()"";

|Infraestructura (BD)|
:6. Consultar en la Base de Datos;

|Aplicación (Caso de Uso)|
if (¿El documento ya existe?) then (Sí)
    :7a. Llamar a Output Port:\n""HandleDuplicateAsync()"";
    |Presentación (Consola / API)|
    :8a. Mostrar al usuario:\n"Error: Documento duplicado";
    stop
else (No)
    |Aplicación (Caso de Uso)|
    :7b. Llamar a la Fábrica:\n""Customer.Create(datos...)"";

    |Dominio (Entidades y V.O.)|
    :8b. Instanciar Value Objects\ny validar (Fail Fast);
    
    if (¿Datos inválidos?) then (Sí)
        :9a. Lanzar ""DomainException"";
        |Aplicación (Caso de Uso)|
        :10a. Atrapar Excepción ""catch"";
        :11a. Llamar a Output Port:\n""HandleValidationErrorAsync()"";
        |Presentación (Consola / API)|
        :12a. Mostrar al usuario:\n"Error de Validación (Ej: Email sin @)";
        stop
    else (No)
        |Dominio (Entidades y V.O.)|
        :9b. Retornar Entidad\n""Customer"" inmaculada;
        
        |Aplicación (Caso de Uso)|
        :10b. Llamar a Repositorio:\n""AddAsync(customer)"";

        |Infraestructura (BD)|
        :11b. Insertar registro en BD;

        |Aplicación (Caso de Uso)|
        :12b. Empaquetar ID en\n""CreateCustomerResponse"" (DTO);
        :13. Llamar a Output Port:\n""HandleSuccessAsync(response)"";

        |Presentación (Consola / API)|
        :14. Mostrar al usuario:\n"¡Cliente creado con éxito!";
        stop
    endif
endif
@enduml
```
