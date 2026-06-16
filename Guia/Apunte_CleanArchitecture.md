# Guía Práctica: Clean Architecture en tu Proyecto CommerX

Este documento es un resumen detallado de cómo funciona la arquitectura limpia (Clean Architecture) aplicada específicamente a tu código, para que puedas repasarlo cuando lo necesites.

## El Concepto Principal: La Regla de Dependencia
El corazón de Clean Architecture es que **las dependencias siempre apuntan hacia adentro**. 
Las reglas de negocio (el centro) no deben saber absolutamente nada sobre bases de datos, interfaces web, consolas o frameworks. Si mañana decides cambiar tu aplicación de consola por una página web, el núcleo de tu aplicación no sufrirá ni una sola modificación.

---

## Capa 1: El Centro (`1. Domain`)
Esta es la capa más importante. Es el guardián de la verdad y contiene las reglas de negocio puras. No tiene referencias a ningún otro proyecto de tu solución.

### ¿Qué hay adentro?
1. **Entities (Entidades):** Ej. `Customer.cs`. Son los objetos principales que tienen identidad propia (un ID). Representan los conceptos fundamentales de tu negocio.
2. **Value Objects (Objetos de Valor):** Ej. `EmailAddress.cs`, `Document.cs`.
   - Actúan como tus **"bloques de construcción irrompibles"**. Construyes tus Entidades usando estos bloques.
   - Heredan de un `record` base (`ValueObject.cs`), lo que los hace inmutables y comparables por su valor (por eso en tus pruebas `doc1 == doc2` funcionó).
   - Usan el patrón de diseño **Static Factory Method** (`public static EmailAddress Create(...)`) para garantizar que sea **imposible** crear un objeto con datos inválidos en tu sistema. Las validaciones de formato estricto ("Fail Fast") ocurren aquí.
3. **Exceptions:** Ej. `InvalidEmailException`. Los mensajes de error de negocio específicos nacen y mueren en esta capa.
4. **Repositories (Interfaces):** Ej. `ICustomerRepository.cs`. 
   - **Inversión de Dependencias:** El contrato de cómo se guarda o busca un cliente va en el Dominio, aunque el código real que se conecte a SQL no esté aquí. Al poner la interfaz aquí, obligas a la base de datos de afuera a adaptarse a tus reglas de negocio, y no al revés.

---

## Capa 2: El Puente o Director de Orquesta (`2. Application`)
*(Esta sección está ampliada para mayor claridad)*

Si el `Domain` tiene el conocimiento puro, la capa de `Application` es la que dice **qué hacer con ese conocimiento**. Actúa como un puente entre el mundo exterior (usuario/consola) y tus reglas internas. Depende exclusivamente del `Domain`.

**Regla de oro de Application:** No valida formatos (eso lo hacen los Value Objects del Dominio) ni guarda directamente en bases de datos. **Solo coordina.**

### ¿Qué hay adentro y cómo funciona?

1. **Use Cases (Casos de Uso):** Ej. `CreateCustomerUseCase.cs`. 
   - Representan **acciones específicas** que el usuario quiere realizar en tu sistema.
   - Es literalmente una "receta de cocina". Si miras tu código, el método `ExecuteAsync` hace exactamente esto paso a paso:
     1. **Coordina:** Le pregunta al Repositorio: *"¿Ya existe alguien con este documento?"*.
     2. **Decide flujo:** Si existe, no lanza un error técnico, sino que usa un Puerto para avisar: *"Oye exterior, detente, hay un duplicado"*.
     3. **Delega al Dominio:** Llama a `Customer.Create(...)`. Aquí le pasa la pelota al Dominio para que valide los datos y construya la Entidad real. Si falla, el Dominio lanza un `DomainException` que la Aplicación atrapa.
     4. **Guarda:** Si el Dominio devolvió un cliente válido, la Aplicación le dice al Repositorio: *"Guárdalo"*.
     5. **Notifica éxito:** Avisa al mundo exterior que todo salió bien.

2. **DTOs (Data Transfer Objects):** Ej. `CreateCustomerRequest`, `CreateCustomerResponse`.
   - Son literalmente "cajas de cartón" o "sobres" sin ninguna lógica o método adentro. 
   - **¿Por qué usarlos?** Se usan para recibir los datos de la Consola (o de internet) sin obligar a la Consola a crear tus complejos `ValueObjects` o `Entities`. Sirven para no "contaminar" ni exponer el Dominio. Entra un DTO sucio, sale un DTO limpio; las Entidades nunca salen de la Aplicación.

3. **Ports (Puertos):** Ej. `ICreateCustomerInputPort`, `ICreateCustomerOutputPort`.
   - Esta es una técnica avanzada para que la Aplicación sea **totalmente ciega** a quién la está usando.
   - **Input Port:** Es la interfaz que implementa tu Caso de Uso (la entrada al proceso).
   - **Output Port:** Es la forma en que el Caso de Uso "habla" hacia afuera. En lugar de hacer un `return` tradicional, tu Caso de Uso llama a `_outputPort.HandleDuplicateAsync()`. 
   - **¿La magia de esto?** Al Caso de Uso no le importa si ese Puerto está conectado a una Consola que imprime texto rojo, o a un controlador de una API web que devuelve un Error HTTP 400. La Aplicación solo grita el resultado, el exterior decide cómo mostrarlo.

4. **Aspectos Transversales (Lo que no ves pero suele ir aquí):**
   - Aunque la validación profunda va en el Dominio, a veces en Application se hacen validaciones "rápidas" (ej. comprobar que el DTO no venga completamente vacío) antes de pasarlo al Dominio para ahorrar tiempo.
   - También es el lugar donde, en el futuro, pondrás cosas como "iniciar una transacción de base de datos", "guardar un log de quién hizo la acción", etc.

---

## Capa 3: La Salida (`ConsoleTest` / Infraestructura)
Es tu punto de entrada actual y el único lugar que puede tocar el mundo exterior (pantalla, teclado).

### ¿Qué hace en tu proyecto?
- **Fase Actual (Playground):** Actualmente lo usas para testear directamente tu Dominio. Por ejemplo, forzando la creación de un `EmailAddress` malo para comprobar que tu regla de "Fail Fast" lanza la excepción correcta, o probando que la igualdad `doc1 == doc2` funciona gracias a tus Value Objects.
- **Fase Futura (Inyección de Dependencias):** Cuando conectes todo tu proyecto, `ConsoleTest` será el encargado de armar el rompecabezas. Deberá:
  1. Instanciar un Repositorio (quizás uno falso/Fake por ahora).
  2. Instanciar un Output Port (una clase en ConsoleTest que sepa imprimir en pantalla).
  3. Crear el Caso de Uso pasándole esas dos piezas.
  4. Pedir datos al usuario, meterlos en un DTO y llamar a `ExecuteAsync()`.

---

## Notas Adicionales
- **La carpeta `Domain` extra:** Esa carpeta sin el "1." que contiene un `obj` es simplemente basura residual generada por Visual Studio de un proyecto antiguo que borraste o renombraste. Puedes eliminarla con total seguridad de tu disco duro, no afecta a tu código.
