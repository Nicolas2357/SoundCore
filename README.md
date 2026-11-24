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

## 📝 Calificación

| Dimensión | Peso | Calificación obtenida | Evidencia en el proyecto |
| :--- | :---: | :---: | :--- |
| Estructuras desde cero (punteros, nodos, algoritmos) | 40% | ___ / 40 | `Nodo<T>` y `ListaSimpleEnlazada<T>` con los 6 métodos, sin arrays ni colecciones de .NET. |
| Interfaz gráfica WinForms (usabilidad, binding reactivo) | 25% | ___ / 25 | Cambio de estructura en vivo, reproductor real, BPM automático, barra de estado con tiempos. |
| Integración .NET 10 (`LinkedList<T>` y `List<T>`) | 20% | ___ / 20 | Las mismas 6 operaciones implementadas con las colecciones nativas. |
| Benchmark y análisis (medición empírica con `Stopwatch`) | 15% | ___ / 15 | Benchmark determinista (sin `Random`), 5 corridas, mediana y gráfico comparativo. |
| **Total** | **100%** | **___ / 100** | |

**Observaciones de la revisión (atendidas en la versión 2.0):**

| # | Observación | Estado |
| :---: | :--- | :---: |
| 1 | Quitar el `Random` del benchmark | ✅ |
| 2 | Que los BPM se calculen correctamente | ✅ |
| 3 | Mejorar todavía más la interfaz | ✅ |
| 4 | Agregar la calificación al README | ✅ |
| 5 | Manejar cuando un archivo no tiene BPM | ✅ |

