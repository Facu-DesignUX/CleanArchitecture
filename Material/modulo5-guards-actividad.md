Tecnicatura Superior en Desarrollo de Software

## Módulo 06 — Guards: validación de precondiciones

Actividad Práctica — CU-CLI-001: Alta de Cliente | Asignatura: Programación II | Instituto de Educación Superior N.º 9007 Dr. Salvador Calafat | Ciclo Lectivo 2026

## Ejemplo práctico

En el Módulo 04 implementaste CreateCustomerUseCase con sus puertos e interfaces. En ese momento el UseCase solo capturaba DomainException si el dominio rechazaba un dato.

En esta actividad agregás la capa de validación de precondiciones técnicas: los Guards. Antes de que el dominio sea invocado, el UseCase verificará que los campos del DTO llegaron bien formados y devolverá todos los errores juntos si alguno falla.


## Archivos a crear o actualizar — estructura de proyectos

## Archivo 1 — CommerX.Application / Common / Validation / ValidationError.cs

## ¿Qué hace esta clase?

Transporta la información de un error de validación: qué campo falló y cuál es el mensaje. No es una excepción — se acumula en una lista y se devuelve al llamador juntos con los demás errores encontrados. Usa primary constructor de C# 12 para mantenerla inmutable y sin lógica.


Recordá: ValidationError es una clase simple — no un record ni una excepción. Los DTOs de este proyecto son clases; las excepciones se reservan para el dominio.

## Archivo 2 — CommerX.Application / Common / Validation / GuardBuilderBase.cs

## ¿Qué hace esta clase?

Clase abstracta genérica que implementa el patrón Fluent Builder con CRTP (Curiously Recurring Template Pattern). Acumula los errores internamente y los expone como solo lectura. Cada subclase concreta pasa su propio tipo como argumento genérico para que el encadenamiento devuelva siempre el tipo concreto.


## Archivo 3 — CommerX.Application / Common / Validation / GuardBuilderString.cs

## ¿Qué hace esta clase?

Builder concreto para campos de texto. Hereda de GuardBuilderBase<GuardBuilderString> — pasando su propio tipo como argumento genérico (CRTP) — para que cada método devuelva GuardBuilderString y el encadenamiento funcione sin perder el tipo. Expone cuatro reglas: NotNullOrEmpty , MinLength ,

MaxLength e InvalidEmail


## Archivo 4 — CommerX.Application / Common / Validation / Guard.cs

## ¿Qué hace esta clase?

Punto de entrada estático que crea el Builder correspondiente al tipo de dato recibido. El UseCase y el Hub interactúan solo con esta clase — nunca instancian los Builders directamente. Por ahora solo tiene la sobrecarga para string ; en módulos posteriores se agregarán sobrecargas para DateOnly y Guid .

```
1 // CommerX.Application/Common/Validation/Guard.cs
2 namespace CommerX.Application.Common.Validation;
3
4 // clase estática — no se instancia, es el punto de entrada del Guard
5 public static class Guard
6 {
7 // Against — "defenderse contra" un valor de texto inválido
8 public static GuardBuilderString Against(string value, string paramName)
9 => new GuardBuilderString(value, paramName);
10 }
```


## Archivo 5 — CommerX.Application / Common / Validation / IModelValidatorHub.cs

## ¿Qué hace esta interfaz?

Define el contrato que el UseCase usa para delegar toda la validación sin conocer qué Guards se aplican internamente. Un único método recibe el modelo y devuelve todos los errores encontrados — si la colección está vacía, el modelo es válido.

```
1 // CommerX.Application/Common/Validation/IModelValidatorHub.cs
2 namespace CommerX.Application.Common.Validation;
3
4 // puerto de validación — un método, una responsabilidad
5 public interface IModelValidatorHub<TModel>
6 {
7 // valida el modelo y devuelve todos los errores encontrados
8 // si la colección está vacía, el modelo es válido
9 IEnumerable<ValidationError> Validate(TModel model);
10 }
```

## Archivo 6 — CommerX.Application / Customers / Validation / CreateCustomerValidatorHub.cs

## ¿Qué hace esta clase?

Implementación concreta del hub para CreateCustomerRequest . Aplica un Guard por cada campo del DTO, combina todos los errores y los devuelve como una sola secuencia. El UseCase no sabe qué reglas se aplican — solo recibe la lista resultante.

| Campo Reglas Guard FullName NotNullOrEmpty · MinLength(2) · MaxLength(100) LastName NotNullOrEmpty · MinLength(2) · MaxLength(100) Document NotNullOrEmpty · MinLength(7) · MaxLength(8) |
| --- |
| Email NotNullOrEmpty · InvalidEmail() Phone NotNullOrEmpty · MaxLength(20) |


