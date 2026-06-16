# Guía de Sintaxis y Conceptos de C# en CommerX

Este documento es un glosario extenso diseñado para explicarte "en humano" todas las palabras clave, modificadores y estructuras que estamos usando en tu proyecto. Dado que estás construyendo una Clean Architecture, estás utilizando funciones muy modernas y avanzadas de C#.

---

## 1. Tipos de Estructuras (¿Dónde vive el código?)

### `class` (Clase Tradicional)
Es la forma clásica de definir un objeto en C#. Tiene propiedades y métodos. Por defecto, dos clases son iguales **solo si son exactamente el mismo espacio en memoria** (referencia).
```csharp
// Ejemplo: Tu Caso de Uso es una clase porque ejecuta acciones.
public class CreateCustomerUseCase { ... }
```

### `record` (El superpoder de tus Value Objects)
Es una característica moderna de C# (introducida en C# 9). Un `record` es como una clase, pero está diseñado para **guardar datos inmutables** (que no cambian una vez creados). 
La magia del `record` es que **se compara por valor**. Si tienes dos records con los mismos datos adentro, C# dirá que son iguales (por eso tu `doc1 == doc2` funcionó en el `ConsoleTest`).
```csharp
// Ejemplo: Tus Value Objects son records.
public record EmailAddress { ... }
```

### `interface` (Contratos)
Una interfaz **no tiene código ejecutable**. Es simplemente un "contrato" o una lista de promesas. Dice *qué* métodos deben existir, pero no *cómo* funcionan.
```csharp
// Promete que quien use esto debe tener un método "AddAsync".
public interface ICustomerRepository 
{
    Task AddAsync(Customer customer);
}
```

---

## 2. Modificadores de Acceso (¿Quién puede ver esto?)

### `public`
Cualquier archivo de cualquier proyecto puede acceder a esta clase, método o variable.
```csharp
public class Customer { ... } // Todo el mundo puede ver a la entidad Customer.
```

### `private`
**Solo el código que está dentro de las mismas llaves `{ }`** de esa clase puede verlo o usarlo.
```csharp
public class CreateCustomerUseCase 
{
    // Solo el caso de uso puede usar esta variable, nadie de afuera puede tocarla.
    private readonly ICustomerRepository _repository; 
}
```

---

## 3. Modificadores de Comportamiento (Las reglas del juego)

### `abstract` (La Plantilla)
Una clase `abstract` es una clase "incompleta" que **no se puede instanciar** (no puedes hacer `new ValueObject()`). Sirve únicamente como plantilla base para que otras hereden de ella.
```csharp
// Nadie puede crear un "ValueObject" genérico. Tienen que crear un EmailAddress o Address.
public abstract record ValueObject { ... } 
```

### `sealed` (El Punto Final)
Es lo contrario de `abstract`. Cuando pones `sealed`, le dices a C# que **nadie puede heredar** de esta clase. Es una medida de seguridad y optimización (el código corre más rápido porque el compilador sabe que esta clase es definitiva).
```csharp
// Nadie puede crear una clase "EmailCorporativo" que herede de EmailAddress.
public sealed record EmailAddress : ValueObject { ... }
```

### `static` (Lo que pertenece al "Molde", no a la galleta)
Normalmente, para usar una clase, tienes que crear un objeto (`new EmailAddress()`). Pero si un método o variable es `static`, pertenece a la clase en sí misma, no a una copia específica. Se puede usar sin hacer `new`.
```csharp
// Factory Method Estático: Lo llamas desde afuera como EmailAddress.Create("...")
public static EmailAddress Create(string value) { ... }
```

### `readonly` (Solo Lectura)
Una variable `readonly` **solo puede recibir un valor una vez** (usualmente dentro del constructor). Después de eso, nadie puede cambiar su valor. Es excelente para la seguridad.
```csharp
private readonly ICustomerRepository _repository; // Se asigna al nacer y jamás se cambia.
```

---

## 4. Asincronismo (Para que tu app no se quede congelada)

En aplicaciones modernas, no quieres que el programa se quede "congelado" esperando a que una base de datos lenta responda. Por eso usamos asincronismo.

### `Task` o `Task<T>`
Representa una **promesa** de que un trabajo se terminará en el futuro. Si la función devuelve algo, usas `Task<Tipo>`. Si no devuelve nada (un void clásico), usas solo `Task`.
```csharp
// Promete que en el futuro devolverá un Customer (o null si no lo encuentra).
Task<Customer?> FindByDocumentAsync(string document);
```

### `async` y `await`
Van siempre de la mano. `async` se pone en el nombre de la función para avisar que tendrá pausas. `await` se pone justo antes de llamar a una base de datos u otra función lenta.
El `await` le dice a C#: *"Oye, esto va a tardar. Ve a hacer otras cosas y cuando la base de datos responda, regresas aquí y sigues con la siguiente línea"*.
```csharp
public async Task ExecuteAsync(CreateCustomerRequest request)
{
    // C# se pausa aquí, libera la memoria, y cuando la BD responde, continúa.
    var existing = await _repository.FindByDocumentAsync(request.Document);
    // ... más código ...
}
```

