# IES 9-007 Salvador Calafat



## Programación II - Tecnicatura Superior en Desarrollo de Software



### Anexo 07a - OperationResult

**Asignatura:** Programación II | **Instituto de Educación Superior N.º 9007 Dr. Salvador Calafat** | **Ciclo Lectivo 2026**

## ¿Qué es OperationResult?



**OperationResult**


*Objeto de resultado estandarizado | Capa: Application*

`OperationResult<T>` es un objeto que encapsula el resultado de una operación. En lugar de lanzar una excepción cuando algo falla o devolver null cuando no hay resultado la operación siempre devuelve un `OperationResult` que puede representar dos estados posibles: éxito con un valor de tipo `T`, o fallo con una lista de errores descriptivos.

Este patrón también conocido como Result Type o Discriminated Union en otros lenguajes hace que el flujo de la aplicación sea explícito: el llamador siempre sabe que debe verificar si la operación fue exitosa antes de usar el valor devuelto.

**Estados posibles:**

* `Ok(value)` exito con dato


* `Fail(errors)` fallo con errores


* `IsSuccess` indica el estado


* `Value` dato si exitoso


* `Errors` lista si fallido



---

# El problema que resuelve:



Cuando un UseCase termina ya sea con éxito o con un error esperado necesita comunicar ese resultado al OutputPort. Sin `OperationResult`, esa comunicación se hace con múltiples métodos separados, variables auxiliares o excepciones usadas como control de flujo.

`OperationResult<T>` unifica esa comunicación en un único objeto: el UseCase construye el resultado y lo pasa al OutputPort, que decide qué hacer según el estado.

# Analogía del mundo real



Imaginá que pedís un análisis de laboratorio. Unos días después recibís un sobre. Dentro del sobre puede haber dos cosas: el resultado con los valores del análisis, o una nota que dice "muestra insuficiente - repetir extracción".

En ambos casos recibís el mismo sobre la misma estructura. Antes de leer el resultado, mirás si hay una nota de error. Si no la hay, leés los valores. Si la hay, leés el motivo y tomás acción.

`OperationResult<T>` es ese sobre: siempre llega, siempre tiene la misma estructura, y el receptor sabe exactamente cómo manejarlo sin importar si trajo buenas o malas noticias.

**OperationResult - siempre el mismo sobre, dos contenidos posibles**

* **UseCase ejecuta la operacion**

* Produce: `OperationResult<T>` (`IsSuccess: bool`, `Value: T?`, `Errors: IReadOnlyList`) -> `Ok(value)` | `Fail(errors)`



* **OutputPort decide que hacer**

* Si `IsSuccess`: `Value` disponible -> `outputPort.HandleSuccessAsync(result)`

* Si no es `IsSuccess`: `Errors` disponibles -> `outputPort.HandleErrorAsync(result)`




---

# Características esenciales



**Estructura de la clase**

`OperationResult<T>` tiene constructor privado y solo se puede crear mediante los métodos estáticos `Ok()` y `Fail()`. Esto garantiza que nunca exista un resultado en estado indeterminado siempre es éxito con valor o fallo con errores.

```csharp
// clase sellada no se puede heredar
public sealed class OperationResult<T>
{
    // constructor privado solo Ok() y Fail() pueden crear instancias
    private OperationResult (bool isSuccess, T? value, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    // true si la operación terminó con éxito
    public bool IsSuccess { get; }

    // valor producido solo tiene sentido si IsSuccess es true 
    public T? Value { get; }

    // errores solo tienen sentido si IsSuccess es false 
    public IReadOnlyList<string> Errors { get; }

    // crea un resultado exitoso con el valor producido 
    public static OperationResult<T> Ok (T value)
        => new(true, value, Array.Empty<string>());

    // crea un resultado fallido con la lista de mensajes de error 
    public static OperationResult<T> Fail (IReadOnlyList<string> errors) 
        => new(false, default, errors);

    // sobrecarga conveniente para un solo mensaje de error 
    public static OperationResult<T> Fail(string error)
        => new(false, default, new() { error });
}

```
# Uso en un UseCase



El UseCase produce un `OperationResult` al final de su ejecución y lo pasa al OutputPort. El OutputPort inspecciona `IsSuccess` y decide cómo notificar al llamador.

