// ============================================================================
// Archivo:      Pista.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// ============================================================================

namespace SoundCore.Modelos
{
    /// <summary>De dónde salió el BPM de una pista.</summary>
    public enum OrigenBpm
    {
        /// <summary>Lo escribió el usuario (o lo marcó con TAP).</summary>
        Manual,
        /// <summary>Venía guardado en la etiqueta ID3 (TBPM) del archivo.</summary>
        Etiqueta,
        /// <summary>El archivo no lo traía: se calculó analizando el audio.</summary>
        Calculado,
        /// <summary>El archivo no tiene BPM y no se pudo calcular: Bpm vale 0.</summary>
        SinBpm
    }

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
        string? RutaArchivo = null,
        OrigenBpm OrigenBpm = OrigenBpm.Manual
    )
    {
        /// <summary>True si la pista tiene un BPM válido (las pistas sin BPM valen 0).</summary>
        public bool TieneBpm => Bpm > 0;

        /// <summary>
        /// Criterio de la "curva BPM": de menor a mayor, y las pistas sin BPM al final
        /// (si no, un 0 las mandaría al principio y rompería la curva).
        /// </summary>
        public static int CompararPorBpm(Pista a, Pista b)
        {
            int claveA = a.TieneBpm ? a.Bpm : int.MaxValue;
            int claveB = b.TieneBpm ? b.Bpm : int.MaxValue;
            return claveA.CompareTo(claveB);
        }
    }
}