---

## 5. Manejo de Errores (Las bombas y los escudos)

### `throw` (Lanzar la bomba)
Cuando algo sale mal y viola tus reglas de negocio, "lanzas" una excepción. Esto detiene inmediatamente el código y destruye todo hacia atrás hasta que alguien lo atrape.
```csharp
if (string.IsNullOrWhiteSpace(value))
{
    // ¡BOMBA! El código se detiene aquí al instante.
    throw new InvalidEmailException("El email no puede estar vacío");
}
```

### `try / catch` (El escudo)
Es como envuelves el código peligroso. Si una "bomba" (excepción) explota dentro del `try`, en lugar de cerrar el programa con un error rojo feo, el código salta inmediatamente al bloque `catch`, donde tú decides qué hacer (como imprimir un mensaje bonito o registrar el error).
```csharp
try 
{
    // Intentamos hacer algo peligroso que podría hacer "throw"
    var customer = Customer.Create(...); 
}
catch (DomainException ex)
{
    // Si explotó arriba, el programa no se cae, simplemente cae aquí.
    Console.WriteLine($"Hubo un error de negocio: {ex.Message}");
}
```

---

## 6. Constructores e Inyección de Dependencias

### El Constructor (`public NombreDeLaClase(...)`)
Es el código que se ejecuta en el instante en que haces `new NombreDeLaClase()`. 

### Inyección de Dependencias (DI)
Es el patrón que estás usando en tu `CreateCustomerUseCase`. En lugar de que el Caso de Uso cree su propia conexión a la base de datos (`new SqlConnection()`), **pide que alguien se la pase ya creada** a través de su constructor.
```csharp
public class CreateCustomerUseCase 
{
    private readonly ICustomerRepository _repository;

    // CONSTRUCTOR: Quien quiera usar este Caso de Uso, DEBE pasarme un Repositorio.
    // Yo no lo creo, yo lo "inyecto" desde afuera.
    public CreateCustomerUseCase(ICustomerRepository repository)
    {
        _repository = repository; // Lo guardo en mi variable readonly para usarlo después.
    }
}
```
Esto es lo que hace a tu código tan limpio: la clase no sabe cómo se fabricó el Repositorio, solo sabe usarlo.

---

## 7. Características de C# Moderno (El "Azúcar Sintáctico")

Dado que vienes de usar estructuras clásicas, tu código tiene mucha sintaxis moderna que C# agregó para escribir menos y hacer el código más limpio.

### `get; private set;` (Encapsulamiento de Propiedades)
En vez de crear una variable pública que cualquiera pueda arruinar, se usan propiedades. El `get` permite que cualquiera lea el valor (como `Customer.Email`), pero el `private set` prohíbe que alguien de afuera cambie su valor (`Customer.Email = "hacker@mal.com"` fallaría). Solo la propia clase puede modificarlo.
```csharp
// Todos pueden leer el FullName, pero solo Customer puede modificarlo internamente.
public FullName FullName { get; private set; } = null!;
```

### El Operador `!` (Null-Forgiving)
En C# moderno, el sistema es muy estricto con los nulos. Ese `= null!;` que ves arriba le dice al compilador: *"Confía en mí, sé que arranca en nulo pero te juro que le daré un valor antes de usarlo"*. Se usa mucho cuando conectas bases de datos (Entity Framework).

### La Flecha Gorda `=>` (Expression-bodied members)
Es simplemente una forma vaga (¡y genial!) de ahorrarse las llaves `{ }` y el `return` cuando una función tiene **una sola línea de código**.
```csharp
// Forma Clásica:
public void UpdateEmail(string newEmail) 
{
    Email = EmailAddress.Create(newEmail);
}

// Forma Moderna (La que usas en tu Customer.cs):
public void UpdateEmail(string newEmail) => Email = EmailAddress.Create(newEmail);
```

### `var` (Tipado Implícito)
En lugar de escribir `CreateCustomerResponse respuesta = new CreateCustomerResponse()`, puedes usar `var`. C# es inteligente y adivina el tipo basándose en lo que está a la derecha del igual. Esto hace el código mucho más fácil de leer.
```csharp
var existing = await _repository.FindByDocumentAsync(request.Document);
```

### `is not null` (Pattern Matching)
Antes se usaba `if (existing != null)`. Ahora C# introdujo `is not null`. Hace exactamente lo mismo, pero es más seguro bajo el capó y se lee mucho más como inglés o español normal.
```csharp
if (existing is not null)
{
    // El cliente ya existía...
}
```

### Namespaces de un solo archivo (File-scoped namespaces)
En C# antiguo, tenías que poner un `namespace { ... }` enorme que envolvía todo tu archivo y te robaba espacio en pantalla. Ahora, se pone una sola línea al principio con punto y coma.
```csharp
// Todo lo que esté en este archivo pertenece a esta "carpeta lógica".
namespace CommerX.Domain.Customers.Entities; 
```