```csharp
public async Task ExecuteAsync (RegisterBookRequest request)
{
    // paso 1 validación de precondiciones
    var errors = _validator.Validate(request).ToList();
    if (errors.Count > 0)
    {
        // construye resultado fallido y notifica
        var failed = OperationResult<RegisterBookResponse>.Fail (errors.Select(e => e.ErrorMessage).ToList());
        await _outputPort.HandleAsync (failed);
        return;
    }

    // paso 2 dominio y persistencia
    Book book = Book.Create(request.Title, request.Author);
    await _repository.AddAsync(book);

    // paso 3 construye resultado exitoso y notifica
    var response = new RegisterBookResponse { BookId = book.Id };
    var success = OperationResult<RegisterBookResponse>.Ok (response);
    await _outputPort.HandleAsync (success);
}

```

---

# Operation Result sin valor - operaciones void



Algunas operaciones no producen un valor de retorno solo indican si tuvieron éxito o fallaron. Para estos casos existe la versión no genérica `OperationResult` con un tipo marcador. La solución más simple es usar `OperationResult<bool>` donde el valor de éxito es simplemente true

```csharp
// operación que no produce un valor específico
// se usa bool como marcador true indica éxito
var result = OperationResult<bool>.Ok(true);

// o con un tipo Unit explícito si el proyecto lo define 
public readonly struct Unit { } // tipo vacío sin datos

var result = OperationResult<Unit>.Ok(new Unit());

```

---

# ¿Por qué se usa?



Cuando una operación puede fallar de forma esperada - validación, unicidad, regla de negocio - hay tres alternativas sin `OperationResult`

# Alternativas sin OperationResult - y sus problemas:



* **Excepción:** correcto para errores inesperados, pero incorrecto para flujos esperados. Usar excepciones como control de flujo es costoso en rendimiento y confunde al llamador.


* **Retornar null:** obliga al llamador a adivinar si null significa "no encontrado", "error" o "aún no disponible". No expresa la causa del fallo.


* **Variables auxiliares:** el método modifica variables externas o recibe parámetros out dificulta la lectura y el testeo del código.



`OperationResult<T>` resuelve los tres problemas: el fallo es explícito, la causa está incluida en el objeto y el llamador no puede ignorar el estado sin leer `IsSuccess`

# Beneficios concretos:



1. **Flujo explícito:** el llamador siempre verifica `IsSuccess` no puede olvidarse de manejar el error.


2. **Causa incluida:** `Errors` describe qué falló sin necesidad de capturar excepciones ni interpretar códigos.


3. **Testeable:** el test verifica el estado del resultado directamente sin capturar excepciones ni mockear efectos secundarios.



---

## ¿Cómo encaja en la arquitectura?



`OperationResult<T>` vive en la capa Application, en la carpeta `Common/`. Es compartido por todos los UseCases no tiene dependencias externas, solo tipos del propio lenguaje.

**OperationResult en el flujo completo del UseCase**

* **InterfaceAdapter:** ViewModel - llama a `ExecuteAsync(request)` y espera la notificacion del OutputPort


* **Application:** UseCase


1. `Hub.Validate()`

2. `Domain.Create()`

3. `OperationResult.Ok/Fail()`



* **Common/Results/:** `OperationResult<T>` (`IsSuccess` / `Value` / `Errors` | `Ok()` / `Fail()`)


* **Domain:** `Book.Create()` - lanza `DomainException` si falla el UseCase la captura y construye `OperationResult Fail()`

* **OutputPort:** recibe el `OperationResult` `HandleSuccessAsync`/`HandleErrorAsync` notifica al InterfaceAdapter



**Ubicacion en el proyecto:**


`CommerX.Application/Common/Results/OperationResult.cs`

Es el unico archivo sin interfaces ni implementaciones adicionales. Todos los UseCases lo usan directamente.
# Marco teórico



**Ejemplo completo ejecutable en Main()**

El siguiente ejemplo implementa `OperationResult<T>` completo y lo usa en un UseCase simulado. No requiere dependencias externas todo funciona en un programa de consola.

