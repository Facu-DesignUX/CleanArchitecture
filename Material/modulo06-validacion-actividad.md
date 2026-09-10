Tecnicatura Superior en Desarrollo de Software

Módulo 06 — Validación de Modelos: IModelValidatorHub<T>

Actividad Práctica — CU-CLI-001: Alta de Cliente | Asignatura: Programación II | Instituto de Educación Superior N.º 9007 Dr. Salvador Calafat | Ciclo Lectivo 2026

## Contexto de la actividad

En el Módulo 05 integraste los Guards directamente en CreateCustomerUseCase : un Guard por campo, todos concatenados dentro del método ExecuteAsync . Esa solución funciona, pero mezcla la responsabilidad de validación con la orquestación del flujo de negocio.

En esta actividad extraés esa lógica a una clase dedicada: CreateCustomerValidatorHub . El UseCase queda reducido a una sola línea de validación y delega completamente en el hub.

Objetivo: Crear IModelValidatorHub<T> , implementar CreateCustomerValidatorHub , refactorizar CreateCustomerUseCase para usar el hub y registrar todo en DependencyContainer.cs .


## Archivos involucrados

## Parte A — Crear IModelValidatorHub<T>

La interfaz define el contrato genérico de validación. Vive en Common/Validatio n/ porque es compartida por todos los dominios de la aplicación. Un solo método: dado un modelo de tipo TModel , devolver todos los errores encontrados.

Ruta: CommerX.Application / Common / Validation / IModelValidatorHub.cs


## ¿Por qué IEnumerable y no List?

IEnumerable<T> es la abstracción más amplia para una secuencia. El llamador decide si necesita materializarla con .ToList() o simplemente iterarla. El hub no impone una estructura de colección concreta.

## Parte B — Crear CreateCustomerValidatorHub

La implementación concreta del hub para CreateCustomerRequest . Contiene un Guard por cada campo del DTO y combina todos los errores en una sola secuencia con .Concat() . No hay lógica de negocio aquí — solo precondiciones técnicas de formato y presencia.

Ruta: CommerX.Application / Customers / Validation / CreateCustomerValidatorHub.cs


Regla clave: El hub devuelve la secuencia sin materializar (sin llamar a .ToList() ). El UseCase la materializa una sola vez cuando necesita verificar errors.Count .

## Parte C — Refactorizar CreateCustomerUseCase

El UseCase del Módulo 05 contenía los Guards directamente en ExecuteAsyn c . Ahora se refactoriza para recibir el hub por inyección de dependencias y delegar en él toda la validación. El resultado es un método más limpio y con una sola responsabilidad claramente delimitada.

Ruta: CommerX.Application / Customers / UseCases / CreateCustomerUseCase.cs


```
15 // repositorio inyectado — contrato definido en Domain
16 private readonly ICustomerRepository _repository;
17 // puerto de salida inyectado — notifica el resultado al llamador
18 private readonly ICreateCustomerOutputPort _outputPort;
19 // hub de validación — valida las precondiciones técnicas del DTO
20 private readonly IModelValidatorHub<CreateCustomerRequest> _validator;
21
22 public CreateCustomerUseCase(
23 ICustomerRepository repository,
24 ICreateCustomerOutputPort outputPort,
25 IModelValidatorHub<CreateCustomerRequest> validator)
26 {
27 _repository = repository;
28 _outputPort = outputPort;
29 _validator = validator;
30 }
31
32 public async Task ExecuteAsync(CreateCustomerRequest request)
33 {
34 // PASO 1 — Guard Hub: precondiciones técnicas del DTO
35 var errors = _validator.Validate(request).ToList();
36 if (errors.Count > 0)
37 {
38 // notifica todos los errores juntos — el dominio no es invocado
39 await _outputPort.ValidationErrorsAsync(errors);
40 return;
41 }
42
43 try
44 {
45 // verificamos que no exista un cliente con el mismo documento
46 Customer? existing = await _repository.FindByDocumentAsync(request.Document);
47
48 if (existing is not null)
49 {
50 // notificamos la duplicidad y detenemos la ejecución
51 await _outputPort.HandleDuplicateAsync(request.Document);
52 return;
53 }
54
55 // PASO 2 — Dominio: el dominio valida las reglas — puede lanzar DomainException
56 var customer = Customer.Create(
57 request.FirstName,
58 request.LastName,
59 request.Document,
60 request.Email,
```