## Archivo 7 — CommerX.Application / Customers / CreateCustomerUseCase.

## cs — actualizar

## ¿Qué cambia?

El UseCase recibe el hub por inyección de dependencias y lo invoca al inicio de ExecuteAsync . Si hay errores de precondición, notifica al OutputPort y retorna inmediatamente — sin invocar al dominio. Solo si el DTO es válido, continúa con la creación de la entidad.

```
1 // CommerX.Application/Customers/CreateCustomerUseCase.cs
2 using CommerX.Application.Common.Validation;
3 using CommerX.Domain.Common.Exceptions;
4
5 namespace CommerX.Application.Customers;
6
7 public class CreateCustomerUseCase : ICreateCustomerInputPort
8 {
9 private readonly ICreateCustomerOutputPort _outputPort;
10 private readonly ICustomerRepository _repository;
11 private readonly IModelValidatorHub<CreateCustomerRequest> _validator;
12
13 // el hub se inyecta igual que el repositorio y el outputPort
14 public CreateCustomerUseCase(
15 ICreateCustomerOutputPort outputPort,
16 ICustomerRepository repository,
17 IModelValidatorHub<CreateCustomerRequest> validator)
18 {
19 _outputPort = outputPort;
20 _repository = repository;
21 _validator = validator;
22 }
23
24 public async Task ExecuteAsync(CreateCustomerRequest request)
25 {
26 // PASO 1 — Guard Hub: precondiciones técnicas del DTO
27 var errors = _validator.Validate(request).ToList();
28 if (errors.Count > 0)
29 {
30 // notifica todos los errores juntos — el dominio no es invocado
31 await _outputPort.ValidationErrorsAsync(errors);
32 return;
33 }
34
35 // PASO 2 — Dominio: construye la entidad con sus invariantes
```


## ¿Por qué Guard antes de Customer.Create()?

El Guard verifica precondiciones técnicas del DTO — que los campos llegaron y tienen formato mínimo aceptable. El dominio verifica invariantes de negocio — que la fecha de nacimiento implica mayoría de edad, que el documento tiene formato específico del país. Son dos capas distintas con responsabilidades distintas.


## Archivo 8 — CommerX.Application / DependencyContainer.cs — actualizar

## ¿Qué cambia?

genérica IModelValidatorHub<CreateCustomerRequest> , no de la clase concreta.

## Explicación paso a paso

La implementación se realiza en cuatro etapas ordenadas. Cada etapa debe compilar correctamente antes de pasar a la siguiente.


## Paso 1 — Crear los helpers de validación

Los archivos ValidationError , GuardBuilderBase , GuardBuilderString y Guard forman el núcleo reutilizable del sistema de validación. Deben existir antes que el hub porque el hub los usa. Todos viven en Common/Validation/ — no son específicos de ningún caso de uso.

## Paso 2 — Definir IModelValidatorHub<TModel>

La interfaz genérica desacopla el UseCase del hub concreto. El UseCase solo conoce IModelValidatorHub<CreateCustomerRequest> — no sabe qué Guards se aplican internamente. Si mañana se agregan o modifican reglas de precondición, el UseCase no necesita cambiar.

## Paso 3 — Implementar CreateCustomerValidatorHub

El hub concreto aplica un Guard por campo y concatena todos los errores en una sola secuencia. El UseCase llama a Validate() , recibe la lista completa y la entrega al OutputPort de una sola vez — el usuario ve todos los errores juntos, no uno por vez.

## Paso 4 — Actualizar UseCase y DependencyContainer

El UseCase agrega un tercer parámetro en el constructor. La validación ocurre antes del try que invoca al dominio: si hay errores, se notifica y se retorna. El DependencyContainer agrega un nuevo AddScoped registrando la interfaz genérica con la implementación concreta.

## Resumen de ubicaciones

| Archivo Proyecto Carpeta Tipo |
| --- |
| CommerX.Application | Common/Validation/ | class — DTO de error ValidationError.cs |


| CommerX.Application | Common/Validation/ | abstract class — CRTP base GuardBuilderBase.cs |
| CommerX.Application | Common/Validation/ | class — builder concreto para string GuardBuilderString.cs |
| CommerX.Application | Common/Validation/ | static class — punto de entrada Guard.cs |
| CommerX.Application | Common/Validation/ | interface — contrato genérico IModelValidatorHub.cs |
| CommerX.Application | Customers/Validation/ | class — implementación concreta CreateCustomerValidatorHub.cs |
| CommerX.Application | Customers/ | class — actualizar constructor y ExecuteAsync CreateCustomerUseCase.cs |
| CommerX.Application | raíz del proyecto | static class — agregar registro del hub DependencyContainer.cs |