```csharp
// implementación completa de OperationResult
public sealed class OperationResult<T>
{
    private OperationResult (bool isSuccess, T? value, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }

    public static OperationResult<T> Ok(T v)
        => new(true, v, Array.Empty<string>());

    public static OperationResult<T> Fail(string e) 
        => new(false, default, new[] {e});
}

// simulacion de un servicio que puede fallar
static OperationResult<string> BuscarLibro (string isbn)
{
    // simula que el ISBN 978-0" no existe 
    if (isbn == "978-0")
        return OperationResult<string>.Fail("Libro no encontrado.");

    return OperationResult<string>.Ok($"Titulo para ISBN {isbn}");
}

// Main() el llamador siempre verifica IsSuccess
var r1 = BuscarLibro ("978-0");
if (!r1.IsSuccess)
    Console.WriteLine($"Error: {r1.Errors[0]}"); // Error: Libro no encontrado.

var r2 = BuscarLibro("978-3-16-148410-0");
if (r2.IsSuccess)
    Console.WriteLine($"Encontrado: {r2.Value}"); // Encontrado: Titulo para ISBN 978-3-...

```

---

# Errores comunes



## Error 1 acceder a Value sin verificar IsSuccess



Si la operación falló, `Value` es null o default. Acceder a sus miembros sin verificar `IsSuccess` primero produce una NullReferenceException

**ERROR COMUN**


Value usado directamente sin verificar IsSuccess.

```csharp
var result = BuscarLibro("isbn-invalido");

// MAL: si IsSuccess es false, Value es null 
Console.WriteLine(result.Value.Length); // NullReferenceException

```

**BUENA PRACTICA**


Siempre verificar IsSuccess antes de usar Value.

```csharp
var result = BuscarLibro("isbn-invalido");
if (!result.IsSuccess) return;

// a partir de aquí, Value está garantizado 
Console.WriteLine(result.Value!.Length);

```

---

## Error 2 usar OperationResult para errores inesperados



`OperationResult` es para flujos esperados fallos que el sistema anticipa y comunica al usuario. Los errores inesperados base de datos caída, timeout de red, bug de programación deben lanzar excepciones, no encapsularse en un `OperationResult`

**ERROR COMUN**


Excepción de infraestructura capturada y convertida en Fail().

```csharp
try
{
    await _repo.AddAsync(book);
    return OperationResult<bool>.Ok(true);
}
catch (SqlException ex)
{
    // MAL: error de infraestructura no es un flujo esperado
    return OperationResult<bool>.Fail(ex.Message);
}

```

**BUENA PRACTICA**


Errores inesperados propagan la excepcion el middleware global los maneja.

```csharp
// sin try/catch la excepción sube al middleware
await _repo.AddAsync(book);
return OperationResult<bool>.Ok(true);
// SqlException no es un flujo esperado el middleware la captura

```

---

# Qué NO hacer



**No usar OperationResult como contenedor de excepciones de dominio**

Las `DomainException` se lanzan desde los Value Objects cuando una invariante es violada. El UseCase las captura y las convierte en `OperationResult.Fail()` si corresponde comunicarlas al usuario. Pero el VO nunca debe retornar un `OperationResult` eso rompería su patrón Fail Fast.

**QUE NO HACER**


Value Object retorna OperationResult rompe Fail Fast y mezcla capas.

```csharp
// MAL: VO de dominio retorna OperationResult de Application 
public static OperationResult<BookTitle> Create(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        return OperationResult<BookTitle>.Fail ("Titulo invalido.");
    return OperationResult<BookTitle>.Ok (new BookTitle(value));
}

```

**BUENA PRACTICA**


VO lanza Domain Exception. El UseCase la captura y construye el Fail().

```csharp
// VO Fail Fast con DomainException
public static BookTitle Create(string value)
{
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidBookTitleException (value);
    return new BookTitle (value);
}
// UseCase captura y convierte si corresponde comunicarlo

```

---

# Resumen



reglas de OperationResult:

* Solo para flujos esperados no para errores de infraestructura.


* Siempre verificar IsSuccess antes de acceder a Value


* Los Value Objects usan DomainException nunca retornan Operation Result.


* El UseCase construye el resultado el OutputPort decide cómo comunicarlo.