Cambio clave respecto al Módulo 05: Los Guards individuales por campo desaparecieron del UseCase. El campo _validator y la llamada a _validator.Validate(request).ToList() en la línea 35 reemplazan todo ese bloque. El resto del flujo (unicidad, dominio, persistencia, notificación) queda intacto.

## Parte D — Registrar el hub en DependencyContainer.cs

El UseCase depende de IModelValidatorHub<CreateCustomerRequest> — una interfaz. Para que el contenedor de DI pueda resolver esa dependencia, hay que indicarle qué clase concreta inyectar. El registro se agrega en el método de extensión existente de la capa Application.

Ruta: CommerX.Application / DependencyContainer.cs

1 // CommerX.Application/DependencyContainer.cs


## ¿Por qué Scoped y no Singleton?

El hub no tiene estado propio — cada llamada a Validate() trabaja con el request que recibe. Sin embargo se registra como Scoped para mantener coherencia de ciclo de vida con el UseCase: ambos nacen y mueren en el mismo contexto de operación. Si el hub en el futuro necesitara una dependencia Scoped (poco probable), ya estaría correctamente configurado.

## Parte E — Checklist de verificación

Antes de dar por completada la actividad, verificá que cada punto esté presente y correctamente implementado en tu proyecto.


| N.º | Archivo | Qué verificar |
| --- | --- | --- |
| 1 | IModelValidatorHub.cs | Interfaz genérica en Common/Validation/ . Un solo método Validate(TModel) que devuelve IEnumerable<ValidationError> . |
| 2 | CreateCustomerValidatorHub.cs | Implementa IModelValidatorHub<CreateCustomerRequest> . Un Guard por campo del DTO. Devuelve la secuencia con .Concat() sin llamar a .ToList() . |
| 3 | CreateCustomerUseCase.cs | Constructor recibe IModelValidatorHub<CreateCustomerRequest> como tercer parámetro. ExecuteAsync llama a _validator.Validate(request).ToList() antes de cualquier otra operación. No hay Guards individuales en el UseCase. |
| 4 | DependencyContainer.cs | AddScoped<IModelValidatorHub<CreateCustomerRequest>, CreateCustomerValidatorHub>() registrado en AddCustomerUseCases() . |
| 5 | Compilación | El proyecto CommerX.Application compila sin errores. No hay referencias a Guards individuales dentro de ExecuteAsync . |
| --- |

## Parte F — Preguntas de reflexión

- 1. ¿Por qué IModelValidatorHub<T> es un puerto y no una clase concreta?

- ¿Qué ventaja tiene que el UseCase dependa de la interfaz y no de CreateCustomerValidatorHub directamente?

- 2. ¿Qué sucede si olvidás registrar el hub en DependencyContainer.cs ? ¿En qué momento falla? ¿En tiempo de compilación o en tiempo de ejecución? ¿Cuál sería el mensaje de error?

- 3. ¿Por qué el hub devuelve IEnumerable y el UseCase llama a .ToList () ?

- ¿Qué pasaría si el hub llamara a .ToList() internamente y el UseCase también lo hiciera?

- 4. ¿Cuál es la diferencia entre validación técnica (hub) y validación de dominio (Value Object)?

Dá un ejemplo de cada una usando los campos de CreateCustomerRequest .


## 5. ¿Dónde se verifica la unicidad del documento y por qué no en el hub? ¿Qué principio de diseño justifica esa decisión?

- 6. Si en el futuro se agregara el campo SecondLastName al DTO, ¿qué archivos habría que modificar?

- ¿El UseCase debería cambiar? ¿Por qué sí o por qué no?

Instituto de Educación Superior N.º 9007 Dr. Salvador Calafat | Tecnicatura Superior en Desarrollo de Software | Asignatura: Programación II | Tema: Módulo 06 — Validación de Modelos — Actividad Práctica | Versión: 2026
