// ============================================================================
// Archivo:      AudioManager.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.1
// Descripción:  Encapsula la reproducción de audio real con NAudio,
//               incluyendo control de reproducción/pausa/detención y
//               seguimiento de posición para la barra de progreso.
// ============================================================================

using NAudio.Wave;

namespace SoundCore.UI
{
    /// <summary>
    /// Encapsula la reproducción de audio real con NAudio.
    /// Una instancia por MainForm; se libera con Dispose() al cerrar la ventana.
    /// </summary>
    public class AudioManager : IDisposable
    {
        private WaveOut? _dispositivoSalida;
        private AudioFileReader? _archivoAudio;

        /// <summary>True si hay una pista cargada (sonando o en pausa).</summary>
        public bool HayPistaCargada => _archivoAudio != null;

        /// <summary>True si el audio está sonando en este momento (no pausado ni detenido).</summary>
        public bool EstaReproduciendo => _dispositivoSalida?.PlaybackState == PlaybackState.Playing;

        /// <summary>True si el audio está pausado (cargado pero detenido temporalmente).</summary>
        public bool EstaPausado => _dispositivoSalida?.PlaybackState == PlaybackState.Paused;

        public TimeSpan PosicionActual => _archivoAudio?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan DuracionTotal => _archivoAudio?.TotalTime ?? TimeSpan.Zero;

        /// <summary>Carga y reproduce el archivo indicado desde el inicio. Detiene lo anterior si había algo sonando.</summary>
        public void Reproducir(string rutaArchivo)
        {
            Detener();

            _archivoAudio = new AudioFileReader(rutaArchivo);
            _dispositivoSalida = new WaveOut();
            _dispositivoSalida.Init(_archivoAudio);
            _dispositivoSalida.Play();
        }

        /// <summary>Pausa la reproducción actual, conservando la posición.</summary>
        public void Pausar()
        {
            if (EstaReproduciendo)
                _dispositivoSalida!.Pause();
        }

        /// <summary>Reanuda la reproducción desde donde se pausó.</summary>
        public void Reanudar()
        {
            if (EstaPausado)
                _dispositivoSalida!.Play();
        }

        /// <summary>Mueve la posición de reproducción al punto indicado (usado por la barra de progreso).</summary>
        public void BuscarPosicion(TimeSpan posicion)
        {
            if (_archivoAudio != null)
                _archivoAudio.CurrentTime = posicion;
        }

        /// <summary>Detiene y libera los recursos de audio actuales (sin lanzar error si no había nada sonando).</summary>
        public void Detener()
        {
            _dispositivoSalida?.Stop();
            _dispositivoSalida?.Dispose();
            _archivoAudio?.Dispose();
            _dispositivoSalida = null;
            _archivoAudio = null;
        }

        public void Dispose() => Detener();
    }
}