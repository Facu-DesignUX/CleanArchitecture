## CU-CLI-001 — Alta de Cliente · Sistema CommerX

---

### 1. Identificación

| Campo | Valor |
|---|---|
| **ID** | CU-CLI-001 |
| **Nombre** | Alta de Cliente |
| **Versión** | 1.0 |
| **Sistema** | CommerX |
| **Módulo** | Gestión de Clientes |

---

### 2. Descripción General

**Descripción:** Permite registrar un nuevo cliente en el sistema, almacenando sus datos personales, de contacto y opcionalmente información adicional.

**Objetivo:** Incorporar un cliente válido al sistema para su posterior gestión.

**Alcance:** Desde el ingreso de datos hasta la confirmación del registro.

---

### 3. Actores

- **Principal:** Usuario / Operador / Administrador
- **Secundarios:** Sistema de validación · Servicios externos (opcional)

---

### 4. Pre y Postcondiciones

**Precondiciones:**
- El usuario debe estar autenticado y tener permisos para crear clientes.
- El sistema debe estar disponible.

**Postcondiciones:**

| Estado | Resultado |
|---|---|
| **Éxito** | Cliente registrado en la base de datos. Se genera un ID único de cliente. |
| **Fallo** | No se persisten datos. Se informa el error al usuario. |

---

### 5. Flujo Principal

| Paso | Descripción |
|---|---|
| 1 | El actor accede a "Alta de Cliente". |
| 2 | El sistema muestra el formulario. |
| 3 | El actor ingresa los datos requeridos: Nombre, Apellido, Documento, Email, Teléfono, Dirección. |
| 4 | El actor confirma la operación. |
| 5 | El sistema valida los datos. |
| 6 | El sistema registra el cliente. |
| 7 | El sistema muestra mensaje de éxito. |

---

### 6. Flujos Alternativos

| ID | Nombre | Descripción |
|---|---|---|
| 6.1 | Datos incompletos | El sistema detecta campos obligatorios faltantes. Muestra mensajes de error. Permite corregir los datos. |
| 6.2 | Cliente duplicado | El sistema detecta coincidencia por documento o email. Notifica al usuario. Permite cancelar o modificar. |
| 6.3 | Error del sistema | Falla al guardar datos. Se muestra mensaje de error. El evento queda registrado en logs. |

---

### 7. Reglas de Negocio

| Regla | Descripción |
|---|---|
| Documento único | No puede existir otro cliente registrado con el mismo número de documento. |
| Formato de email | El email debe tener formato válido. |
| Nombre y apellido obligatorios | Ambos campos son requeridos para el registro. |
| Mayoría de edad | El cliente debe ser mayor de edad al momento del registro. |

---

### 8. Datos de Entrada / Salida

**Entrada:**

| Campo | Observaciones |
|---|---|
| Nombre | Obligatorio. |
| Apellido | Obligatorio. |
| Tipo y número de documento | Obligatorio. Debe ser único en el sistema. |
| Email | Obligatorio. Formato válido requerido. |
| Teléfono | Opcional. |
| Dirección | Opcional. |
| Fecha de nacimiento | Requerida para validar mayoría de edad. |
| Observaciones | Campo adicional opcional. |

**Salida:** Confirmación de alta, ID único generado para el nuevo cliente, o mensajes de error descriptivos en caso de fallo.

---

### 9. Validaciones

| Validación | Descripción |
|---|---|
| Campos obligatorios | Nombre, Apellido, Documento y Email no pueden estar vacíos. |
| Formato de email | Debe cumplir el patrón estándar de correo electrónico. |
| Longitud de campos | Cada campo debe respetar la longitud máxima permitida. |
| Unicidad | Documento y email no deben pertenecer a un cliente ya registrado. |
| Tipos de datos | Los datos ingresados deben corresponder al tipo esperado por cada campo. |

---

### 10. Requisitos No Funcionales

| Atributo | Requisito |
|---|---|
| **Usabilidad** | Interfaz clara con campos etiquetados y organizados. |
| **Rendimiento** | Tiempo de respuesta menor a 2 segundos. |
| **Seguridad** | Control de acceso y validación en todas las capas. |
| **Disponibilidad** | Alta disponibilidad del sistema. |

---

### 11. Interfaz de Usuario

- **Formulario:** campos etiquetados y organizados visualmente.
- **Botones:** `Guardar` · `Cancelar`
- **Mensajes:** errores de validación en línea junto a cada campo que los originó.

---

### 12. Dependencias y Riesgos

**Dependencias:**
- **Base de datos** — acceso para persistir el nuevo registro.
- **Autenticación** — verificación de identidad y permisos del usuario.
- **Servicios externos** — integración opcional.

**Riesgos:**

| Riesgo | Mitigación |
|---|---|
| Datos incorrectos | Validaciones estrictas en capa de entrada y de dominio. |
| Fallos de validación | Pruebas unitarias y de integración. |
| Problemas de conexión | Manejo de excepciones y reintentos. |

---

### 13. Metadata

| Atributo | Valor |
|---|---|
| **Frecuencia de uso** | Alta |
| **Prioridad** | Alta |
| **Supuestos** | El usuario conoce los datos del cliente. El sistema tiene acceso a la base de datos. |
| **Notas** | Espacio reservado para observaciones adicionales. |