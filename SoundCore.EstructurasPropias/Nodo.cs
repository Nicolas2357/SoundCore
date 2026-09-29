// ============================================================================
// Archivo:      Nodo.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.0
// ============================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace SoundCore.EstructurasPropias
{
    /// <summary>
    /// Nodo genérico para la lista simplemente enlazada.
    /// Encapsula el valor y la referencia al siguiente nodo.
    /// </summary>
    public class Nodo<T>
    {
        public T Valor { get; set; }
        public Nodo<T>? Siguiente { get; set; }

        public Nodo(T valor)
        {
            Valor = valor;
            Siguiente = null;
        }
    }
}