// ============================================================================
// Archivo:      AudioManager.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
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

        /// <summary>True si la pista llegó al final por sí sola (sigue cargada, pero ya no suena).</summary>
        public bool Termino => _archivoAudio != null && _dispositivoSalida?.PlaybackState == PlaybackState.Stopped;

        public TimeSpan PosicionActual => _archivoAudio?.CurrentTime ?? TimeSpan.Zero;
        public TimeSpan DuracionTotal => _archivoAudio?.TotalTime ?? TimeSpan.Zero;

        /// <summary>Carga y reproduce el archivo indicado desde el inicio. Detiene lo anterior si había algo sonando.</summary>
        public void Reproducir(string rutaArchivo)
        {
            Detener();

            try
            {
                _archivoAudio = new AudioFileReader(rutaArchivo);
                _dispositivoSalida = new WaveOut();
                _dispositivoSalida.Init(_archivoAudio);
                _dispositivoSalida.Play();
            }
            catch
            {
                Detener(); // no dejar recursos a medio crear si el archivo está dañado o ya no existe
                throw;
            }
        }

        /// <summary>Pausa la reproducción actual, conservando la posición.</summary>
        public void Pausar()
        {
            if (EstaReproduciendo)
                _dispositivoSalida!.Pause();
        }

        /// <summary>Reanuda desde donde se pausó; si la pista ya terminó, la vuelve a tocar desde el inicio.</summary>
        public void Reanudar()
        {
            if (EstaPausado)
            {
                _dispositivoSalida!.Play();
            }
            else if (Termino)
            {
                // Si quedó al final, se vuelve al inicio; si el usuario movió la barra, sigue desde ahí
                if (_archivoAudio!.CurrentTime >= _archivoAudio.TotalTime - TimeSpan.FromMilliseconds(250))
                    _archivoAudio.CurrentTime = TimeSpan.Zero;
                _dispositivoSalida!.Play();
            }
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