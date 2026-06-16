# Resumen de Repaso: Clean Architecture y su Modularidad

Este documento sirve como un mapa mental rápido para comprender cómo está estructurado el proyecto CommerX basándose en los principios de Clean Architecture. La analogía principal que usaremos es la del "Cerebro" y el "Director de Orquesta".

---

## 1. Domain (El Pilar y Cerebro del Sistema)

La capa de **Dominio (`CommerX.Domain`)** es la verdad absoluta de tu negocio. 
- **Qué es:** Es el "Cerebro". No sabe absolutamente nada sobre bases de datos, APIs, JSONs, ni páginas web. Es ciego al mundo exterior.
- **Responsabilidad:** Contiene las entidades (ej. `Customer`) y los cálculos, reglas y validaciones estrictas en forma de Value Objects (ej. `EmailAddress`, `BirthDate`).
- **Comportamiento:** Si un email no tiene un "@", el cerebro "estalla" (lanza una excepción de dominio). Si se intenta crear un cliente menor de edad, el cerebro frena la operación. Su único trabajo es asegurar que la información tenga sentido lógico y comercial.

---

## 2. Application (El Director de Orquesta)

La capa de **Aplicación (`CommerX.Application`)** es donde viven los Casos de Uso (Use Cases).
- **Qué es:** Es el "Director de Orquesta". El director no toca los instrumentos (no hace validaciones de negocio crudas como chequear la edad), sino que coordina a los músicos.
- **Responsabilidad:** Define el *flujo de trabajo* (Workflow) de la aplicación.
- **Ejemplo de flujo:**
  1. Le pide a la base de datos verificar si un email ya existe.
  2. Recolecta los datos y se los envía al "Cerebro" (Domain) para que los valide y cree/actualice la entidad.
  3. Si el Cerebro da luz verde, le ordena a la base de datos "¡Guarda esto!".
  4. Avisa a la salida del sistema que la operación terminó con éxito o si hubo errores.

---

## 3. Modulares por Naturaleza: Los Puertos (Ports)

El sistema es altamente modular gracias a los **Puertos (Input y Output Ports)**, que funcionan como cables o mediadores.
- **Input Ports:** Son la puerta de entrada (ej. `ICreateCustomerInputPort`). Le dicen al exterior *qué* se puede hacer (ej. `ExecuteAsync`).
- **Output Ports:** Son la vía de escape de los resultados. La capa de Aplicación no devuelve un "HTTP 200" o un "Error de consola". Simplemente usa su mediador y avisa: `await _outputPort.HandleValidationErrorAsync(ex.Message)`. Quien esté del otro lado escuchando (la consola, la web o una API) decidirá cómo dibujarlo en pantalla.

---

## 4. Comparación Práctica: Casos de Uso

### Caso de Uso 1: Alta de Cliente (`CreateCustomerUseCase`)
- **Acción:** Recibe todos los datos crudos del exterior a través de un DTO Request.
- **Llamada al Cerebro:** Envuelve la llamada a `Customer.Create(...)` en un bloque `try/catch`. 
- **Excepciones:** Si el dominio rechaza los datos por inválidos, lanza una `DomainException`. La Aplicación atrapa el error y se lo pasa al Output Port para ser manejado.

### Caso de Uso 2: Actualización de Cliente (`UpdateCustomerUseCase`)
- **Acción:** Es la misma estructura de orquestación, pero cambian los atributos (son menos propiedades, ya que `FirstName`, `LastName` y `Document` son inmutables según las reglas de negocio).
- **Llamada al Cerebro:** Invoca al método `Customer.Update(...)`.
- **Reutilización del Cerebro:** ¡AQUÍ ESTÁ LA MAGIA MODULAR! Aunque son menos campos, el método `Update` en el Dominio vuelve a instanciar internamente los Value Objects (ej. `EmailAddress.Create(email)`). Esto garantiza que las **mismas validaciones estrictas** que se usaron en la creación, se ejecuten automáticamente en la actualización. Es imposible corromper los datos por olvido, ya que se reutiliza el 100% del código de validación del Dominio.

---

**Conclusión:**
- **Domain =** El *Qué* y las reglas estrictas de negocio.
- **Application =** El *Cómo* se mueven los datos y se coordinan los pasos.
- **Ports =** Los conectores que permiten que todo sea modular e intercambiable sin tocar ni romper el núcleo del sistema.