## Buenas prácticas y errores comunes

## Guard vs. validación de dominio

## ERROR COMUN

El Guard verifica reglas de negocio que pertenecen al dominio — como si el email tiene el formato específico de la empresa o si el documento corresponde a un tipo válido del país.

```
1 // MAL: regla de negocio en el Guard
2 // los dígitos del DNI son invariante del dominio, no precondición técnica 3 Guard.Against(request.Document, "Document")
4 .OnlyDigits(); // incorrecto — esa lógica pertenece al dominio
```


## BUENA PRACTICA

El Guard verifica solo precondiciones técnicas: que el campo llegó, que tiene una longitud mínima aceptable. El dominio verifica las invariantes de negocio al construir la entidad.

```
1 // BIEN: solo precondición técnica — llegó y tiene longitud válida
2 Guard.Against(request.Document, "Document")
3 .NotNullOrEmpty().MinLength(7).MaxLength(8);
4 // el dominio verifica que sean dígitos al construir el Value Object
```

## Retorno después de ValidationErrorsAsync

## ERROR COMUN

Olvidar el return después de notificar los errores — el UseCase continúa ejecutando el dominio con datos inválidos.

```
1 // MAL: sin return, el dominio se ejecuta igual
2 if (errors.Count > 0)
3 await _outputPort.ValidationErrorsAsync(errors);
4 // falta return — Customer.Create() se invoca con datos inválidos
```

## BUENA PRACTICA

Notificar los errores y retornar inmediatamente — el dominio no es invocado si el DTO no pasó la validación de precondiciones.

```
1 // BIEN: notifica y retorna — el dominio no es invocado
2 if (errors.Count > 0)
3 {
4 await _outputPort.ValidationErrorsAsync(errors);
5 return;
6 }
```


## Registro del hub — interfaz vs. clase concreta

## ERROR COMUN

Registrar el hub por su clase concreta — el UseCase queda acoplado a la implementación y no puede testearse en aislamiento.

- 1 // MAL: registrado como clase concreta

- 2 services.AddScoped<CreateCustomerValidatorHub>();

## BUENA PRACTICA

Registrar la interfaz genérica con la implementación concreta — el contenedor inyectará la implementación correcta respetando el DIP.

```
1 // BIEN: interfaz → implementación concreta
2 services.AddScoped<
3 IModelValidatorHub<CreateCustomerRequest>,
4 CreateCustomerValidatorHub>();
```

## Actividad

Implementá de forma completa los Guards para el caso de uso CU-CLI-001 — Alta de Cliente en el proyecto CommerX.Application, siguiendo el orden de las partes. Compilar el proyecto después de cada parte antes de continuar.

## Parte A — ValidationError

- 1. Crear la carpeta Common/Validation/ dentro de CommerX.Application .

- 2. Declarar el namespace: CommerX.Application.Common.Validation .

- 3. Declarar public class ValidationError usando primary constructor de C# 12 con parámetros propertyName y message .

- 4. Exponer ambos parámetros como propiedades de solo lectura con => .

- 5. Verificar que la clase no tiene lógica, constructores adicionales ni herencia.


## Parte B — GuardBuilderBase<TBuilder>

- 1. En la misma carpeta, crear GuardBuilderBase.cs .

- 2. Declarar la clase abstracta genérica con el constraint CRTP: where TBuilder : GuardBuilderBase<TBuilder> .

- 3. Agregar el campo protected readonly List<ValidationError> _errors .

- 4. Agregar el campo protected readonly string _paramName .

- 5. Agregar el constructor protegido que inicializa _paramName .

- 6. Exponer Errors como IReadOnlyList<ValidationError> .

- 7. Exponer IsValid como bool verificando _errors.Count == 0 .

- 8. Agregar el método protegido AddError(string message) .

- 9. Compilar antes de continuar.

## Parte C — GuardBuilderString

- 1. Crear GuardBuilderString.cs en la misma carpeta.

- 2. Heredar de GuardBuilderBase<GuardBuilderString> — pasar el propio tipo.

- 3. Agregar el campo privado string _value .

- 4. Implementar el constructor que llama a base(paramName) e inicializa _value .

- 5. Implementar los cuatro métodos que devuelven GuardBuilderString :

- a. NotNullOrEmpty — usa string.IsNullOrWhiteSpace .

- b. MinLength(int min) — compara _value?.Length < min .

- c. MaxLength(int max) — compara _value?.Length > max .

- d. InvalidEmail — usa Regex para verificar formato.

- 6. Verificar que cada método termina con return this; .

- 7. Compilar antes de continuar.

## Parte D — Guard

- 1. Crear Guard.cs en la misma carpeta.

