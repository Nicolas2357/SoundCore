// ============================================================================
// Archivo:      MainForm.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.0
// ============================================================================

using NAudio.Wave;
using SoundCore.EstructurasPropias;
using SoundCore.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SoundCore.UI
{
    public partial class MainForm : Form
    {
        // Las 3 estructuras se mantienen en paralelo, con los mismos datos
        private readonly ListaSimpleEnlazada<Pista> _colaPropia = new();
        private readonly LinkedList<Pista> _colaLinkedList = new();
        private readonly List<Pista> _colaList = new();
        private readonly AudioManager _audioManager = new();
        private string? _rutaArchivoSeleccionado = null;
        private Pista? _pistaCargadaEnPlayer = null;
        private int _contadorId = 1;
        private Pista? _pistaSonando = null;

        public MainForm()
        {
            InitializeComponent();
            AplicarTemaVisual();
            ConfigurarColumnasGrid();
            CargarDatosSemilla();
            RefrescarVista();
        }

        private void ConfigurarColumnasGrid()
        {
            dgvCola.ColumnCount = 5;
            dgvCola.Columns[0].Name = "Pos";
            dgvCola.Columns[1].Name = "ID";
            dgvCola.Columns[2].Name = "Título / Artista";
            dgvCola.Columns[3].Name = "BPM";
            dgvCola.Columns[4].Name = "Duración";
            dgvCola.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // Datos de ejemplo para que la cola no arranque vacía
        private void CargarDatosSemilla()
        {
            var demo = new[]
            {
                CrearPistaSemilla("The Chain", "Fleetwood Mac", 105,
                    "Fleetwood Mac - The Chain (Official Audio).mp3"),
                CrearPistaSemilla("Am I Dreaming", "Metro Boomin, A$AP Rocky, Roisee", 140,
                    "Metro Boomin, A$AP Rocky, Roisee - Am I Dreaming (Visualizer).mp3"),
                CrearPistaSemilla("Fireball", "Pitbull ft. John Ryan", 128,
                    "Pitbull - Fireball (Official Video) ft. John Ryan.mp3")
            };

            foreach (var p in demo)
            {
                _colaPropia.AgregarAlFinal(p);
                _colaLinkedList.AddLast(p);
                _colaList.Add(p);
            }
        }

        // Arma una pista semilla leyendo su duración real desde el archivo en Assets/
        private Pista CrearPistaSemilla(string titulo, string artista, int bpm, string nombreArchivo)
        {
            string ruta = Path.Combine(AppContext.BaseDirectory, "Assets", nombreArchivo);
            int duracionSegundos = 200; // valor de respaldo si el archivo no se encuentra

            try
            {
                using var lector = new AudioFileReader(ruta);
                duracionSegundos = (int)lector.TotalTime.TotalSeconds;
            }
            catch
            {
                // Si el archivo no existe en este equipo, la pista igual se crea (sin audio real)
                ruta = string.Empty;
            }

            return new Pista(_contadorId++, titulo, artista, bpm, duracionSegundos,
                string.IsNullOrEmpty(ruta) ? null : ruta);
        }

        // Lee los campos del formulario y arma la pista (con valores por defecto si están vacíos)
        private Pista CrearPistaDesdeFormulario()
        {
            string titulo = string.IsNullOrWhiteSpace(txtTitulo.Text)
                ? $"Pista {_contadorId}" : txtTitulo.Text.Trim();
            string artista = string.IsNullOrWhiteSpace(txtArtista.Text)
                ? "DJ Desconocido" : txtArtista.Text.Trim();

            var pista = new Pista(_contadorId++, titulo, artista, (int)numBpm.Value, (int)numDuracion.Value, _rutaArchivoSeleccionado);

            // Se limpia tras crear la pista, para que el próximo registro parta sin archivo
            _rutaArchivoSeleccionado = null;
            lblArchivoSeleccionado.Text = "Sin archivo seleccionado";

            return pista;
        }

        // Redibuja el grid con la estructura que esté seleccionada en los radio buttons
        private void RefrescarVista()
        {
            dgvCola.Rows.Clear();

            IEnumerable<Pista> coleccion = rbPropia.Checked ? _colaPropia
                                         : rbLinkedList.Checked ? _colaLinkedList
                                         : _colaList;

            int index = 1;
            int duracionTotal = 0;

            foreach (var p in coleccion)
            {
                dgvCola.Rows.Add(index++, p.Id, $"{p.Titulo} — {p.Artista}", $"{p.Bpm} BPM", $"{p.DuracionSegundos}s");
                duracionTotal += p.DuracionSegundos;
            }

            lblEstadisticas.Text = $"Total en cola: {index - 1} | Tiempo total: {TimeSpan.FromSeconds(duracionTotal):mm\\:ss}";
        }
        // Cambio en vivo de estructura: al marcar otro radio button se redibuja el grid
        private void rbEstructura_CheckedChanged(object? sender, EventArgs e)
        {
            // Este evento se dispara dos veces (el que se desmarca y el que se marca); solo reaccionamos al marcado
            if (sender is RadioButton rb && rb.Checked)
                RefrescarVista();
        }

        private void btnEncolarFinal_Click(object? sender, EventArgs e)
        {
            var pista = CrearPistaDesdeFormulario();

            if (rbPropia.Checked) _colaPropia.AgregarAlFinal(pista);
            else if (rbLinkedList.Checked) _colaLinkedList.AddLast(pista);
            else _colaList.Add(pista);

            RefrescarVista();
        }
        private void btnExaminar_Click(object? sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Selecciona un archivo de audio",
                Filter = "Archivos de audio (*.mp3;*.wav)|*.mp3;*.wav|Todos los archivos (*.*)|*.*"
            };

            if (dialogo.ShowDialog() != DialogResult.OK) return;

            _rutaArchivoSeleccionado = dialogo.FileName;
            lblArchivoSeleccionado.Text = Path.GetFileName(dialogo.FileName);

            // Duración exacta, leída directamente del archivo
            try
            {
                using var lector = new AudioFileReader(dialogo.FileName);
                int segundos = (int)lector.TotalTime.TotalSeconds;
                numDuracion.Value = Math.Clamp(segundos, (int)numDuracion.Minimum, (int)numDuracion.Maximum);
            }
            catch
            {
                // Si el archivo no se puede leer bien, simplemente no autocompleta la duración
            }

            // Título / Artista, a partir del nombre del archivo (patrón "Artista - Título.mp3")
            string nombreSinExtension = Path.GetFileNameWithoutExtension(dialogo.FileName);
            string[] partes = nombreSinExtension.Split(" - ", 2, StringSplitOptions.TrimEntries);

            if (partes.Length == 2)
            {
                txtArtista.Text = partes[0];
                txtTitulo.Text = partes[1];
            }
            else
            {
                txtTitulo.Text = nombreSinExtension;
            }
        }

        private void btnReproducirSiguiente_Click(object? sender, EventArgs e)
        {
            var pista = CrearPistaDesdeFormulario();

            if (rbPropia.Checked)
            {
                _colaPropia.ReproducirSiguiente(pista);
            }
            else if (rbLinkedList.Checked)
            {
                if (_colaLinkedList.First == null) _colaLinkedList.AddFirst(pista);
                else _colaLinkedList.AddAfter(_colaLinkedList.First, pista);
            }
            else
            {
                if (_colaList.Count <= 1) _colaList.Add(pista);
                else _colaList.Insert(1, pista);
            }

            RefrescarVista();
        }

        private void btnAvanzar_Click(object? sender, EventArgs e)
        {
            try
            {
                Pista actual;

                if (rbPropia.Checked)
                {
                    actual = _colaPropia.AvanzarPista();
                }
                else if (rbLinkedList.Checked)
                {
                    if (_colaLinkedList.First == null) throw new InvalidOperationException();
                    actual = _colaLinkedList.First.Value;
                    _colaLinkedList.RemoveFirst();
                }
                else
                {
                    if (_colaList.Count == 0) throw new InvalidOperationException();
                    actual = _colaList[0];
                    _colaList.RemoveAt(0);
                }

                _pistaSonando = actual;
                _pistaCargadaEnPlayer = actual;
                lblTituloActual.Text = $"▶ {actual.Titulo} — {actual.Artista} ({actual.Bpm} BPM)";
                lblTituloActual.ForeColor = ColorVerde;

                if (!string.IsNullOrEmpty(actual.RutaArchivo))
                {
                    _audioManager.Reproducir(actual.RutaArchivo);
                    tbProgreso.Enabled = true;
                    btnPlayPausa.Enabled = true;
                    btnDetener.Enabled = true;
                    btnPlayPausa.Text = "⏸";
                    timerProgreso.Start();
                }
                else
                {
                    // Esta pista no tiene archivo asociado: se refleja en el player, sin controles activos
                    tbProgreso.Value = 0;
                    tbProgreso.Enabled = false;
                    btnPlayPausa.Enabled = false;
                    btnDetener.Enabled = false;
                    lblTiempo.Text = "0:00 / 0:00";
                    timerProgreso.Stop();
                }

                RefrescarVista();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("No hay pistas pendientes en la cola.", "Fin del Setlist",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            timerProgreso.Stop();
            _audioManager.Dispose();
        }
        private void btnInvertir_Click(object? sender, EventArgs e)
        {
            if (rbPropia.Checked)
            {
                _colaPropia.Invertir();
            }
            else if (rbLinkedList.Checked)
            {
                var temporal = new List<Pista>(_colaLinkedList);
                temporal.Reverse();
                _colaLinkedList.Clear();
                foreach (var item in temporal) _colaLinkedList.AddLast(item);
            }
            else
            {
                _colaList.Reverse();
            }

            RefrescarVista();
        }
        private void btnPlayPausa_Click(object? sender, EventArgs e)
        {
            if (_audioManager.EstaReproduciendo)
            {
                _audioManager.Pausar();
                btnPlayPausa.Text = "▶";
                timerProgreso.Stop();
            }
            else if (_audioManager.EstaPausado)
            {
                _audioManager.Reanudar();
                btnPlayPausa.Text = "⏸";
                timerProgreso.Start();
            }
        }

        private void btnDetener_Click(object? sender, EventArgs e)
        {
            _audioManager.Detener();
            timerProgreso.Stop();

            tbProgreso.Value = 0;
            tbProgreso.Enabled = false;
            btnPlayPausa.Enabled = false;
            btnDetener.Enabled = false;
            btnPlayPausa.Text = "▶";
            lblTiempo.Text = "0:00 / 0:00";
            lblTituloActual.Text = "SIN REPRODUCCIÓN";
            lblTituloActual.ForeColor = ColorTextoSec;
            _pistaCargadaEnPlayer = null;
        }

        // Al soltar el mouse sobre la barra, salta el audio a esa posición
        private void tbProgreso_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_audioManager.HayPistaCargada) return;

            double proporcion = tbProgreso.Value / 100.0;
            var nuevaPosicion = TimeSpan.FromSeconds(_audioManager.DuracionTotal.TotalSeconds * proporcion);
            _audioManager.BuscarPosicion(nuevaPosicion);
        }

        // Cada 500ms: mueve la barra y actualiza el contador de tiempo mientras suena
        private void timerProgreso_Tick(object? sender, EventArgs e)
        {
            if (!_audioManager.HayPistaCargada || _audioManager.DuracionTotal.TotalSeconds <= 0) return;

            double proporcion = _audioManager.PosicionActual.TotalSeconds / _audioManager.DuracionTotal.TotalSeconds;
            tbProgreso.Value = Math.Clamp((int)(proporcion * 100), 0, 100);

            lblTiempo.Text = $"{FormatoTiempo(_audioManager.PosicionActual)} / {FormatoTiempo(_audioManager.DuracionTotal)}";
        }

        private static string FormatoTiempo(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:D2}";
        private void btnOrdenarBpm_Click(object? sender, EventArgs e)
        {
            if (rbPropia.Checked)
            {
                // Se reinserta cada pista con InsertarOrdenado en una lista temporal y luego se copia de vuelta
                var temporal = new ListaSimpleEnlazada<Pista>();
                foreach (var pista in _colaPropia)
                    temporal.InsertarOrdenado(pista, (a, b) => a.Bpm.CompareTo(b.Bpm));

                _colaPropia.Limpiar();
                foreach (var p in temporal) _colaPropia.AgregarAlFinal(p);
            }
            else if (rbLinkedList.Checked)
            {
                var ordenadas = _colaLinkedList.OrderBy(p => p.Bpm).ToList();
                _colaLinkedList.Clear();
                foreach (var p in ordenadas) _colaLinkedList.AddLast(p);
            }
            else
            {
                _colaList.Sort((a, b) => a.Bpm.CompareTo(b.Bpm));
            }

            RefrescarVista();
        }

        private void btnPurgar_Click(object? sender, EventArgs e)
        {
            if (rbPropia.Checked)
            {
                _colaPropia.DepurarDuplicados((a, b) =>
                    a.Titulo.Equals(b.Titulo, StringComparison.OrdinalIgnoreCase));
            }
            else if (rbLinkedList.Checked)
            {
                var unicos = _colaLinkedList.DistinctBy(p => p.Titulo, StringComparer.OrdinalIgnoreCase).ToList();
                _colaLinkedList.Clear();
                foreach (var p in unicos) _colaLinkedList.AddLast(p);
            }
            else
            {
                var unicos = _colaList.DistinctBy(p => p.Titulo, StringComparer.OrdinalIgnoreCase).ToList();
                _colaList.Clear();
                _colaList.AddRange(unicos);
            }

            RefrescarVista();
        }
        // ====== PALETA Dark Cyber-Audio ======
        private static readonly Color ColorFondo = Color.FromArgb(11, 15, 25);
        private static readonly Color ColorPanel = Color.FromArgb(19, 29, 49);
        private static readonly Color ColorTarjeta = Color.FromArgb(24, 35, 60);
        private static readonly Color ColorBorde = Color.FromArgb(36, 53, 86);
        private static readonly Color ColorCyan = Color.FromArgb(0, 240, 255);
        private static readonly Color ColorVerde = Color.FromArgb(16, 185, 129);
        private static readonly Color ColorPurpura = Color.FromArgb(139, 92, 246);
        private static readonly Color ColorTexto = Color.FromArgb(241, 245, 249);
        private static readonly Color ColorTextoSec = Color.FromArgb(148, 163, 184);
        private static readonly Color ColorBotonBase = Color.FromArgb(30, 41, 59);

        /// <summary>Aplica el tema oscuro a toda la interfaz. Llamar tras InitializeComponent().</summary>
        private void AplicarTemaVisual()
        {
            BackColor = ColorFondo;
            ForeColor = ColorTexto;

            // Estilo base por tipo de control (recorre también los que están dentro de GroupBox)
            foreach (var c in TodosLosControles(this))
            {
                switch (c)
                {
                    case GroupBox gb:
                        gb.BackColor = ColorPanel;
                        gb.ForeColor = ColorCyan;
                        gb.Paint += GroupBox_Paint;
                        break;
                    case RadioButton rb:
                        rb.ForeColor = Color.FromArgb(203, 213, 225);
                        rb.BackColor = Color.Transparent;
                        break;
                    case Label lb:
                        lb.ForeColor = ColorTextoSec;
                        lb.BackColor = Color.Transparent;
                        break;
                    case TextBox tb:
                        tb.BackColor = ColorTarjeta;
                        tb.ForeColor = ColorTexto;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case NumericUpDown nud:
                        nud.BackColor = ColorTarjeta;
                        nud.ForeColor = ColorTexto;
                        nud.BorderStyle = BorderStyle.FixedSingle;
                        break;
                }
            }

            // Botones: borde de acento según el tipo de acción
            EstilizarBoton(btnEncolarFinal, ColorCyan);
            EstilizarBoton(btnReproducirSiguiente, ColorCyan);
            EstilizarBoton(btnAvanzar, ColorVerde);
            EstilizarBoton(btnInvertir, ColorPurpura);
            EstilizarBoton(btnOrdenarBpm, ColorPurpura);
            EstilizarBoton(btnPurgar, ColorPurpura);
            EstilizarBoton(btnBenchmark, ColorPurpura);

            EstilizarGrid();
            EstilizarDisplay();

            // Consola de telemetría del benchmark
            txtResultadosBenchmark.BackColor = Color.FromArgb(9, 13, 22);
            txtResultadosBenchmark.ForeColor = ColorVerde;
            txtResultadosBenchmark.Font = new Font("Consolas", 10F);

            lblEstadisticas.ForeColor = ColorTextoSec;
            lblEstadisticas.Font = new Font("Consolas", 9.5F);
        }

        // Botón plano con borde de acento y hover/pressed derivados del mismo color
        private static void EstilizarBoton(Button b, Color acento)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = acento;
            b.FlatAppearance.MouseOverBackColor = Mezclar(ColorBotonBase, acento, 0.25);
            b.FlatAppearance.MouseDownBackColor = Mezclar(ColorBotonBase, acento, 0.45);
            b.BackColor = ColorBotonBase;
            b.ForeColor = ColorTexto;
            b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
        }

        private void EstilizarGrid()
        {
            dgvCola.BorderStyle = BorderStyle.None;
            dgvCola.BackgroundColor = ColorPanel;
            dgvCola.GridColor = ColorBorde;
            dgvCola.EnableHeadersVisualStyles = false;
            dgvCola.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvCola.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvCola.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvCola.ColumnHeadersHeight = 36;
            dgvCola.RowTemplate.Height = 30;

            dgvCola.ColumnHeadersDefaultCellStyle.BackColor = ColorTarjeta;
            dgvCola.ColumnHeadersDefaultCellStyle.ForeColor = ColorCyan;
            dgvCola.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorTarjeta;
            dgvCola.ColumnHeadersDefaultCellStyle.SelectionForeColor = ColorCyan;
            dgvCola.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

            dgvCola.DefaultCellStyle.BackColor = ColorPanel;
            dgvCola.DefaultCellStyle.ForeColor = ColorTexto;
            dgvCola.DefaultCellStyle.SelectionBackColor = Color.FromArgb(13, 92, 111); // cyan mezclado con el fondo
            dgvCola.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCola.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            dgvCola.AlternatingRowsDefaultCellStyle.BackColor = ColorTarjeta;
            dgvCola.AlternatingRowsDefaultCellStyle.ForeColor = ColorTexto;
        }

        // Label "Now Playing" con aspecto de display LED de CDJ
        // Panel "Now Playing" con aspecto de display LED de CDJ
        private void EstilizarDisplay()
        {
            pnlNowPlaying.BackColor = Color.FromArgb(5, 10, 18);

            lblTituloActual.ForeColor = ColorTextoSec; // gris hasta que suene algo (btnAvanzar lo pone en verde)

            lblTiempo.ForeColor = ColorTextoSec;

            EstilizarBoton(btnPlayPausa, ColorVerde);
            EstilizarBoton(btnDetener, ColorPurpura);

            // Borde neón dibujado a mano (BorderStyle no permite cambiar el color)
            pnlNowPlaying.Paint += (s, e) =>
            {
                using var lapiz = new Pen(ColorVerde, 2);
                e.Graphics.DrawRectangle(lapiz, 1, 1, pnlNowPlaying.Width - 3, pnlNowPlaying.Height - 3);
            };
        }

        // Borde y título del GroupBox con los colores del tema
        private void GroupBox_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not GroupBox gb) return;

            var g = e.Graphics;
            g.Clear(gb.BackColor);

            using var fuente = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            var tam = TextRenderer.MeasureText(gb.Text, fuente);

            using var lapiz = new Pen(ColorBorde);
            g.DrawRectangle(lapiz, 0, tam.Height / 2, gb.Width - 1, gb.Height - tam.Height / 2 - 1);

            // Hueco en el borde para que el título quede "encima" de la línea
            using var fondo = new SolidBrush(gb.BackColor);
            g.FillRectangle(fondo, 8, 0, tam.Width, tam.Height);
            TextRenderer.DrawText(g, gb.Text, fuente, new Point(8, 0), ColorCyan);
        }

        // Mezcla dos colores: t = 0 devuelve 'a', t = 1 devuelve 'b'
        private static Color Mezclar(Color a, Color b, double t) =>
            Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));

        // Devuelve todos los controles del formulario, incluidos los anidados
        private static IEnumerable<Control> TodosLosControles(Control raiz)
        {
            foreach (Control hijo in raiz.Controls)
            {
                yield return hijo;
                foreach (var nieto in TodosLosControles(hijo))
                    yield return nieto;
            }
        }

        private void btnBenchmark_Click(object? sender, EventArgs e)
        {
            int n = 20_000;
            var random = new Random(42);
            var sw = new Stopwatch();

            // 1. Inserción intermedia (justo tras la cabeza): Lista propia, O(1) cada una
            var testPropia = new ListaSimpleEnlazada<Pista>();
            testPropia.AgregarAlFinal(new Pista(0, "Head", "DJ", 120, 200));
            sw.Start();
            for (int i = 0; i < n; i++)
                testPropia.ReproducirSiguiente(new Pista(i, $"Pista {i}", "DJ", random.Next(100, 150), 180));
            sw.Stop();
            long tiempoPropia = sw.ElapsedMilliseconds;

            // 2. Inserción intermedia: LinkedList<T> nativa, O(1) cada una
            var testLinked = new LinkedList<Pista>();
            testLinked.AddLast(new Pista(0, "Head", "DJ", 120, 200));
            sw.Restart();
            for (int i = 0; i < n; i++)
                testLinked.AddAfter(testLinked.First!, new Pista(i, $"Pista {i}", "DJ", random.Next(100, 150), 180));
            sw.Stop();
            long tiempoLinked = sw.ElapsedMilliseconds;

            // 3. Inserción intermedia: List<T>, O(n) cada una por el desplazamiento del arreglo
            var testList = new List<Pista> { new Pista(0, "Head", "DJ", 120, 200) };
            sw.Restart();
            for (int i = 0; i < n; i++)
                testList.Insert(1, new Pista(i, $"Pista {i}", "DJ", random.Next(100, 150), 180));
            sw.Stop();
            long tiempoList = sw.ElapsedMilliseconds;

            txtResultadosBenchmark.Text =
                $"=== RESULTADOS ({n:N0} INSERCIONES INTERMEDIAS) ===\r\n" +
                $"• Lista Enlazada Propia:    {tiempoPropia} ms  [O(1) por reconexión de punteros]\r\n" +
                $"• LinkedList<T> de .NET:    {tiempoLinked} ms  [O(1) por reconexión de punteros]\r\n" +
                $"• List<T> (arreglo):        {tiempoList} ms  [O(n) por desplazamiento de memoria]";
        }
    }
}