# Capítulo 1: Conceptos de Clean Architecture

El corazón de Clean Architecture es la **Regla de Dependencia**: las dependencias siempre apuntan hacia adentro. Las reglas de negocio (el centro) no deben saber nada sobre bases de datos, consolas o interfaces web.

## 1. Domain (El Cerebro) `1. Domain`
Es el guardián de la verdad. No sabe de bases de datos ni de interfaces.
- **Entities (Entidades):** Ej. `Customer.cs`. Objetos con identidad propia (un ID). Representan los conceptos fundamentales de tu negocio.
- **Value Objects:** Ej. `EmailAddress.cs`, `FirstName.cs`. Bloques de construcción *inmutables*. Usan un **Static Factory Method** (`public static EmailAddress Create(...)`) para garantizar que es imposible crear datos inválidos (Patrón *Fail Fast*).
- **Exceptions:** Ej. `InvalidEmailException.cs`. Si un Value Object falla, lanza una bomba (excepción) que detiene todo.
- **Repositories (Interfaces):** Ej. `ICustomerRepository.cs`. El contrato de cómo se guarda/busca. ¡La implementación real de BD NO va aquí, solo el contrato!

## 2. Application (El Director de Orquesta) `2. Application`
Define el *Flujo de trabajo* (Workflow). No valida formatos (eso lo hace el Dominio), solo coordina.
- **Use Cases (Casos de Uso):** Ej. `CreateCustomerUseCase.cs`. Recibe datos, le pregunta a la BD, delega la creación al Dominio, y luego guarda en BD.
- **DTOs (Data Transfer Objects):** Ej. `CreateCustomerRequest`. "Cajas de cartón" sin lógica. Transportan los textos crudos de la consola hacia el Caso de Uso, para no exponer las Entidades.
- **Ports (Puertos):** 
  - *Input Port:* La interfaz que implementa tu Caso de Uso.
  - *Output Port:* El "Walkie-Talkie" del Caso de Uso. Ej. `await _outputPort.HandleValidationErrorAsync(ex.Message);`. Al Caso de Uso no le importa quién escucha del otro lado (Consola o Web), solo avisa lo que pasó.

## 3. Asincronismo y Tareas (`Task`, `async`, `await`)
En la capa de Aplicación verás mucha programación asíncrona:
- **`Task`**: Promete que un trabajo se terminará en el futuro.
- **`async/await`**: Permite que el programa no se congele mientras la base de datos responde. Ejemplo: `await _repository.AddAsync(customer)`.

*(Para más dudas de sintaxis, ve al Capítulo 3).*
