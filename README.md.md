# 🎧 SoundCore Engine GUI

Gestor de cola de reproducción para DJ, desarrollado en **C# / .NET 10 / Windows Forms**, como reto de la Unidad 2 (Estructuras Lineales) de Estructura de Datos — TecNM Campus Monclova.

Compara en paralelo tres implementaciones de lista para gestionar la misma cola de reproducción: una **lista simplemente enlazada construida desde cero**, la `LinkedList<T>` de .NET y la `List<T>` de .NET, permitiendo alternar entre ellas en vivo desde la interfaz y medir su rendimiento con un benchmark de estrés.

---

## 👤 Integrantes

| Nombre completo | No. de Control |
| :--- | :--- |
| Nicolas Ponce Carmona | I25050376 |

**Fecha de entrega:** Martes 29 de Septiembre de 2026

---

## 🏗️ Arquitectura

El proyecto está organizado en 3 capas, separadas en proyectos independientes dentro de la misma solución `SoundCore.sln`:

```
SoundCore/
├── SoundCore.Modelos/            → Biblioteca de clases (.NET 10)
│   └── Pista.cs                  → record inmutable: modelo de datos
├── SoundCore.EstructurasPropias/ → Biblioteca de clases (.NET 10)
│   ├── Nodo.cs                   → nodo genérico (Nodo<T>)
│   └── ListaSimpleEnlazada.cs    → lista enlazada genérica desde cero
└── SoundCore.UI/                 → Aplicación de Windows Forms (.NET 10)
    ├── MainForm.cs / .Designer.cs → formulario principal y eventos
    ├── AudioManager.cs            → reproducción de audio real con NAudio
    └── Assets/                    → archivos de audio de ejemplo
```

**Regla de desacoplamiento:** `SoundCore.EstructurasPropias` no tiene ninguna dependencia de UI (nada de `MessageBox`, `Console`, ni referencias a Windows Forms). Es una lista genérica (`ListaSimpleEnlazada<T>`) 100% reutilizable en cualquier tipo de aplicación (consola, web, escritorio), que implementa `IEnumerable<T>` para poder enlazarse directamente a cualquier control de datos.

---

## ⚙️ Requisitos para compilar y ejecutar

- **Visual Studio 2026** (o superior) con la carga de trabajo ".NET desktop development".
- **.NET 10 SDK**.
- Paquete NuGet **NAudio** (se restaura automáticamente al abrir la solución; instalado únicamente en `SoundCore.UI`).

**Pasos:**
1. Clonar el repositorio.
2. Abrir `SoundCore.sln` en Visual Studio.
3. Establecer `SoundCore.UI` como proyecto de inicio.
4. Compilar y ejecutar (F5).

> **Nota sobre el audio de ejemplo:** por derechos de autor, los archivos `.mp3` de la carpeta `Assets/` **no se incluyen** en este repositorio. El programa funciona sin ellos (las pistas quedan sin audio real), o puedes agregar tus propios archivos `.mp3`/`.wav` desde el botón **"Examinar..."** de la interfaz.

---

## 🧠 Los 6 métodos de `ListaSimpleEnlazada<T>`

| Método | Complejidad | Descripción |
| :--- | :---: | :--- |
| `AgregarAlFinal` | O(n) | Inserta al final de la cola. |
| `ReproducirSiguiente` | O(1) | Inserta justo después de la cabeza ("Up Next"). |
| `AvanzarPista` | O(1) | Desencola y retorna la pista en cabeza. |
| `Invertir` | O(n) tiempo, **O(1) memoria** | Invierte la lista *in-place*, reordenando solo 3 punteros (`previo`, `actual`, `siguiente`). Cero nodos ni listas auxiliares. |
| `InsertarOrdenado` | O(n) | Inserta manteniendo orden, según un `Comparison<T>` (usado para ordenar por BPM). |
| `DepurarDuplicados` | O(n²) tiempo, O(1) espacio | Elimina duplicados con doble puntero, sin `HashSet` ni estructuras auxiliares. Conserva la primera aparición. |

---

## 🖥️ Funcionalidad de la interfaz

- **Registro de pista:** título, artista, BPM, duración y archivo de audio (con autocompletado de duración y, si el nombre sigue el patrón `Artista - Título.mp3`, también de esos campos).
- **Estructura activa:** alterna en vivo entre Lista Propia, `LinkedList<T>` y `List<T>`; el grid se redibuja al instante con la estructura seleccionada.
- **Acciones de cola:** Encolar al final, Reproducir Siguiente, Avanzar Pista, Invertir Lista, Ordenar por Curva BPM, Purgar Duplicados.
- **Reproductor real (extensión sobre la especificación base):** reproducción de audio con NAudio, controles de Play/Pausa/Detener, barra de progreso arrastrable y contador de tiempo.
- **Benchmark de estrés:** mide con `Stopwatch` el tiempo de 20,000 inserciones intermedias en la Lista Propia, `LinkedList<T>` y `List<T>`, evidenciando la diferencia entre operaciones O(1) por reconexión de punteros y operaciones O(n) por desplazamiento de memoria (`Array.Copy`).

---

## 🎵 Extensión: reproducción de audio real con NAudio

Aunque la especificación base contempla únicamente una simulación visual de reproducción, se integró **NAudio** (`AudioFileReader` + `WaveOutEvent`) para reproducir audio real desde la cola, incluyendo control de Play/Pausa/Detener y una barra de progreso funcional. Esta dependencia está **aislada exclusivamente en `SoundCore.UI`**; los proyectos `SoundCore.Modelos` y `SoundCore.EstructurasPropias` no tienen ninguna dependencia externa y siguen usando solo la BCL de .NET.

---

## 🧪 Casos de prueba

| ID | Escenario | Resultado esperado |
| :--- | :--- | :--- |
| CP-01 | Inserción al final | Se agrega en la última fila del grid. |
| CP-02 | Prioridad "Up Next" | Se posiciona en la fila 2, detrás de la cabeza. |
| CP-03 | Avanzar pista | Actualiza el reproductor; desaparece la primera fila. |
| CP-04 | Inversión in-place | El grid refleja el orden invertido, sin memoria adicional. |
| CP-05 | Ordenar por curva BPM | El grid reordena de menor a mayor BPM. |
| CP-06 | Purga de duplicados | Se conserva la primera aparición; se elimina la réplica. |
| CP-07 | Manejo de excepciones | `MessageBox` controlado al avanzar con la cola vacía, sin caída de la app. |

Todos los casos se validaron con las 3 estructuras de datos activas.

---

## 📊 Rúbrica de evaluación

| Dimensión | Peso |
| :--- | :---: |
| Estructuras desde cero (punteros, nodos, algoritmos) | 40% |
| Interfaz gráfica WinForms (usabilidad, binding reactivo) | 25% |
| Integración .NET 10 (`LinkedList<T>` y `List<T>`) | 20% |
| Benchmark y análisis (medición empírica con `Stopwatch`) | 15% |

---

## 🎥 Video demostrativo

Video de máximo 3 minutos, disponible en: `[pendiente — agregar enlace de YouTube]`

- **0:00 – 1:15:** demostración en vivo de las operaciones de la cola.
- **1:15 – 2:00:** ejecución del benchmark de estrés y explicación del resultado.
- **2:00 – 3:00:** explicación del método `Invertir()`, señalando `previo`, `actual` y `siguiente`.
