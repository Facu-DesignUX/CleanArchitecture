# Explicación del Código: Módulo 5 — Guards y Validación de Precondiciones

Este documento explica los conceptos teóricos, patrones de diseño y el funcionamiento del código implementado para el Módulo 5.

## 1. Concepto Teórico: Guards vs. Reglas de Dominio

En Arquitectura Limpia, debemos separar dos tipos de validaciones:
1. **Precondiciones Técnicas (Guards):** Verifican que la información llegue en un formato básico aceptable. Ejemplos: que un texto no esté vacío, que tenga un largo máximo, o que un email tenga el formato correcto. No requieren conocer lógicas de negocio profundas.
2. **Invariantes de Dominio:** Verifican las verdaderas reglas del negocio. Ejemplos: que un cliente debe ser mayor de edad o que el formato del documento coincida con las leyes del país.

**El Módulo 5 se enfoca exclusivamente en los Guards (Precondiciones Técnicas).** Nos aseguramos de rechazar la basura *antes* de que llegue a nuestra capa de Dominio.

## 2. Patrones de Diseño Utilizados

### Fluent Builder
Permite construir o configurar un objeto encadenando métodos de forma legible (ej. `Guard.Against(valor).NotNull().MinLength(2)`).

### CRTP (Curiously Recurring Template Pattern)
Es un patrón de diseño avanzado donde una clase hereda de una clase genérica base, pasándose a sí misma como parámetro genérico. 
En nuestro código, lo vemos en:
```csharp
public abstract class GuardBuilderBase<TBuilder> where TBuilder : GuardBuilderBase<TBuilder>
```
**¿Para qué sirve?** Sirve para que los métodos de la clase base devuelvan siempre el tipo concreto de la clase hija. Así, al encadenar métodos, no perdemos los métodos específicos (por ejemplo, `InvalidEmail()`, que solo existe para *strings*).

## 3. Explicación de Clases y Funciones

### `ValidationError.cs`
- **¿Qué hace?:** Es un simple DTO (Data Transfer Object) inmutable.
- **Lógica:** Transporta el nombre del campo que falló (`PropertyName`) y el motivo del error (`Message`). No contiene ninguna lógica.

### `GuardBuilderBase<TBuilder>`
- **¿Qué hace?:** Es la clase abstracta de la que heredarán todos los validadores (ej. para *strings*, *guids*, *fechas*).
- **Lógica:** Contiene una lista interna `_errors`. Provee la propiedad `IsValid` (que es `true` si no hay errores) y un método protegido `AddError(...)` para ir acumulando fallas sin lanzar excepciones inmediatamente. Así podemos devolver todos los errores juntos.

### `GuardBuilderString.cs`
- **¿Qué hace?:** Es el builder concreto especializado en campos de texto (`string`).
- **Lógica:** Recibe el valor a validar. Contiene las funciones específicas de validación de texto:
  - `NotNullOrEmpty()`: Verifica si es nulo o espacios en blanco.
  - `MinLength(int)` / `MaxLength(int)`: Verifica la longitud de la cadena.
  - `InvalidEmail()`: Utiliza Expresiones Regulares (`Regex`) para asegurar el formato de correo electrónico.
  - **Retorno:** Cada método retorna `this` (la propia instancia), lo que permite encadenar múltiples llamadas (`.NotNullOrEmpty().MaxLength(50)`).

### `Guard.cs`
- **¿Qué hace?:** Clase estática que funciona como "Punto de Entrada" elegante.
- **Lógica:** Solo tiene métodos de fábrica. Al llamar a `Guard.Against(valor, nombre)`, por debajo crea una instancia del Builder correcto (`GuardBuilderString`), ocultando la palabra reservada `new` al desarrollador para que el código del consumidor se vea mucho más fluido y declarativo.
