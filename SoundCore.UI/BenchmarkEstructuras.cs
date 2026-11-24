// ============================================================================
// Archivo:      BenchmarkEstructuras.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// Descripción:  Benchmark de estrés de inserciones intermedias, sin Random.
//
//   Para que la comparación sea justa y repetible:
//     • Datos deterministas: las N pistas se generan UNA sola vez, con BPM
//       calculado a partir del índice (100 + i % 50), y las 3 estructuras
//       reciben exactamente los mismos objetos en el mismo orden.
//     • Se cronometra solo la inserción: crear las pistas queda fuera del
//       Stopwatch (antes también se medía la creación de cada objeto y la
//       generación del número aleatorio).
//     • Calentamiento previo para que el compilador JIT no castigue a la
//       primera estructura medida.
//     • Varias corridas por estructura; se reporta la mediana (resiste
//       picos aislados del recolector de basura o del sistema operativo).
// ============================================================================

using SoundCore.EstructurasPropias;
using SoundCore.Modelos;
using System.Diagnostics;

namespace SoundCore.UI
{
    public sealed record ResultadoBenchmark(string Estructura, string Complejidad, double MedianaMs, double MinimoMs, double MaximoMs);

    public static class BenchmarkEstructuras
    {
        /// <summary>Genera N pistas de prueba de forma determinista (mismo resultado en cada ejecución).</summary>
        public static Pista[] GenerarPistas(int n)
        {
            var pistas = new Pista[n];
            for (int i = 0; i < n; i++)
                pistas[i] = new Pista(i + 1, $"Pista {i + 1}", "DJ Benchmark", 100 + (i % 50), 180);
            return pistas;
        }

        public static IReadOnlyList<ResultadoBenchmark> Ejecutar(int n, int corridas)
        {
            Pista[] datos = GenerarPistas(n);
            var cabeza = new Pista(0, "Head", "DJ", 120, 200);

            // Calentamiento (no se reporta): compila con JIT los 3 caminos de código
            Pista[] calentamiento = GenerarPistas(Math.Min(n, 1_000));
            InsertarPropia(calentamiento, cabeza);
            InsertarLinkedList(calentamiento, cabeza);
            InsertarList(calentamiento, cabeza);

            return new[]
            {
                Medir("Lista Enlazada Propia", "O(1) reconexión de punteros", corridas, () => InsertarPropia(datos, cabeza)),
                Medir("LinkedList<T> (.NET)",  "O(1) reconexión de punteros", corridas, () => InsertarLinkedList(datos, cabeza)),
                Medir("List<T> (arreglo)",     "O(n) desplazamiento Array.Copy", corridas, () => InsertarList(datos, cabeza)),
            };
        }

        // Cada método construye su estructura fuera del Stopwatch y devuelve solo el tiempo de inserción
        private static double InsertarPropia(Pista[] datos, Pista cabeza)
        {
            var lista = new ListaSimpleEnlazada<Pista>();
            lista.AgregarAlFinal(cabeza);
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < datos.Length; i++)
                lista.ReproducirSiguiente(datos[i]);   // inserta justo después de la cabeza
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private static double InsertarLinkedList(Pista[] datos, Pista cabeza)
        {
            var lista = new LinkedList<Pista>();
            var nodoCabeza = lista.AddLast(cabeza);
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < datos.Length; i++)
                lista.AddAfter(nodoCabeza, datos[i]);
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private static double InsertarList(Pista[] datos, Pista cabeza)
        {
            var lista = new List<Pista>(datos.Length + 1) { cabeza }; // capacidad reservada: se mide el desplazamiento, no el crecimiento
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < datos.Length; i++)
                lista.Insert(1, datos[i]);             // desplaza todos los elementos posteriores
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private static ResultadoBenchmark Medir(string nombre, string complejidad, int corridas, Func<double> prueba)
        {
            var tiempos = new double[corridas];
            for (int i = 0; i < corridas; i++)
            {
                // Recolectar basura antes de cada corrida para que no interrumpa la medición
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                tiempos[i] = prueba();
            }

            Array.Sort(tiempos);
            double mediana = corridas % 2 == 1
                ? tiempos[corridas / 2]
                : (tiempos[corridas / 2 - 1] + tiempos[corridas / 2]) / 2.0;

            return new ResultadoBenchmark(nombre, complejidad, mediana, tiempos[0], tiempos[^1]);
        }
    }
}
