# Registro de Casos de Uso (Use Cases)

Este documento es un "diario" o registro vivo de todas las acciones principales que tu sistema puede realizar. 
La idea es que cada vez que agreguen una nueva funcionalidad en clase (como Actualizar Cliente, Eliminar Cliente, o Buscar Cliente), vengas a este archivo y la documentes aquí. Esto te servirá como un catálogo rápido para saber qué hace tu aplicación.

---

## 1. Crear Cliente
- **Archivo:** `CreateCustomerUseCase.cs`
- **Carpeta:** `2. Application/Customers/UseCases`

### 🎯 Propósito
Registrar un nuevo cliente en el sistema, asegurándose de que el número de documento no exista previamente y de que todos los datos ingresados cumplan con las reglas estrictas de formato del negocio.

### 📥 Entrada (Lo que recibe)
Usa el DTO `CreateCustomerRequest` que contiene:
- `FirstName` (Nombre)
- `LastName` (Apellido)
- `Document` (DNI/Documento)
- `Email`
- `Phone` (Teléfono)
- `Address` (Dirección)
- `BirthDate` (Fecha de nacimiento)

### 📤 Salidas Posibles (Lo que comunica por el OutputPort)
El caso de uso nunca devuelve un `return` normal, sino que avisa por su "Walkie-Talkie" (`ICreateCustomerOutputPort`) uno de estos tres resultados:
1. `HandleSuccessAsync`: ¡Todo salió bien! Devuelve el ID del nuevo cliente generado.
2. `HandleDuplicateAsync`: Falla porque ya existe alguien en la base de datos con ese número de documento.
3. `HandleValidationErrorAsync`: Falla porque el Dominio detectó un formato inválido (ej. un email sin `@` o alguien menor de edad) y lanzó una `DomainException`.

### ⚙️ ¿Cómo funciona? (Lógica interna)
1. Llama a `_repository.FindByDocumentAsync` para verificar si el DNI ya está registrado. Si está, aborta y notifica el duplicado.
2. Si no existe, llama a `Customer.Create(...)` pasándole los datos crudos.
3. El Dominio hace toda la magia pesada (crea y valida los Value Objects).
4. Si el Dominio tiene éxito y no explota, la Aplicación llama a `_repository.AddAsync(customer)` para guardarlo en la base de datos.
5. Notifica el éxito absoluto al mundo exterior.

---

## 2. [Espacio Reservado para: Modificar Cliente]
*(Cuando implementen este caso de uso en clase, puedes copiar el formato de arriba y pegarlo aquí para documentar qué DTO usa de entrada, qué puertos de salida tiene y qué pasos sigue).*

---

## 3. [Espacio Reservado para: Eliminar Cliente]
*(Para futuros usos).*
