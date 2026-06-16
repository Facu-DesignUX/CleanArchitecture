## CU-CLI-002 — Actualización de Cliente · Sistema CommerX

---

### 1. Identificación

| Campo | Valor |
|---|---|
| **Nombre del Caso de Uso** | Actualización de Cliente |
| **ID** | CU-CLI-002 |
| **Versión** | 1.0 |
| **Fecha** | ___ / ___ / ______ |
| **Autor** | __________________________ |
| **Sistema** | CommerX |
| **Módulo** | Gestión de Clientes |

---

### 2. Descripción General

| Campo | Detalle |
|---|---|
| **Descripción breve** | Permite modificar los datos de contacto de un cliente existente en el sistema. El nombre, el apellido y el número de documento no pueden modificarse, ya que forman parte de la identidad registrada del cliente. |
| **Objetivo** | Mantener actualizada la información de contacto de un cliente ya registrado, garantizando que los datos modificados cumplan las mismas reglas de negocio que en el alta. |
| **Alcance** | Desde la localización del cliente hasta la confirmación de la actualización exitosa. |

---

### 3. Actores

| Rol | Actor |
|---|---|
| **Actor principal** | Usuario / Operador / Administrador |
| **Actores secundarios** | Sistema de validación |

---

### 4. Precondiciones

- El usuario debe estar autenticado.
- El usuario debe tener permisos para modificar clientes.
- El cliente a modificar debe existir en el sistema.
- El sistema debe estar disponible.

---

### 5. Postcondiciones

| Estado | Resultado |
|---|---|
| **Éxito** | Los datos de contacto del cliente quedan actualizados en la base de datos. El ID, nombre, apellido y número de documento permanecen sin cambios. Se muestra confirmación de la operación al usuario. |
| **Fallo** | No se persisten cambios en la base de datos. Se informa el motivo del error al usuario. Los datos originales del cliente permanecen intactos. |

---

### 6. Flujo Principal

| Paso | Descripción |
|---|---|
| 1 | El actor accede a la pantalla de gestión de clientes. |
| 2 | El actor localiza al cliente por ID o número de documento. |
| 3 | El sistema muestra los datos actuales del cliente en un formulario. Los campos Nombre, Apellido y Documento aparecen en modo solo lectura. |
| 4 | El actor modifica uno o más de los campos habilitados: · Email · Teléfono · Dirección · Fecha de nacimiento |
| 5 | El actor confirma la operación presionando Guardar cambios. |
| 6 | El sistema valida los datos modificados contra las reglas de negocio. |
| 7 | El sistema actualiza el registro del cliente en la base de datos. |
| 8 | El sistema muestra mensaje de confirmación de la actualización exitosa. |

---

### 7. Flujos Alternativos

**7.1 CLIENTE NO ENCONTRADO**
El sistema no encuentra un cliente con el criterio ingresado.
Se muestra un mensaje indicando que el cliente no existe en el sistema.
El actor puede ingresar un nuevo criterio de búsqueda o cancelar.

**7.2 EMAIL CON FORMATO INVÁLIDO**
El sistema detecta que el nuevo email no cumple el formato `usuario@dominio.extensión`.
Se muestra el mensaje de error junto al campo Email.
El actor puede corregir el dato y reintentar.

**7.3 EMAIL DUPLICADO**
El sistema detecta que el nuevo email ya pertenece a otro cliente distinto.
Se notifica al usuario indicando el conflicto de unicidad.
El actor puede ingresar un email diferente o cancelar la operación.

**7.4 FECHA DE NACIMIENTO INVÁLIDA**
El sistema detecta que la nueva fecha es futura, o que el cliente no cumpliría 18 años con la fecha ingresada.
Se muestra el mensaje de error junto al campo Fecha de nacimiento.
El actor puede corregir el dato y reintentar.

**7.5 SIN CAMBIOS DETECTADOS**
El actor confirma sin haber modificado ningún campo habilitado.
El sistema detecta que los valores ingresados son idénticos a los actuales.
Se informa al usuario que no se realizaron modificaciones y no se persiste nada.

**7.6 ERROR DEL SISTEMA**
Falla durante la persistencia de los datos en la base de datos.
Se muestra un mensaje de error técnico al usuario.
Los datos originales del cliente no se ven afectados.
El error queda registrado en los logs del sistema.