- 2. Declarar public static class Guard .

- 3. Implementar el método estático Against(string value, string paramName) que retorna new GuardBuilderString(value, paramName) .

- 4. Compilar antes de continuar.


Por ahora Guard solo tiene la sobrecarga para string porque todos los campos de CreateCustomerRequest son texto. En módulos posteriores se agregarán sobrecargas para DateOnly y Guid .

## Parte E — IModelValidatorHub<TModel>

- 1. Crear IModelValidatorHub.cs en la misma carpeta.

- 2. Declarar la interfaz genérica public interface IModelValidatorHub<TModel> .

- 3. Declarar un único método: IEnumerable<ValidationError> Validate(TModel model);

- 4. Compilar antes de continuar.

## Parte F — CreateCustomerValidatorHub

- 1. Crear la carpeta Customers/Validation/ dentro de CommerX.Application .

- 2. Namespace: CommerX.Application.Customers.Validation .

- 3. Agregar using CommerX.Application.Common.Validation;

- 4. Declarar public class CreateCustomerValidatorHub implementando IModelValidatorH ub<CreateCustomerRequest> .

- 5. Implementar el método Validate aplicando las reglas de la tabla: un Guard por campo, encadenando los métodos correspondientes.

- 6. Combinar todos los errores con .Concat() y retornar la secuencia.

- 7. Compilar antes de continuar.

## Parte G — Actualizar CreateCustomerUseCase

- 1. Abrir Customers/CreateCustomerUseCase.cs .

- 2. Agregar using CommerX.Application.Common.Validation;

- 3. Agregar el campo privado IModelValidatorHub<CreateCustomerRequest> _validator .

- 4. Agregar el parámetro validator al constructor y asignarlo a _validator .

- 5. Al inicio de ExecuteAsync , antes del try , agregar: a. Llamar a _validator.Validate(request).ToList() y guardar en errors . b. Si errors.Count > 0 , llamar a _outputPort.ValidationErrorsAsync(errors) y retornar.

- 6. Compilar antes de continuar.

## Parte H — Actualizar DependencyContainer

- 1. Abrir DependencyContainer.cs en la raíz de CommerX.Application .


- 2. Agregar los usings: using CommerX.Application.Common.Validation; y using CommerX.A pplication.Customers.Validation;

- 3. Dentro de AddCustomerUseCases , después del registro del UseCase, agregar el registro del hub:

- 1 // agrega inmediatamente después del registro del UseCase

- 2 services.AddScoped< 3 4 IModelValidatorHub<CreateCustomerRequest>,

- CreateCustomerValidatorHub>();

- 4. Compilar la solución completa — debe compilar sin errores ni warnings.

- 5. Verificar que el hub está registrado como Scoped y no como Singleton ni Tra nsient .

*Parte I — Checklist de entrega*

| Archivo Ubicación Verificación ValidationError.c Common/Validation/ Primary constructor — dos propiedades de |
| --- |
| s solo lectura GuardBuilderBase.c Common/Validation/ Clase abstracta genérica con CRTP y where s — AddError protegido GuardBuilderStrin Common/Validation/ NotNullOrEmpty · MinLength · MaxLength · g.cs InvalidEmail encadenables Guard.cs Common/Validation/ Clase estática — método Against para string IModelValidatorHu Common/Validation/ Interfaz genérica — un solo método b.cs Validate(TModel) CreateCustomerVali Customers/Validation/ 6 Guards — uno por campo — errores datorHub.cs combinados con Concat CreateCustomerUseC Customers/ Inyecta IModelValidatorHub — Guard antes ase.cs de Customer.Create() DependencyContaine raíz de Application Registra IModelValidatorHub como Scoped r.cs |


## Preguntas de reflexion

- 1. ¿Por qué el Guard verifica que Document tenga entre 7 y 8 caracteres pero no verifica que sean todos dígitos? ¿Qué capa es responsable de esa regla?

- 2. Si FullName llega vacío y Email tiene formato incorrecto, ¿cuántos Validation Error devuelve el hub en total? ¿Por qué?

- 3. ¿Qué pasaría si quitás el return después de await _outputPort.ValidationErrors Async(errors) ?

- 4. ¿Por qué IModelValidatorHub<T> se registra como Scoped y no como Singleto n ?

- 5. ¿Qué ventaja tiene que CreateCustomerUseCase dependa de IModelValidatorHub<Cr eateCustomerRequest> en lugar de CreateCustomerValidatorHub directamente?

Instituto de Educación Superior N.º 9007 Dr. Salvador Calafat | Tecnicatura Superior en Desarrollo de Software |

Asignatura: Programación II | Tema: Módulo 06 — Guards — Actividad Práctica | Ciclo Lectivo 2026
