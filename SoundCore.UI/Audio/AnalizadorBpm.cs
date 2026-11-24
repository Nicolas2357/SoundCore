// ============================================================================
// Archivo:      AnalizadorBpm.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// Descripción:  Obtiene el BPM real de un archivo de audio:
//                 1. Si el MP3 trae el BPM en su etiqueta ID3 → se usa ese.
//                 2. Si el archivo NO tiene BPM → se calcula analizando el audio
//                    (DetectorBpm) sobre un fragmento de hasta 90 s.
//                 3. Si tampoco se detecta un pulso claro → se informa "sin BPM"
//                    para que el usuario lo capture a mano o con TAP.
//               Es la única clase del detector que usa NAudio (para decodificar).
// ============================================================================

using NAudio.Wave;
using SoundCore.Modelos;

namespace SoundCore.UI.Audio
{
    /// <summary>Resultado de buscar el BPM de un archivo. Bpm es null cuando el archivo no tiene BPM.</summary>
    public sealed record InfoBpm(int? Bpm, OrigenBpm Origen, string Mensaje);

    public static class AnalizadorBpm
    {
        private static readonly TimeSpan DuracionAnalisis = TimeSpan.FromSeconds(90);
        private static readonly TimeSpan InicioPreferido = TimeSpan.FromSeconds(30);

        /// <summary>Lee o calcula el BPM del archivo. Nunca lanza excepción: los errores se reportan en el mensaje.</summary>
        public static InfoBpm Obtener(string rutaArchivo)
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo) || !File.Exists(rutaArchivo))
                return new InfoBpm(null, OrigenBpm.SinBpm, "No se encontró el archivo de audio.");

            // 1. ¿El archivo ya trae el BPM en sus metadatos?
            double? bpmEtiqueta = LectorEtiquetaBpm.LeerBpm(rutaArchivo);
            if (bpmEtiqueta is double etiqueta)
                return new InfoBpm((int)Math.Round(etiqueta), OrigenBpm.Etiqueta,
                    $"BPM leído de la etiqueta del archivo ({etiqueta:0.##}).");

            // 2. No lo trae: calcularlo a partir del audio
            try
            {
                var (muestras, frecuencia) = LeerFragmentoMono(rutaArchivo);
                var deteccion = DetectorBpm.Detectar(muestras, frecuencia);

                if (deteccion is ResultadoDeteccion d)
                    return new InfoBpm((int)Math.Round(d.Bpm), OrigenBpm.Calculado,
                        $"El archivo no trae BPM: calculado del audio ({d.Bpm:0.0}, confianza {NivelConfianza(d.Confianza)}).");

                // 3. Sin pulso reconocible (audio muy corto, silencio, ambiental, voz…)
                return new InfoBpm(null, OrigenBpm.SinBpm,
                    "El archivo no tiene BPM y no se detectó un pulso claro. Escríbelo o usa TAP.");
            }
            catch (Exception ex)
            {
                return new InfoBpm(null, OrigenBpm.SinBpm,
                    $"No se pudo analizar el audio ({ex.GetType().Name}). Escribe el BPM o usa TAP.");
            }
        }

        private static string NivelConfianza(double c) => c switch
        {
            >= 0.4 => "alta",
            >= 0.2 => "media",
            _ => "baja"
        };

        /// <summary>
        /// Decodifica hasta 90 s del archivo como muestras mono.
        /// Se salta el inicio (intros sin batería) cuando la canción es lo bastante larga.
        /// </summary>
        private static (float[] Muestras, int Frecuencia) LeerFragmentoMono(string ruta)
        {
            using var lector = new AudioFileReader(ruta);
            int canales = lector.WaveFormat.Channels;
            int frecuencia = lector.WaveFormat.SampleRate;
            TimeSpan total = lector.TotalTime;

            TimeSpan inicio = TimeSpan.Zero;
            TimeSpan duracion = DuracionAnalisis;
            if (total > TimeSpan.Zero)
            {
                if (total > TimeSpan.FromSeconds(60))
                    inicio = TimeSpan.FromSeconds(Math.Min(InicioPreferido.TotalSeconds, total.TotalSeconds * 0.15));
                duracion = total - inicio < DuracionAnalisis ? total - inicio : DuracionAnalisis;
            }
            if (inicio > TimeSpan.Zero) lector.CurrentTime = inicio;

            int maxMuestras = (int)(duracion.TotalSeconds * frecuencia);
            var mono = new float[maxMuestras];
            var bufer = new float[frecuencia * canales]; // 1 segundo por lectura
            int escritas = 0;

            while (escritas < maxMuestras)
            {
                int leidas = lector.Read(bufer.AsSpan());
                if (leidas <= 0) break;

                int cuadros = leidas / canales;
                for (int f = 0; f < cuadros && escritas < maxMuestras; f++)
                {
                    float suma = 0;
                    for (int c = 0; c < canales; c++) suma += bufer[f * canales + c];
                    mono[escritas++] = suma / canales;
                }
            }

            if (escritas < maxMuestras) Array.Resize(ref mono, escritas);
            return (mono, frecuencia);
        }
    }
}
