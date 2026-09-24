# Capítulo 4: Mapa del Proyecto (¿Para qué sirve cada archivo?)

Como estudiante, a veces abres la solución y ves decenas de archivos `.cs`. Esta guía es un "mapa" exacto de la estructura actual de **CommerX**. Te explica carpeta por carpeta y archivo por archivo qué responsabilidad tienen.

## 📁 1. Domain (El Cerebro)
Contiene las reglas de negocio, y **no depende de nada más**.

### 📂 Common
Aquí va el código genérico que se reutilizará en todo el sistema (no solo para Clientes, sino para futuros Módulos como Productos, Ventas, etc.).
- **`Entities/BaseEntity.cs`**: La plantilla base para todas las entidades. Suele contener el `Id` genérico y métodos compartidos, para que no tengas que repetir `public Guid Id { get; }` en cada clase nueva.
- **`Exceptions/DomainException.cs`**: La clase madre de todos los errores de negocio. Si explota una regla, siempre será un `DomainException`.
- **`ValueObjects/ValueObject.cs`**: La clase base (el `abstract record`) que le da el superpoder a todos tus Value Objects para que puedan compararse por sus valores internos en lugar de por su espacio en memoria.

### 📂 Customers
Todo lo relacionado específicamente a la entidad "Cliente".
- **`Entities/Customer.cs`**: El protagonista principal. Es el que junta todos los Value Objects. Tiene los métodos `Create()` y `Update()` que garantizan que el cliente siempre esté en un estado válido.
- **`ValueObjects/`**: Las piezas del rompecabezas.
  - `FirstName.cs`, `LastName.cs`: Validan que los nombres no estén vacíos.
  - `EmailAddress.cs`: Valida que exista un `@`.
  - `BirthDate.cs`: Valida la edad mínima (ej. mayor de 18 años).
  - `Document.cs`: Valida el formato del DNI.
  - `Address.cs` y `Phone.cs`: Validaciones de contacto.
- **`Exceptions/`**: Contiene un archivo por cada "rabieta" posible del dominio. Ej. `InvalidEmailException.cs`, `InvalidAgeException.cs`. Nombres muy descriptivos para que cuando ocurra un error, sepas exactamente qué falló.
- **`Repositories/ICustomerRepository.cs`**: La interfaz o "contrato" que promete que alguien, en algún lado (Infraestructura), guardará o buscará clientes. Tiene métodos como `AddAsync`, `FindByIdAsync`.

---

## 📁 2. Application (El Director de Orquesta)
Coordina las acciones entre el Dominio y el mundo exterior.

- **`DependencyContainer.cs`**: Es la configuración. Un archivo especial que se encarga de decirle al proyecto cómo debe "inyectar" o conectar las clases de esta capa para quien las necesite desde afuera.

### 📂 Customers
- **`DTOs/` (Cajas de cartón):**
  - `CreateCustomerRequest.cs`: Lo que entra (Datos crudos desde la consola).
  - `CreateCustomerResponse.cs`: Lo que sale (Ej. el `Id` del cliente nuevo).
  - `UpdateCustomerRequest.cs / Response.cs`: Lo mismo, pero para el caso de uso de actualizar.
- **`Ports/` (Los Intercomunicadores):**
  - `ICreateCustomerInputPort.cs`: Define que existe una acción para "crear" (lo implementa el Caso de Uso).
  - `ICreateCustomerOutputPort.cs`: Define los mensajes que el caso de uso puede gritar hacia afuera (`HandleSuccessAsync`, `HandleValidationErrorAsync`, etc.).
  - `IUpdateCustomer...`: Sus equivalentes para el CU-002 de actualizar.
- **`UseCases/` (Las Recetas de Cocina):**
  - `CreateCustomerUseCase.cs`: Ejecuta los pasos: busca duplicados -> manda al dominio a crear -> guarda en repositorio -> avisa éxito.
  - `UpdateCustomerUseCase.cs`: Busca al cliente por ID -> manda al dominio a actualizar sus datos -> guarda los cambios -> avisa éxito.

---

### 💡 ¿Cómo usar este mapa?
Cuando tu profesor te pida: *"Agreguen la funcionalidad de Eliminar Cliente"*...
1. Irás a **Application/DTOs** a crear `DeleteCustomerRequest.cs`.
2. Irás a **Application/Ports** a crear los puertos de Input/Output.
3. Irás a **Application/UseCases** a crear `DeleteCustomerUseCase.cs`.
4. Probablemente vayas a **Domain/Repositories/ICustomerRepository.cs** a agregar la promesa `Task DeleteAsync(Guid id);`.

Cada vez que te pierdas, vuelve a este archivo para recordar el propósito de cada carpeta.
