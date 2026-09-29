// ============================================================================
// Archivo:      ListaSimpleEnlazada.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.0
// ============================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SoundCore.EstructurasPropias
{
    /// <summary>
    /// Lista simplemente enlazada genérica, implementada desde cero.
    /// Opera únicamente con punteros Nodo<T>.Siguiente (sin arrays ni List<T>).
    /// </summary>
    public class ListaSimpleEnlazada<T> : IEnumerable<T>
    {
        public Nodo<T>? Cabeza { get; private set; }
        public int Conteo { get; private set; }

        public bool EstaVacia => Cabeza == null;

        // 1. Inserción al final: O(n) porque hay que recorrer hasta el último nodo
        public void AgregarAlFinal(T valor)
        {
            var nuevoNodo = new Nodo<T>(valor);

            if (EstaVacia)
            {
                Cabeza = nuevoNodo;
            }
            else
            {
                var actual = Cabeza!;
                while (actual.Siguiente != null)
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevoNodo;
            }

            Conteo++;
        }

        // 2. Up Next: inserta justo después de la cabeza (la que "suena" ahora): O(1)
        public void ReproducirSiguiente(T valor)
        {
            var nuevoNodo = new Nodo<T>(valor);

            if (EstaVacia)
            {
                Cabeza = nuevoNodo;
            }
            else
            {
                nuevoNodo.Siguiente = Cabeza!.Siguiente;
                Cabeza.Siguiente = nuevoNodo;
            }

            Conteo++;
        }

        // 3. Desencolar la pista actual (elimina la cabeza y la devuelve): O(1)
        public T AvanzarPista()
        {
            if (EstaVacia)
                throw new InvalidOperationException("La cola de reproducción está vacía.");

            T valor = Cabeza!.Valor;
            Cabeza = Cabeza.Siguiente;
            Conteo--;
            return valor;
        }

        public void Limpiar()
        {
            Cabeza = null;
            Conteo = 0;
        }
        // 4. Inversión in-place: O(n) tiempo, O(1) memoria auxiliar
        // Solo se redirigen los enlaces 'Siguiente'; no se crean nodos ni listas nuevas.
        public void Invertir()
        {
            Nodo<T>? previo = null;
            Nodo<T>? actual = Cabeza;

            while (actual != null)
            {
                Nodo<T>? siguiente = actual.Siguiente; // 1. guardar el resto de la lista
                actual.Siguiente = previo;             // 2. invertir el enlace
                previo = actual;                       // 3. avanzar previo
                actual = siguiente;                    // 4. avanzar actual
            }

            Cabeza = previo; // el último nodo visitado es la nueva cabeza
        }

        // 5. Inserción ordenada según un criterio (por ejemplo BPM): O(n)
        public void InsertarOrdenado(T valor, Comparison<T> comparador)
        {
            var nuevo = new Nodo<T>(valor);

            // Caso especial: lista vacía o el nuevo va antes que la cabeza
            if (EstaVacia || comparador(valor, Cabeza!.Valor) < 0)
            {
                nuevo.Siguiente = Cabeza;
                Cabeza = nuevo;
                Conteo++;
                return;
            }

            // Buscar el nodo después del cual debe ir el nuevo
            var actual = Cabeza;
            while (actual.Siguiente != null && comparador(valor, actual.Siguiente.Valor) >= 0)
            {
                actual = actual.Siguiente;
            }

            nuevo.Siguiente = actual.Siguiente;
            actual.Siguiente = nuevo;
            Conteo++;
        }

        // 6. Depurar duplicados sin estructuras auxiliares: O(n^2) tiempo, O(1) espacio
        public void DepurarDuplicados(Func<T, T, bool> sonIguales)
        {
            var actual = Cabeza;

            while (actual != null)
            {
                var corredor = actual;

                while (corredor.Siguiente != null)
                {
                    if (sonIguales(actual.Valor, corredor.Siguiente.Valor))
                    {
                        // Saltarse el nodo duplicado para desconectarlo
                        corredor.Siguiente = corredor.Siguiente.Siguiente;
                        Conteo--;
                    }
                    else
                    {
                        corredor = corredor.Siguiente;
                    }
                }

                actual = actual.Siguiente;
            }
        }

        // Permite usar foreach y data binding sobre la lista
        public IEnumerator<T> GetEnumerator()
        {
            var actual = Cabeza;
            while (actual != null)
            {
                yield return actual.Valor;
                actual = actual.Siguiente;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}