El detalle de cada cambio está en [Cambios de la versión 2.0](#-cambios-de-la-versión-20).

---

## 🏗️ Arquitectura

El proyecto está organizado en 3 capas, separadas en proyectos independientes dentro de la misma solución `SoundCore.slnx`:

```
SoundCore/
├── SoundCore.Modelos/              → Biblioteca de clases (.NET 10)
│   └── Pista.cs                    → record inmutable + enum OrigenBpm
├── SoundCore.EstructurasPropias/   → Biblioteca de clases (.NET 10)
│   ├── Nodo.cs                     → nodo genérico (Nodo<T>)
│   └── ListaSimpleEnlazada.cs      → lista enlazada genérica desde cero
└── SoundCore.UI/                   → Aplicación de Windows Forms (.NET 10)
    ├── MainForm.cs / .Designer.cs  → formulario principal y eventos
    ├── ControlesVisuales.cs        → barra de progreso, LED de beat y gráfico (dibujados a mano)
    ├── BenchmarkEstructuras.cs     → benchmark de estrés determinista
    ├── AudioManager.cs             → reproducción de audio real con NAudio
    ├── Audio/
    │   ├── AnalizadorBpm.cs        → obtiene el BPM: etiqueta ID3 → análisis → "sin BPM"
    │   ├── LectorEtiquetaBpm.cs    → lee el frame TBPM de la etiqueta ID3v2
    │   └── DetectorBpm.cs          → calcula el tempo a partir de las muestras de audio
    └── Assets/                     → archivos de audio de ejemplo
```

**Regla de desacoplamiento:** `SoundCore.EstructurasPropias` no tiene ninguna dependencia de UI (nada de `MessageBox`, `Console`, ni referencias a Windows Forms). Es una lista genérica (`ListaSimpleEnlazada<T>`) 100% reutilizable en cualquier tipo de aplicación (consola, web, escritorio), que implementa `IEnumerable<T>` para poder enlazarse directamente a cualquier control de datos.

---

## ⚙️ Requisitos para compilar y ejecutar

- **Visual Studio 2026** (o superior) con la carga de trabajo ".NET desktop development".
- **.NET 10 SDK**.
- Paquete NuGet **NAudio** (se restaura automáticamente al abrir la solución; instalado únicamente en `SoundCore.UI`).

**Pasos:**
1. Clonar el repositorio.
2. Abrir `SoundCore.slnx` en Visual Studio.
3. Establecer `SoundCore.UI` como proyecto de inicio.
4. Compilar y ejecutar (F5).

> **Nota sobre el audio de ejemplo:** las 3 pistas demo leen sus archivos desde `Assets/`. Si esos `.mp3` no están en el equipo, el programa funciona igual (las pistas quedan sin audio real y con su BPM publicado). También puedes agregar tus propios archivos `.mp3`/`.wav` con el botón **"Examinar..."** o arrastrándolos a la ventana.

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

- **Encabezado** con el logo y una insignia que indica la estructura activa (cambia de color con cada estructura).
- **Registro de pista:** título, artista, BPM, duración y archivo de audio. Al elegir (o arrastrar) un archivo se autocompletan la duración, el título/artista (patrón `Artista - Título.mp3`) y el **BPM real** (ver abajo).
- **TAP tempo:** botón para marcar el pulso con clics y medir el BPM a mano.
- **Estructura activa:** alterna en vivo entre Lista Propia, `LinkedList<T>` y `List<T>`; el grid se redibuja al instante con la estructura seleccionada.
- **Acciones de cola:** Encolar al final, Reproducir Siguiente, Avanzar Pista, Invertir Lista, Ordenar por Curva BPM, Purgar Duplicados. Cada botón muestra en su *tooltip* la complejidad en cada estructura, y cada acción reporta en la **barra de estado** cuánto tardó (en milisegundos).
- **Grid de la cola:** columnas separadas de título y artista, BPM coloreado por rango de tempo (< 100, 100–129, ≥ 130), columna *Fuente BPM* (Audio / ID3 / Manual / Sin BPM), duración en `m:ss`, la cabeza marcada como "siguiente en sonar" y un mensaje cuando la cola está vacía.
- **Reproductor real (extensión sobre la especificación base):** audio con NAudio, Play/Pausa/Detener (barra espaciadora = play/pausa), barra de progreso arrastrable, tiempo transcurrido y restante, BPM de la pista y un **LED que destella al tempo** de la canción.
- **Benchmark de estrés:** gráfico de barras y tabla con la mediana de 5 corridas de 20,000 inserciones intermedias en cada estructura.

---

## 🎚️ Cálculo del BPM

Para cada archivo, `AnalizadorBpm` sigue este orden:

1. **Etiqueta ID3:** si el MP3 trae el BPM guardado (frame `TBPM` de ID3v2.3/2.4, o `TBP` en ID3v2.2), se usa ese valor.
2. **Análisis del audio:** si el archivo **no tiene BPM** (lo más común), `DetectorBpm` lo calcula sobre un fragmento de hasta 90 s:
   - diezma la señal a ~22 kHz y calcula el **flujo espectral** (FFT con ventana Hann) → curva de "ataques" (golpes de bombo, caja, etc.);
   - aplica **autocorrelación** a esa curva: los golpes se repiten cada periodo de beat;
   - evalúa los BPM candidatos de 60.0 a 200.0 (paso 0.05) con un **filtro peine** (1, 2, 3 y 4 periodos) y una ponderación alrededor de 120 BPM para evitar errores de "doble / mitad de tempo".
3. **Sin BPM:** si el audio es muy corto, está en silencio o no tiene un pulso claro (tono sostenido, ambiental, voz), no se inventa un valor: el formulario lo indica en ámbar y, al encolar, pregunta si usar el BPM escrito, encolar la pista **sin BPM** o volver para capturarlo (a mano o con TAP). Las pistas sin BPM se muestran con "—" y quedan **al final** al ordenar por curva BPM.

El análisis corre en segundo plano (la ventana no se congela). Resultados con las pistas demo, comparados con el tempo publicado de cada canción:

| Pista | Antes (valor fijo en el código) | Calculado | Publicado |
| :--- | :---: | :---: | :---: |
| Fleetwood Mac — The Chain | 105 | **152** | ≈152 |
| Metro Boomin, A$AP Rocky, Roisee — Am I Dreaming | 140 | **90** | 90 |
| Pitbull ft. John Ryan — Fireball | 128 | **123** | 123 |

---

## ⏱️ Metodología del benchmark

`BenchmarkEstructuras` mide 20,000 inserciones justo después de la cabeza en cada estructura:

- **Sin `Random`:** las 20,000 pistas se generan una sola vez de forma determinista (BPM = `100 + i % 50`) y las 3 estructuras reciben exactamente los mismos objetos, en el mismo orden. Cada ejecución usa los mismos datos.
- **Solo se cronometra la inserción:** crear las pistas queda fuera del `Stopwatch`.
- **Calentamiento** previo (JIT) y `GC.Collect()` antes de cada corrida.
- **5 corridas** por estructura; se reporta la **mediana** (y el mínimo – máximo), con precisión de microsegundos (`Stopwatch.Elapsed.TotalMilliseconds`).

Resultado esperado: la Lista Propia y `LinkedList<T>` tardan fracciones de milisegundo (O(1) por inserción, solo se reconectan punteros), mientras que `List<T>` tarda decenas de milisegundos (O(n) por inserción, por el desplazamiento de memoria con `Array.Copy`).

---

## 🎵 Extensión: reproducción de audio real con NAudio

Aunque la especificación base contempla únicamente una simulación visual de reproducción, se integró **NAudio** (`AudioFileReader` + `WaveOut`) para reproducir audio real desde la cola, incluyendo control de Play/Pausa/Detener y una barra de progreso funcional. NAudio también se usa para decodificar el audio que analiza el detector de BPM. Esta dependencia está **aislada exclusivamente en `SoundCore.UI`**; los proyectos `SoundCore.Modelos` y `SoundCore.EstructurasPropias` no tienen ninguna dependencia externa y siguen usando solo la BCL de .NET.

---

## 🧪 Casos de prueba

| ID | Escenario | Resultado esperado |
| :--- | :--- | :--- |
| CP-01 | Inserción al final | Se agrega en la última fila del grid. |
| CP-02 | Prioridad "Up Next" | Se posiciona en la fila 2, detrás de la cabeza. |
| CP-03 | Avanzar pista | Actualiza el reproductor; desaparece la primera fila. |
| CP-04 | Inversión in-place | El grid refleja el orden invertido, sin memoria adicional. |
| CP-05 | Ordenar por curva BPM | El grid reordena de menor a mayor BPM; con BPM repetidos se conserva el orden relativo y las pistas sin BPM quedan al final. |
| CP-06 | Purga de duplicados | Se conserva la primera aparición; se elimina la réplica. |
| CP-07 | Manejo de excepciones | `MessageBox` controlado al avanzar con la cola vacía, sin caída de la app. |
| CP-08 | BPM calculado | Al cargar un MP3 sin BPM en su etiqueta, el BPM se calcula del audio (Fuente = "Audio"). |
| CP-09 | Archivo sin BPM | Con un audio sin pulso claro aparece el aviso ámbar; al encolar se ofrece usar el valor escrito, encolar sin BPM o cancelar. |
| CP-10 | Benchmark determinista | Dos ejecuciones seguidas usan los mismos datos; la `List<T>` es decenas de veces más lenta que las listas enlazadas. |

---

## 📊 Rúbrica de evaluación

| Dimensión | Peso |
| :--- | :---: |
| Estructuras desde cero (punteros, nodos, algoritmos) | 40% |
| Interfaz gráfica WinForms (usabilidad, binding reactivo) | 25% |
| Integración .NET 10 (`LinkedList<T>` y `List<T>`) | 20% |
| Benchmark y análisis (medición empírica con `Stopwatch`) | 15% |

---

## 🔄 Cambios de la versión 2.0

1. **Benchmark sin `Random`:** datos deterministas pre-generados, idénticos para las 3 estructuras; se mide solo la inserción, con calentamiento y mediana de 5 corridas. Antes se usaba `Random` dentro del ciclo cronometrado y `ElapsedMilliseconds`, que redondeaba a 0 ms las listas enlazadas.
2. **BPM calculados correctamente:** los BPM de las pistas demo ya no son valores fijos (y erróneos); se leen de la etiqueta ID3 o se calculan del audio, tanto para las pistas demo como para cualquier archivo nuevo. Ordenar por curva BPM ahora es estable en las 3 estructuras (`List<T>.Sort` no lo es).
3. **Interfaz mejorada:** encabezado con estructura activa, display "Now Playing" con BPM y LED de beat, barra de progreso propia, grid con más información, gráfico del benchmark, barra de estado con el tiempo de cada operación, *tooltips* con complejidades, arrastrar y soltar archivos, atajo de teclado y ventana que aprovecha el espacio al maximizar.
4. **README con calificación** (sección [Calificación](#-calificación)).
5. **Archivos sin BPM:** se detectan y se manejan sin inventar valores (aviso, TAP, encolar sin BPM; quedan al final de la curva).

---

## 🎥 Video demostrativo

Video de máximo 3 minutos, disponible en: https://www.youtube.com/watch?v=aBvktbAeKQE
