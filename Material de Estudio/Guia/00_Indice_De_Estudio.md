# 📚 Índice de Estudio: Clean Architecture con C#

Bienvenido a tus apuntes de la materia. Este proyecto (CommerX) está construido usando **Clean Architecture**.

Para no abrumarte con tantos archivos, hemos organizado todo el conocimiento teórico en **3 capítulos principales**. Sigue este orden de lectura para entender desde los conceptos hasta el código real.

## 📖 Capítulo 1: ¿Qué hace cada archivo? (Teoría)
👉 **[01_Conceptos_Clean_Architecture.md](./01_Conceptos_Clean_Architecture.md)**

Aprende qué es el **Dominio** (el cerebro), la **Aplicación** (el director de orquesta), y qué significan términos clave como *Entity, Value Object, DTO, Use Case* y *Port*.

## 🚀 Capítulo 2: El Flujo (Arquitectura en Movimiento)
👉 **[02_El_Viaje_del_Dato.md](./02_El_Viaje_del_Dato.md)**

El cuento paso a paso de cómo viaja un dato desde que el usuario aprieta "Enter" en la consola, hasta que se guarda en la base de datos (pasando por las validaciones de negocio y puertos de salida).

## 💻 Capítulo 3: La Hoja de Trucos de C# (Sintaxis)
👉 **[03_Sintaxis_CSharp.md](./03_Sintaxis_CSharp.md)**

¿Viste un `record`, un `get; private set;`, un `Task` o un `async/await` y no recuerdas qué hace? Esta es tu guía de referencia rápida sobre la sintaxis moderna de C# usada en este proyecto.

## 🗺️ Capítulo 4: El Mapa del Proyecto (Carpetas y Archivos)
👉 **[04_Mapa_del_Proyecto.md](./04_Mapa_del_Proyecto.md)**

¿Te pierdes en la solución? Este apunte detalla exactamente para qué sirve **cada carpeta y cada archivo .cs** que existe actualmente en CommerX. Léelo para entender cómo se mapean los conceptos a los archivos reales.

## 🛠️ Capítulo 5: El Orden de Desarrollo (La Receta)
👉 **[05_Orden_De_Desarrollo.md](./05_Orden_De_Desarrollo.md)**

¿Tienes que crear un Caso de Uso nuevo y no sabes por dónde empezar a teclear? Esta es la receta visual y paso a paso. Te indica exactamente qué archivo crear primero, cuál segundo, y cómo conectar todo sin perderte.

---

## 🛠️ Material Práctico: Casos de Uso (Ejercicios)
La carpeta `../Material` contiene la documentación detallada de cada trabajo práctico / caso de uso implementado (ej. CU-001 Crear Cliente, CU-002 Actualizar Cliente) junto con sus diagramas. Consúltalos únicamente cuando vayas a programar esa funcionalidad en específico.