---

### 8. Reglas de Negocio

| Código | Descripción |
|---|---|
| **RN-001** | El nombre, el apellido y el número de documento son inmutables. No pueden modificarse una vez registrado el cliente. |
| **RN-002** | El email debe tener formato válido: `usuario@dominio.extensión`. |
| **RN-003** | El email debe ser único en el sistema. No puede coincidir con el de otro cliente distinto al que se está editando. |
| **RN-004** | La fecha de nacimiento debe mantener la mayoría de edad (18 años cumplidos al momento de la actualización). |
| **RN-005** | La fecha de nacimiento no puede ser una fecha futura ni la fecha actual. |
| **RN-006** | Debe existir al menos un campo modificado respecto al valor actual para que la operación proceda. |

---

### 9. Requisitos No Funcionales

| Atributo | Requisito |
|---|---|
| **Usabilidad** | El formulario debe mostrar los datos actuales del cliente precargados. Los campos no editables (Nombre, Apellido, Documento) deben estar visualmente diferenciados. |
| **Rendimiento** | La carga de datos del cliente y la confirmación de la actualización deben completarse en menos de 2 segundos. |
| **Seguridad** | Solo usuarios autenticados con el permiso correspondiente pueden modificar clientes. |
| **Trazabilidad** | La operación debe quedar registrada indicando qué campos fueron modificados, por quién y cuándo. |

---

### 10. Datos de Entrada

| Campo | Editable | Descripción |
|---|---|---|
| ID de cliente | No | Identificador único. Se usa para localizar el registro. Solo lectura. |
| Nombre | No | Inmutable. Se muestra como referencia. |
| Apellido | No | Inmutable. Se muestra como referencia. |
| Número de documento | No | Inmutable. Identifica de forma única al cliente. |
| Email | Sí | Formato válido requerido. Debe ser único (excluyendo al cliente actual). |
| Teléfono | Sí | Sin restricción de formato estricta. |
| Dirección | Sí | Sin restricción de formato estricta. |
| Fecha de nacimiento | Sí | Fecha pasada. El cliente debe mantener la mayoría de edad (18 años). |

---

### 11. Datos de Salida

| Salida | Descripción |
|---|---|
| **Confirmación** | Mensaje indicando que los datos fueron actualizados exitosamente. |
| **ID del cliente** | Confirmación del cliente que fue modificado. |
| **Datos actualizados** | Los nuevos valores confirmados por el sistema. |
| **Mensajes de error** | Descripción del error en caso de fallo, asociada al campo que lo originó. |

---

### 12. Validaciones

| Validación | Descripción |
|---|---|
| **Existencia del cliente** | El ID debe corresponder a un cliente registrado en el sistema. |
| **Formato de email** | Debe cumplir el patrón `usuario@dominio.extensión`. |
| **Unicidad de email** | El nuevo email no debe pertenecer a otro cliente existente. La verificación excluye al cliente que se está editando. |
| **Mayoría de edad** | La fecha de nacimiento debe resultar en al menos 18 años cumplidos al momento de la actualización. |
| **Fecha no futura** | La fecha de nacimiento no puede ser igual o posterior a la fecha actual. |
| **Existencia de cambios** | Al menos uno de los campos editables debe diferir del valor actual almacenado. |

---

### 13. Interfaz de Usuario

| Elemento | Detalle |
|---|---|
| **Formulario** | Campos precargados con los datos actuales del cliente. Campos no editables con estilo visual diferenciado: · Nombre · Apellido · Documento. Campos editables habilitados: · Email · Teléfono · Dirección · Fecha de nacimiento |
| **Botones** | Guardar cambios · Cancelar |
| **Mensajes en línea** | Errores de validación junto a cada campo que los originó. |
| **Confirmación** | Mensaje de éxito visible al completar la operación exitosamente. |

---

### 14. Frecuencia de Uso

**Frecuencia:** Media

---

### 15. Prioridad

**Prioridad:** Alta

---

### 16. Supuestos

- El usuario conoce el ID o el número de documento del cliente a modificar.
- El sistema tiene acceso a la base de datos para localizar y actualizar el registro.
- El cliente fue registrado previamente mediante el caso de uso CU-CLI-001 — Alta de Cliente.

