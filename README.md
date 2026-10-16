# AGIL — Aprendizaje Gamificado con Itinerarios Libres

Aplicación móvil educativa para Android basada en microaprendizaje y gamificación, desarrollada como Trabajo Fin de Grado del Grado en Diseño y Desarrollo de Videojuegos de la Universidad Rey Juan Carlos.

AGIL traslada las dinámicas del aprendizaje gamificado a cualquier materia que pueda expresarse mediante texto e imagen, y permite a los propios usuarios crear itinerarios formativos y compartirlos entre sí.

## Descarga

El paquete instalable está disponible en el apartado de [versiones del repositorio](https://github.com/NBGonzalez/TFG/releases/latest).

- Requiere Android 7.0 (API 24) o superior.
- Al no distribuirse a través de Google Play, Android solicitará permiso para instalar aplicaciones de origen desconocido.

## Características

- **Cuatro tipos de minijuego**: explicación, cuestionario, unir columnas y rellenar huecos.
- **Itinerarios oficiales** de SQL y Biología, distribuidos desde la nube y descargados bajo demanda.
- **Editor de itinerarios** integrado, para crear contenido propio desde el móvil sin conocimientos técnicos.
- **Compartición** de itinerarios mediante las aplicaciones de mensajería del dispositivo, sin servidor intermedio.
- **Gamificación** fundamentada en la teoría de la autodeterminación: estrellas, racha diaria, títulos y avatares desbloqueables y clasificación diaria.
- **Explicación de errores** mediante inteligencia artificial generativa, en el contexto exacto del fallo.
- **Progreso sincronizado** en la nube, con acceso anónimo y vinculación opcional a Google Play Games.

## Tecnologías

| Ámbito | Tecnología |
|---|---|
| Motor y lenguaje | Unity 6 (6000.2.6f2) y C# |
| Identidad y progreso | Unity Authentication y Unity Cloud Save |
| Distribución de contenidos | Azure PlayFab |
| Inteligencia artificial | API de Google Gemini |
| Cuentas | Google Play Games Services |
| Compartición | NativeShare |
| Efectos visuales | Shader Graph |

## Estructura del repositorio

- `Assets/Scripts/` — código fuente propio del proyecto.
- `Assets/Shaders/` — sombreadores creados con Shader Graph.
- `Assets/Resources/` — recursos cargados en tiempo de ejecución: interfaz, avatares, títulos e imágenes de los minijuegos.
- `Assets/Plugins/Android/` — manifiesto de Android con los filtros de intención para importar itinerarios.

## Abrir el proyecto

1. Clonar el repositorio.
2. Abrirlo con Unity 6000.2.6f2 o una versión compatible.
3. La clave de la API de Gemini no se incluye en el repositorio. Para que funcione la explicación de errores, es necesario proporcionar una clave propia.

## Autoría

- **Autor**: Néstor Bermejillo González
- **Tutor**: Mario Manzano Vázquez
- **Titulación**: Grado en Diseño y Desarrollo de Videojuegos — Universidad Rey Juan Carlos

## Licencia

El código propio de este proyecto se distribuye bajo la licencia [Apache 2.0](LICENSE). Las bibliotecas de terceros incluidas conservan sus licencias originales.
