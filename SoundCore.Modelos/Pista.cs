// ============================================================================
// Archivo:      Pista.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.0
// ============================================================================

using System;
using System.Collections.Generic;
using System.Text;


namespace SoundCore.Modelos
{
    /// <summary>
    /// Representa una pista musical dentro de la cola de reproducción del DJ.
    /// Es un record inmutable: cada "cambio" genera una nueva instancia.
    /// </summary>
    public record Pista(
        int Id,
        string Titulo,
        string Artista,
        int Bpm,
        int DuracionSegundos,
        string? RutaArchivo = null
    );
}