---

### 17. Dependencias

| Dependencia | Descripción |
|---|---|
| **CU-CLI-001** | Alta de Cliente. El cliente debe existir en el sistema antes de poder ser actualizado. |
| **Base de datos** | Acceso de lectura y escritura sobre el repositorio de clientes. |
| **Autenticación** | El sistema de autenticación debe estar disponible para verificar permisos del usuario. |

---

### 18. Riesgos

| Riesgo | Mitigación |
|---|---|
| **Email duplicado por condición de carrera** | Aplicar restricción de unicidad en la base de datos como segunda línea de defensa. |
| **Modificación no autorizada** | Validar permisos tanto en la capa de presentación como en la capa de aplicación. |
| **Falla durante la persistencia** | Usar operación atómica para garantizar que el registro no quede en estado inconsistente. |

---

### 19. Actividad Práctica

**CONSIGNA**

Con base en este caso de uso y en el material teórico visto hasta ahora, implementar en el proyecto COMMERX los elementos detallados a continuación. Cada ítem debe seguir los mismos patrones y convenciones aplicados en el caso de uso CU-CLI-001 — ALTA DE CLIENTE.

**CHECKLIST DE IMPLEMENTACIÓN**

**DOMINIO — COMMERX.DOMAIN**
- ☐ Agregar el método `Update()` a la entidad `Customer` con los cuatro campos modificables.
- ☐ Verificar que `Email` y `BirthDate` sean validados por sus Value Objects dentro del método.
- ☐ Garantizar que `FirstName`, `LastName` y `Document` no sean modificables desde afuera de la entidad.

**APLICACIÓN — COMMERX.APPLICATION**
- ☐ Crear `UpdateCustomerRequest` como `sealed class` con propiedades `init` y constructor explícito.
- ☐ Crear `UpdateCustomerResponse` como `sealed class` con propiedades `init` y constructor explícito.
- ☐ Crear `IUpdateCustomerInputPort` con el método `ExecuteAsync()`.
- ☐ Crear `IUpdateCustomerOutputPort` con los métodos `HandleSuccessAsync`, `HandleDuplicateAsync`, `HandleValidationErrorAsync` y `ValidationErrorsAsync`.
- ☐ Ampliar `ICustomerRepository` con los métodos para localizar y persistir la actualización del cliente.
- ☐ Implementar `UpdateCustomerUseCase` con la lógica completa del caso de uso.
- ☐ Registrar `UpdateCustomerUseCase` en `DependencyContainer` de la capa de aplicación.

**PREGUNTAS DE REFLEXIÓN**
- ☐ ¿Qué métodos nuevos necesita `ICustomerRepository` para localizar un cliente por ID y persistir la actualización?
- ☐ ¿Cómo se verifica que el nuevo email no pertenece a otro cliente distinto al que se está editando?
- ☐ ¿Dónde y cómo se detecta que no hubo cambios en los datos? ¿En el Interactor o en la entidad?
- ☐ ¿Por qué `FirstName`, `LastName` y `Document` no deben ser campos editables en `Update()`?
- ☐ ¿Qué excepción lanza el VO `Email` si el formato es inválido? ¿Quién la captura y cómo la traduce?

---

### 20. Notas

**DIFERENCIA CLAVE ENTRE ALTA Y ACTUALIZACIÓN**

En el Alta (CU-CLI-001) el sistema genera un `Id` nuevo con `Guid.NewGuid()`. En la Actualización (CU-CLI-002) el `Id` ya existe: el sistema lo usa para localizar al cliente. Nunca se genera un nuevo `Id`.

El método `Update()` de la entidad recibe los nuevos valores y aplica las mismas reglas de validación que `Create()` para los campos que lo requieren. La entidad sigue siendo responsable de su propia validez en todo momento.

**ATENCIÓN — UNICIDAD DE EMAIL EN LA ACTUALIZACIÓN**

Al verificar que el nuevo email no esté duplicado, la consulta al repositorio debe EXCLUIR AL CLIENTE ACTUAL de la búsqueda. De lo contrario, si el usuario guarda sin cambiar el email, el sistema lo rechazaría incorrectamente por encontrar su propio email ya registrado.