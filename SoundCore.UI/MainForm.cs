// ============================================================================
// Archivo:      MainForm.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// Cambios 2.0:  • BPM real: se lee de la etiqueta ID3 o se calcula del audio.
//               • Manejo de archivos sin BPM (aviso, TAP, encolar sin BPM).
//               • Benchmark determinista (sin Random) con mediana de 5 corridas.
//               • Interfaz renovada: encabezado, display con LED de beat,
//                 grid con columnas separadas, gráfico del benchmark, barra
//                 de estado con el tiempo de cada operación, arrastrar y soltar.
// ============================================================================

using NAudio.Wave;
using SoundCore.EstructurasPropias;
using SoundCore.Modelos;
using SoundCore.UI.Audio;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace SoundCore.UI
{
    public partial class MainForm : Form
    {
        // Las 3 estructuras arrancan con los mismos datos; cada acción opera sobre la estructura activa
        private readonly ListaSimpleEnlazada<Pista> _colaPropia = new();
        private readonly LinkedList<Pista> _colaLinkedList = new();
        private readonly List<Pista> _colaList = new();
        private readonly AudioManager _audioManager = new();
        private readonly List<Pista> _pistasSemilla = new();
        private string? _rutaArchivoSeleccionado;
        private Pista? _pistaSonando;
        private int _contadorId = 1;

        // ---- Estado del BPM del formulario de registro ----
        private OrigenBpm _origenBpmFormulario = OrigenBpm.Manual;
        private bool _cambiandoBpmPorCodigo;
        private int _versionAnalisis;          // descarta resultados de un archivo anterior
        private readonly List<long> _toquesTap = new();
        private readonly Stopwatch _relojTap = Stopwatch.StartNew();

        // ---- Layout vertical (el grid crece cuando la ventana es más alta) ----
        private bool _layoutCapturado;
        private int _altoContenidoBase, _altoGridBase, _sepGridEstadisticas, _sepEstadisticasBenchmark, _sepBenchmarkEstado;

        // BPM publicado de las pistas demo: solo se usa si el archivo no está en Assets/
        private static readonly (string Titulo, string Artista, int BpmRespaldo, string Archivo)[] Semillas =
        {
            ("The Chain", "Fleetwood Mac", 152, "Fleetwood Mac - The Chain (Official Audio).mp3"),
            ("Am I Dreaming", "Metro Boomin, A$AP Rocky, Roisee", 90, "Metro Boomin, A$AP Rocky, Roisee - Am I Dreaming (Visualizer).mp3"),
            ("Fireball", "Pitbull ft. John Ryan", 123, "Pitbull - Fireball (Official Video) ft. John Ryan.mp3"),
        };

        private const string TextoBpmPorDefecto = "BPM manual · carga un archivo para calcularlo";
        private const string TextoArchivoPorDefecto = "Sin archivo · o arrástralo aquí";
        private const string TextoBotonBenchmark = "▶  EJECUTAR BENCHMARK (20,000 ITEMS)";

        public MainForm()
        {
            InitializeComponent();

            ActivarDobleBufer(pnlHeader);
            ActivarDobleBufer(pnlNowPlaying);
            ActivarDobleBufer(dgvCola);

            AplicarTemaVisual();
            ConfigurarColumnasGrid();
            ConfigurarAyudas();
            ConfigurarArrastrarYSoltar();

            CargarDatosSemilla();
            RefrescarVista();
            ActualizarEstadoBpm(TextoBpmPorDefecto, ColorTextoSec);
            MostrarEstado("Listo. Carga o arrastra un archivo de audio para calcular su BPM automáticamente.");
        }

        // ====================================================================
        // Arranque y layout
        // ====================================================================

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // En pantallas pequeñas la ventana se ajusta al área visible (el contenido se desplaza)
            var area = Screen.FromControl(this).WorkingArea;
            if (Height > area.Height) { Height = area.Height; Top = area.Top; }
            if (Width > area.Width) { Width = area.Width; Left = area.Left; }
        }

        private async void MainForm_Shown(object? sender, EventArgs e)
        {
            CapturarLayoutBase();
            MinimumSize = new Size(Math.Min(Width, Screen.FromControl(this).WorkingArea.Width), Math.Min(560, Height));
            AjustarLayoutVertical();

            await AnalizarPistasSemillaAsync();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            AjustarLayoutVertical();
            pnlHeader.Invalidate();
            pnlNowPlaying.Invalidate();
        }

        // Se toma la geometría ya escalada (DPI) como referencia
        private void CapturarLayoutBase()
        {
            _altoContenidoBase = pnlEstado.Bottom - AutoScrollPosition.Y;
            _altoGridBase = dgvCola.Height;
            _sepGridEstadisticas = lblEstadisticas.Top - dgvCola.Bottom;
            _sepEstadisticasBenchmark = gbBenchmark.Top - lblEstadisticas.Bottom;
            _sepBenchmarkEstado = pnlEstado.Top - gbBenchmark.Bottom;
            _layoutCapturado = true;
        }

        private void AjustarLayoutVertical()
        {
            if (!_layoutCapturado) return;

            int extra = Math.Max(0, ClientSize.Height - _altoContenidoBase);
            SuspendLayout();
            dgvCola.Height = _altoGridBase + extra;
            lblEstadisticas.Top = dgvCola.Bottom + _sepGridEstadisticas;
            gbBenchmark.Top = lblEstadisticas.Bottom + _sepEstadisticasBenchmark;
            pnlEstado.Top = gbBenchmark.Bottom + _sepBenchmarkEstado;
            ResumeLayout();
        }

        // Datos de ejemplo para que la cola no arranque vacía
        private void CargarDatosSemilla()
        {
            foreach (var (titulo, artista, bpm, archivo) in Semillas)
            {
                var p = CrearPistaSemilla(titulo, artista, bpm, archivo);
                _colaPropia.AgregarAlFinal(p);
                _colaLinkedList.AddLast(p);
                _colaList.Add(p);
                if (p.RutaArchivo != null) _pistasSemilla.Add(p);
            }
        }

        // Arma una pista semilla leyendo su duración real desde el archivo en Assets/
        private Pista CrearPistaSemilla(string titulo, string artista, int bpmRespaldo, string nombreArchivo)
        {
            string? ruta = Path.Combine(AppContext.BaseDirectory, "Assets", nombreArchivo);
            int duracionSegundos = 200; // valor de respaldo si el archivo no se encuentra

            try
            {
                using var lector = new AudioFileReader(ruta);
                duracionSegundos = (int)Math.Round(lector.TotalTime.TotalSeconds);
            }
            catch
            {
                // Si el archivo no existe en este equipo, la pista igual se crea (sin audio real)
                ruta = null;
            }

            return new Pista(_contadorId++, titulo, artista, bpmRespaldo, duracionSegundos, ruta, OrigenBpm.Manual);
        }

        // Al abrir la ventana se calcula el BPM real de las pistas demo (en segundo plano, sin congelar la UI)
        private async Task AnalizarPistasSemillaAsync()
        {
            if (_pistasSemilla.Count == 0) return;

            MostrarEstado("Calculando el BPM real de las pistas demo a partir de su audio…");
            var tareas = _pistasSemilla.Select(p => Task.Run(() => (Pista: p, Info: AnalizadorBpm.Obtener(p.RutaArchivo!))));
            var resultados = await Task.WhenAll(tareas);
            if (IsDisposed) return;

            var resumen = new List<string>();
            foreach (var (pista, info) in resultados)
            {
                if (info.Bpm is int bpm)
                {
                    ReemplazarPista(pista, pista with { Bpm = bpm, OrigenBpm = info.Origen });
                    resumen.Add($"{pista.Titulo} {bpm}");
                }
                else
                {
                    resumen.Add($"{pista.Titulo} sin pulso claro (se queda en {pista.Bpm})");
                }
            }

            RefrescarVista();
            MostrarEstado("BPM de las pistas demo: " + string.Join("  ·  ", resumen), TipoEstado.Exito);
        }

        // Los records son inmutables: para "actualizar" una pista se cambia el valor del nodo / posición
        private void ReemplazarPista(Pista vieja, Pista nueva)
        {
            for (var nodo = _colaPropia.Cabeza; nodo != null; nodo = nodo.Siguiente)
                if (ReferenceEquals(nodo.Valor, vieja)) nodo.Valor = nueva;

            for (var nodo = _colaLinkedList.First; nodo != null; nodo = nodo.Next)
                if (ReferenceEquals(nodo.Value, vieja)) nodo.Value = nueva;

            for (int i = 0; i < _colaList.Count; i++)
                if (ReferenceEquals(_colaList[i], vieja)) _colaList[i] = nueva;

            if (ReferenceEquals(_pistaSonando, vieja))
            {
                _pistaSonando = nueva;
                MostrarBpmEnDisplay(nueva);
            }
        }

        // ====================================================================
        // Registro de pista: archivo, BPM automático, TAP
        // ====================================================================

        private void btnExaminar_Click(object? sender, EventArgs e)
        {
            using var dialogo = new OpenFileDialog
            {
                Title = "Selecciona un archivo de audio",
                Filter = "Archivos de audio (*.mp3;*.wav)|*.mp3;*.wav|Todos los archivos (*.*)|*.*"
            };

            if (dialogo.ShowDialog(this) != DialogResult.OK) return;
            CargarArchivo(dialogo.FileName);
        }

        private async void CargarArchivo(string ruta)
        {
            _rutaArchivoSeleccionado = ruta;
            lblArchivoSeleccionado.Text = Path.GetFileName(ruta);
            lblArchivoSeleccionado.ForeColor = ColorTexto;
            ttAyuda.SetToolTip(lblArchivoSeleccionado, ruta);

            // Duración exacta, leída directamente del archivo
            try
            {
                using var lector = new AudioFileReader(ruta);
                int segundos = (int)Math.Round(lector.TotalTime.TotalSeconds);
                numDuracion.Value = Math.Clamp(segundos, (int)numDuracion.Minimum, (int)numDuracion.Maximum);
            }
            catch
            {
                MostrarEstado("No se pudo leer la duración del archivo; ajústala a mano.", TipoEstado.Aviso);
            }

            // Título / Artista, a partir del nombre del archivo (patrón "Artista - Título.mp3")
            string nombreSinExtension = Path.GetFileNameWithoutExtension(ruta);
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

            // BPM: etiqueta del archivo → análisis del audio → aviso "sin BPM"
            int version = ++_versionAnalisis;
            EstablecerAnalizando(true);
            ActualizarEstadoBpm("⏳ Buscando el BPM del archivo…", ColorCyan);

            InfoBpm info = await Task.Run(() => AnalizadorBpm.Obtener(ruta));

            if (IsDisposed || version != _versionAnalisis) return; // mientras tanto se eligió otro archivo
            EstablecerAnalizando(false);
            AplicarInfoBpm(info, Path.GetFileName(ruta));
        }

        private void AplicarInfoBpm(InfoBpm info, string nombreArchivo)
        {
            if (info.Bpm is int bpm)
            {
                EstablecerBpmPorCodigo(bpm);
                _origenBpmFormulario = info.Origen;
                ActualizarEstadoBpm((info.Origen == OrigenBpm.Etiqueta ? "✔ " : "⟳ ") + info.Mensaje, ColorVerde);
                MostrarEstado($"BPM de «{nombreArchivo}»: {bpm} ({DescripcionFuente(info.Origen).ToLowerInvariant()}).", TipoEstado.Exito);
            }
            else
            {
                // El archivo no tiene BPM: no se inventa un valor, se le pide al usuario
                _origenBpmFormulario = OrigenBpm.SinBpm;
                ActualizarEstadoBpm("⚠ " + info.Mensaje, ColorAmbar);
                MostrarEstado($"«{nombreArchivo}» no tiene BPM: escríbelo, márcalo con TAP o encola la pista sin BPM.", TipoEstado.Aviso);
            }
        }

        private void EstablecerAnalizando(bool analizando)
        {
            btnEncolarFinal.Enabled = !analizando;
            btnReproducirSiguiente.Enabled = !analizando;
            gbRegistro.Cursor = analizando ? Cursors.AppStarting : Cursors.Default;
        }

        private void EstablecerBpmPorCodigo(int bpm)
        {
            _cambiandoBpmPorCodigo = true;
            try
            {
                numBpm.Value = Math.Clamp(bpm, (int)numBpm.Minimum, (int)numBpm.Maximum);
            }
            finally
            {
                _cambiandoBpmPorCodigo = false;
            }
        }

        // Si el usuario escribe el BPM, la pista pasa a tener BPM "manual"
        private void numBpm_ValueChanged(object? sender, EventArgs e)
        {
            if (_cambiandoBpmPorCodigo) return;
            _origenBpmFormulario = OrigenBpm.Manual;
            ActualizarEstadoBpm("✎ BPM capturado manualmente", ColorTexto);
        }

        // Tap tempo: el DJ marca el pulso con clics y se promedian los intervalos
        private void btnTap_Click(object? sender, EventArgs e)
        {
            long ahora = _relojTap.ElapsedMilliseconds;
            if (_toquesTap.Count > 0 && ahora - _toquesTap[^1] > 2000) _toquesTap.Clear(); // pausa larga = nueva medición
            _toquesTap.Add(ahora);
            if (_toquesTap.Count > 8) _toquesTap.RemoveAt(0);

            if (_toquesTap.Count < 2)
            {
                ActualizarEstadoBpm("TAP: sigue marcando el pulso…", ColorCyan);
                return;
            }

            double intervaloMs = (_toquesTap[^1] - _toquesTap[0]) / (double)(_toquesTap.Count - 1);
            double bpm = 60_000.0 / intervaloMs;
            EstablecerBpmPorCodigo((int)Math.Round(bpm));
            _origenBpmFormulario = OrigenBpm.Manual;
            ActualizarEstadoBpm($"✎ TAP: {_toquesTap.Count} toques → {bpm:0.0} BPM", ColorTexto);
        }

        private void ActualizarEstadoBpm(string texto, Color color)
        {
            lblEstadoBpm.Text = texto;
            lblEstadoBpm.ForeColor = color;
        }

        // Lee los campos del formulario y arma la pista. Devuelve null si el usuario cancela.
        private Pista? CrearPistaDesdeFormulario()
        {
            string titulo = string.IsNullOrWhiteSpace(txtTitulo.Text)
                ? $"Pista {_contadorId}" : txtTitulo.Text.Trim();
            string artista = string.IsNullOrWhiteSpace(txtArtista.Text)
                ? "DJ Desconocido" : txtArtista.Text.Trim();

            int bpm = (int)numBpm.Value;
            OrigenBpm origen = _origenBpmFormulario;

            // Caso especial: el archivo no tiene BPM y no se pudo calcular
            if (origen == OrigenBpm.SinBpm)
            {
                var respuesta = MessageBox.Show(this,
                    $"El archivo «{lblArchivoSeleccionado.Text}» no tiene BPM y no se pudo calcular de su audio.\n\n" +
                    $"• Sí: encolar con {bpm} BPM (el valor escrito en el formulario).\n" +
                    "• No: encolar SIN BPM (al ordenar por curva BPM quedará al final).\n" +
                    "• Cancelar: volver para escribirlo o marcarlo con TAP.",
                    "Pista sin BPM", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (respuesta == DialogResult.Cancel)
                {
                    numBpm.Focus();
                    return null;
                }
                if (respuesta == DialogResult.Yes) origen = OrigenBpm.Manual;
                else bpm = 0;
            }

            var pista = new Pista(_contadorId++, titulo, artista, bpm, (int)numDuracion.Value, _rutaArchivoSeleccionado, origen);

            // Se limpia tras crear la pista, para que el próximo registro parta sin archivo
            _rutaArchivoSeleccionado = null;
            _origenBpmFormulario = OrigenBpm.Manual;
            lblArchivoSeleccionado.Text = TextoArchivoPorDefecto;
            lblArchivoSeleccionado.ForeColor = ColorTextoSec;
            ttAyuda.SetToolTip(lblArchivoSeleccionado, null);
            ActualizarEstadoBpm(TextoBpmPorDefecto, ColorTextoSec);

            return pista;
        }

        // ====================================================================
        // Arrastrar y soltar archivos de audio sobre la ventana
        // ====================================================================

        private void ConfigurarArrastrarYSoltar()
        {
            foreach (var c in TodosLosControles(this).Prepend(this))
            {
                c.AllowDrop = true;
                c.DragEnter += Control_DragEnter;
                c.DragDrop += Control_DragDrop;
            }
        }

        private static string? PrimerAudio(IDataObject? datos)
        {
            if (datos?.GetData(DataFormats.FileDrop) is not string[] archivos) return null;
            return archivos.FirstOrDefault(a =>
                a.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                a.EndsWith(".wav", StringComparison.OrdinalIgnoreCase));
        }

        private void Control_DragEnter(object? sender, DragEventArgs e) =>
            e.Effect = PrimerAudio(e.Data) != null ? DragDropEffects.Copy : DragDropEffects.None;

        private void Control_DragDrop(object? sender, DragEventArgs e)
        {
            string? ruta = PrimerAudio(e.Data);
            if (ruta != null) CargarArchivo(ruta);
        }

        // ====================================================================
        // Vista de la cola
        // ====================================================================

        private IEnumerable<Pista> ColeccionActiva() =>
            rbPropia.Checked ? _colaPropia : rbLinkedList.Checked ? _colaLinkedList : _colaList;

        private string NombreEstructuraActiva() =>
            rbPropia.Checked ? "Lista Propia" : rbLinkedList.Checked ? "LinkedList<T>" : "List<T>";

        private Color ColorEstructuraActiva() =>
            rbPropia.Checked ? ColorCyan : rbLinkedList.Checked ? ColorVerde : ColorPurpura;

        private void ConfigurarColumnasGrid()
        {
            dgvCola.Columns.Clear();
            AgregarColumna("Pos", "#", 56, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("Id", "ID", 50, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("Titulo", "TÍTULO", 0, DataGridViewContentAlignment.MiddleLeft, 55);
            AgregarColumna("Artista", "ARTISTA", 0, DataGridViewContentAlignment.MiddleLeft, 45);
            AgregarColumna("Bpm", "BPM", 64, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("Fuente", "FUENTE BPM", 90, DataGridViewContentAlignment.MiddleCenter);
            AgregarColumna("Duracion", "DURACIÓN", 104, DataGridViewContentAlignment.MiddleCenter);

            // Mensaje cuando la cola está vacía
            dgvCola.Paint += (s, e) =>
            {
                if (dgvCola.Rows.Count > 0) return;
                var area = new Rectangle(0, dgvCola.ColumnHeadersHeight, dgvCola.ClientSize.Width,
                    Math.Max(0, dgvCola.ClientSize.Height - dgvCola.ColumnHeadersHeight));
                TextRenderer.DrawText(e.Graphics,
                    "La cola está vacía\nRegistra una pista y presiona «Encolar al final»",
                    FuenteNormal, area, ColorTextoSec,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            };
        }

        private void AgregarColumna(string nombre, string encabezado, int ancho, DataGridViewContentAlignment alineacion, float pesoRelleno = 0)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = nombre,
                HeaderText = encabezado,
                SortMode = DataGridViewColumnSortMode.NotSortable, // el orden lo deciden las estructuras, no el grid
                ReadOnly = true
            };

            if (pesoRelleno > 0)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.FillWeight = pesoRelleno;
                col.MinimumWidth = 80;
            }
            else
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                col.Width = ancho;
            }

            col.DefaultCellStyle.Alignment = alineacion;
            col.HeaderCell.Style.Alignment = alineacion;
            dgvCola.Columns.Add(col);
        }

        // Redibuja el grid con la estructura que esté seleccionada en los radio buttons
        private void RefrescarVista()
        {
            dgvCola.SuspendLayout();
            dgvCola.Rows.Clear();

            int pos = 0, duracionTotal = 0, conBpm = 0, sumaBpm = 0, minBpm = int.MaxValue, maxBpm = 0;

            foreach (var p in ColeccionActiva())
            {
                pos++;
                int indice = dgvCola.Rows.Add(
                    pos == 1 ? "▶ 1" : pos.ToString(),
                    p.Id,
                    p.Titulo,
                    p.Artista,
                    p.TieneBpm ? p.Bpm.ToString() : "—",
                    TextoFuente(p.OrigenBpm),
                    FormatoDuracion(p.DuracionSegundos));

                var fila = dgvCola.Rows[indice];
                var celdaBpm = fila.Cells["Bpm"];
                celdaBpm.Style.ForeColor = ColorParaBpm(p);
                celdaBpm.Style.Font = FuenteNegrita;
                celdaBpm.ToolTipText = DescripcionFuente(p.OrigenBpm);
                fila.Cells["Fuente"].Style.ForeColor = ColorFuente(p.OrigenBpm);
                fila.Cells["Fuente"].ToolTipText = DescripcionFuente(p.OrigenBpm);
                fila.Cells["Titulo"].ToolTipText = string.IsNullOrEmpty(p.RutaArchivo)
                    ? "Pista sin archivo de audio (solo simulación)"
                    : Path.GetFileName(p.RutaArchivo);

                if (pos == 1)
                {
                    // La cabeza de la lista es la siguiente en sonar
                    fila.DefaultCellStyle.Font = FuenteNegrita;
                    fila.Cells["Pos"].Style.ForeColor = ColorVerde;
                    fila.Cells["Pos"].ToolTipText = "Siguiente en sonar (cabeza de la lista)";
                }

                duracionTotal += p.DuracionSegundos;
                if (p.TieneBpm)
                {
                    conBpm++;
                    sumaBpm += p.Bpm;
                    minBpm = Math.Min(minBpm, p.Bpm);
                    maxBpm = Math.Max(maxBpm, p.Bpm);
                }
            }

            dgvCola.ResumeLayout();
            dgvCola.ClearSelection();
            dgvCola.Invalidate();

            string textoBpm = conBpm > 0
                ? $"BPM promedio {sumaBpm / (double)conBpm:0}  ·  rango {minBpm}–{maxBpm}"
                : "sin datos de BPM";
            int sinBpm = pos - conBpm;

            lblEstadisticas.Text =
                $"{pos} pista(s) en cola  ·  duración total {FormatoDuracion(duracionTotal)}  ·  {textoBpm}" +
                (sinBpm > 0 ? $"  ·  ⚠ {sinBpm} sin BPM" : string.Empty);

            pnlHeader.Invalidate();
        }

        // Cambio en vivo de estructura: al marcar otro radio button se redibuja el grid
        private void rbEstructura_CheckedChanged(object? sender, EventArgs e)
        {
            // Este evento se dispara dos veces (el que se desmarca y el que se marca); solo reaccionamos al marcado
            if (sender is not RadioButton rb || !rb.Checked) return;

            RefrescarVista();
            MostrarEstado($"Mostrando la cola de {NombreEstructuraActiva()} ({ColeccionActiva().Count()} pistas).");
        }

        // ====================================================================
        // Acciones de cola (cada una mide su tiempo y lo muestra en la barra de estado)
        // ====================================================================

        private static double Medir(Action accion)
        {
            var sw = Stopwatch.StartNew();
            accion();
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private void btnEncolarFinal_Click(object? sender, EventArgs e)
        {
            var pista = CrearPistaDesdeFormulario();
            if (pista is null) return;

            double ms = Medir(() =>
            {
                if (rbPropia.Checked) _colaPropia.AgregarAlFinal(pista);
                else if (rbLinkedList.Checked) _colaLinkedList.AddLast(pista);
                else _colaList.Add(pista);
            });

            RefrescarVista();
            MostrarEstado($"«{pista.Titulo}» encolada al final · {NombreEstructuraActiva()} · {ms:0.000} ms", TipoEstado.Exito);
        }

        private void btnReproducirSiguiente_Click(object? sender, EventArgs e)
        {
            var pista = CrearPistaDesdeFormulario();
            if (pista is null) return;

            double ms = Medir(() =>
            {
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
            });

            RefrescarVista();
            MostrarEstado($"«{pista.Titulo}» insertada como «Up Next» · {NombreEstructuraActiva()} · {ms:0.000} ms", TipoEstado.Exito);
        }

        // Saca la cabeza de la estructura activa (lanza InvalidOperationException si está vacía)
        private Pista DesencolarActiva()
        {
            if (rbPropia.Checked) return _colaPropia.AvanzarPista();

            if (rbLinkedList.Checked)
            {
                var primero = _colaLinkedList.First ?? throw new InvalidOperationException("La cola está vacía.");
                Pista valor = primero.Value;
                _colaLinkedList.RemoveFirst();
                return valor;
            }

            if (_colaList.Count == 0) throw new InvalidOperationException("La cola está vacía.");
            Pista p = _colaList[0];
            _colaList.RemoveAt(0);
            return p;
        }

        private void btnAvanzar_Click(object? sender, EventArgs e)
        {
            Pista actual;
            double ms;
            try
            {
                Pista? sacada = null;
                ms = Medir(() => sacada = DesencolarActiva());
                actual = sacada!;
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(this, "No hay pistas pendientes en la cola.", "Fin del Setlist",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                MostrarEstado("La cola está vacía: no hay pista para avanzar.", TipoEstado.Aviso);
                return;
            }

            _pistaSonando = actual;
            lblTituloActual.Text = $"{actual.Titulo} — {actual.Artista}";
            lblTituloActual.ForeColor = ColorVerde;
            MostrarBpmEnDisplay(actual);
            barraProgreso.Valor = 0;

            if (!string.IsNullOrEmpty(actual.RutaArchivo))
            {
                // El audio se maneja aparte: un archivo dañado no debe confundirse con "cola vacía"
                try
                {
                    _audioManager.Reproducir(actual.RutaArchivo);
                    ActivarControlesReproductor(true);
                    btnPlayPausa.Text = "⏸";
                    timerProgreso.Start();
                    MostrarEstado($"Sonando «{actual.Titulo}» · desencolada de {NombreEstructuraActiva()} en {ms:0.000} ms", TipoEstado.Exito);
                }
                catch (Exception ex)
                {
                    ActivarControlesReproductor(false);
                    lblTiempo.Text = "Audio no disponible";
                    MostrarEstado($"No se pudo reproducir «{Path.GetFileName(actual.RutaArchivo)}»: {ex.Message}", TipoEstado.Error);
                }
            }
            else
            {
                // Esta pista no tiene archivo asociado: se refleja en el player, sin controles activos
                _audioManager.Detener();
                ActivarControlesReproductor(false);
                lblTiempo.Text = "Pista sin archivo de audio (simulación)";
                MostrarEstado($"«{actual.Titulo}» avanzada (sin archivo de audio) · {ms:0.000} ms", TipoEstado.Info);
            }

            RefrescarVista();
        }

        private void btnInvertir_Click(object? sender, EventArgs e)
        {
            double ms = Medir(() =>
            {
                if (rbPropia.Checked)
                {
                    _colaPropia.Invertir(); // in-place, O(1) memoria
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
            });

            RefrescarVista();
            MostrarEstado($"Cola invertida · {NombreEstructuraActiva()} · {ms:0.000} ms", TipoEstado.Exito);
        }

        private void btnOrdenarBpm_Click(object? sender, EventArgs e)
        {
            // Mismo criterio en las 3 estructuras: BPM ascendente, sin BPM al final, y orden ESTABLE
            // (las pistas con el mismo BPM conservan su orden relativo).
            var criterio = Comparer<Pista>.Create(Pista.CompararPorBpm);

            double ms = Medir(() =>
            {
                if (rbPropia.Checked)
                {
                    // Se reinserta cada pista con InsertarOrdenado en una lista temporal y luego se copia de vuelta
                    var temporal = new ListaSimpleEnlazada<Pista>();
                    foreach (var pista in _colaPropia)
                        temporal.InsertarOrdenado(pista, Pista.CompararPorBpm);

                    _colaPropia.Limpiar();
                    foreach (var p in temporal) _colaPropia.AgregarAlFinal(p);
                }
                else if (rbLinkedList.Checked)
                {
                    var ordenadas = _colaLinkedList.OrderBy(p => p, criterio).ToList();
                    _colaLinkedList.Clear();
                    foreach (var p in ordenadas) _colaLinkedList.AddLast(p);
                }
                else
                {
                    // List<T>.Sort no es estable (introsort): con BPM repetidos daría un orden distinto
                    // al de las otras estructuras, por eso se usa OrderBy, que sí es estable.
                    var ordenadas = _colaList.OrderBy(p => p, criterio).ToList();
                    _colaList.Clear();
                    _colaList.AddRange(ordenadas);
                }
            });

            RefrescarVista();
            MostrarEstado($"Cola ordenada por curva BPM (menor → mayor) · {NombreEstructuraActiva()} · {ms:0.000} ms", TipoEstado.Exito);
        }

        private void btnPurgar_Click(object? sender, EventArgs e)
        {
            int antes = ColeccionActiva().Count();

            double ms = Medir(() =>
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
            });

            int eliminados = antes - ColeccionActiva().Count();
            RefrescarVista();
            MostrarEstado(eliminados > 0
                ? $"Se eliminaron {eliminados} duplicado(s) · {NombreEstructuraActiva()} · {ms:0.000} ms"
                : $"No había duplicados · {NombreEstructuraActiva()} · {ms:0.000} ms",
                eliminados > 0 ? TipoEstado.Exito : TipoEstado.Info);
        }

        // ====================================================================
        // Reproductor
        // ====================================================================

        private void ActivarControlesReproductor(bool activo)
        {
            barraProgreso.Enabled = activo;
            btnPlayPausa.Enabled = activo;
            btnDetener.Enabled = activo;
            if (!activo)
            {
                timerProgreso.Stop();
                barraProgreso.Valor = 0;
                btnPlayPausa.Text = "▶";
                indicadorBeat.Intensidad = 0;
            }
        }

        private void MostrarBpmEnDisplay(Pista pista)
        {
            lblBpmActual.Text = pista.TieneBpm ? $"{pista.Bpm} BPM" : "— BPM";
            lblBpmActual.ForeColor = ColorParaBpm(pista);
            ttAyuda.SetToolTip(lblBpmActual, DescripcionFuente(pista.OrigenBpm));
        }

        private void btnPlayPausa_Click(object? sender, EventArgs e)
        {
            if (_audioManager.EstaReproduciendo)
            {
                _audioManager.Pausar();
                btnPlayPausa.Text = "▶";
                timerProgreso.Stop();
                indicadorBeat.Intensidad = 0;
                MostrarEstado("En pausa.");
            }
            else if (_audioManager.EstaPausado || _audioManager.Termino)
            {
                _audioManager.Reanudar();
                btnPlayPausa.Text = "⏸";
                timerProgreso.Start();
                MostrarEstado($"Sonando «{_pistaSonando?.Titulo}».");
            }
        }

        private void btnDetener_Click(object? sender, EventArgs e)
        {
            _audioManager.Detener();
            ActivarControlesReproductor(false);

            lblTiempo.Text = "0:00 / 0:00";
            lblTituloActual.Text = "SIN REPRODUCCIÓN";
            lblTituloActual.ForeColor = ColorTextoSec;
            lblBpmActual.Text = "— BPM";
            lblBpmActual.ForeColor = ColorTextoSec;
            _pistaSonando = null;
            MostrarEstado("Reproducción detenida.");
        }

        // Al soltar la barra, salta el audio a esa posición
        private void barraProgreso_BusquedaSolicitada(object? sender, double proporcion)
        {
            if (!_audioManager.HayPistaCargada) return;
            var nuevaPosicion = TimeSpan.FromSeconds(_audioManager.DuracionTotal.TotalSeconds * proporcion);
            _audioManager.BuscarPosicion(nuevaPosicion);
        }

        // Cada 40 ms: barra, contador de tiempo y LED que destella en cada beat
        private void timerProgreso_Tick(object? sender, EventArgs e)
        {
            if (!_audioManager.HayPistaCargada)
            {
                timerProgreso.Stop();
                indicadorBeat.Intensidad = 0;
                return;
            }

            var posicion = _audioManager.PosicionActual;
            var total = _audioManager.DuracionTotal;

            if (total.TotalSeconds > 0 && !barraProgreso.Arrastrando)
                barraProgreso.Valor = posicion.TotalSeconds / total.TotalSeconds;

            var restante = total > posicion ? total - posicion : TimeSpan.Zero;
            string tiempo = $"{FormatoTiempo(posicion)} / {FormatoTiempo(total)}   (−{FormatoTiempo(restante)})";
            if (lblTiempo.Text != tiempo) lblTiempo.Text = tiempo;

            if (_audioManager.EstaReproduciendo && _pistaSonando is { TieneBpm: true } p)
            {
                double beats = posicion.TotalSeconds * p.Bpm / 60.0;
                double fase = beats - Math.Floor(beats);       // 0 justo en el golpe
                indicadorBeat.Intensidad = Math.Max(0, 1 - fase * 3.5);
            }
            else
            {
                indicadorBeat.Intensidad = 0;
            }

            if (_audioManager.Termino)
            {
                timerProgreso.Stop();
                barraProgreso.Valor = 1;
                indicadorBeat.Intensidad = 0;
                btnPlayPausa.Text = "↻";
                MostrarEstado($"Terminó «{_pistaSonando?.Titulo}». ↻ la repite; «Avanzar pista» toca la siguiente.");
            }
        }

        // Barra espaciadora = reproducir / pausar (salvo cuando se está escribiendo)
        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Space || ActiveControl is TextBoxBase || ActiveControl is NumericUpDown) return;
            if (!btnPlayPausa.Enabled) return;

            btnPlayPausa_Click(this, EventArgs.Empty);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            timerProgreso.Stop();
            _audioManager.Dispose();
        }

        // ====================================================================
        // Benchmark de estrés (determinista, sin Random)
        // ====================================================================

        private async void btnBenchmark_Click(object? sender, EventArgs e)
        {
            const int n = 20_000;
            const int corridas = 5;

            btnBenchmark.Enabled = false;
            btnBenchmark.Text = "⏳  EJECUTANDO BENCHMARK…";
            MostrarEstado($"Ejecutando benchmark: {n:N0} inserciones intermedias × {corridas} corridas por estructura…");

            try
            {
                // En segundo plano para que la ventana no se congele mientras mide
                var resultados = await Task.Run(() => BenchmarkEstructuras.Ejecutar(n, corridas));
                if (IsDisposed) return;
                MostrarResultadosBenchmark(n, corridas, resultados);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnBenchmark.Enabled = true;
                    btnBenchmark.Text = TextoBotonBenchmark;
                }
            }
        }

        private void MostrarResultadosBenchmark(int n, int corridas, IReadOnlyList<ResultadoBenchmark> r)
        {
            var colores = new[] { ColorCyan, ColorVerde, ColorPurpura };
            graficoBenchmark.MostrarResultados(r.Select((x, i) => (x.Estructura, x.MedianaMs, colores[i % colores.Length])).ToList());

            var lineas = new List<string>
            {
                $"=== {n:N0} INSERCIONES INTERMEDIAS · {corridas} corridas · mediana (mín – máx) ==="
            };
            foreach (var x in r)
                lineas.Add($"• {x.Estructura,-22} {x.MedianaMs,9:0.000} ms  ({x.MinimoMs:0.000} – {x.MaximoMs:0.000})  [{x.Complejidad}]");

            var propia = r[0];
            var lista = r[^1];
            if (propia.MedianaMs > 0)
                lineas.Add($"→ List<T> fue {lista.MedianaMs / propia.MedianaMs:0.0}× más lenta que la Lista Propia " +
                           "(mismos datos para las 3, sin Random).");

            txtResultadosBenchmark.Text = string.Join("\r\n", lineas);
            MostrarEstado($"Benchmark terminado · Propia {propia.MedianaMs:0.000} ms · LinkedList {r[1].MedianaMs:0.000} ms · List {lista.MedianaMs:0.000} ms", TipoEstado.Exito);
        }

        // ====================================================================
        // Barra de estado y textos auxiliares
        // ====================================================================

        private enum TipoEstado { Info, Exito, Aviso, Error }

        private void MostrarEstado(string mensaje, TipoEstado tipo = TipoEstado.Info)
        {
            string icono = tipo switch
            {
                TipoEstado.Exito => "✔",
                TipoEstado.Aviso => "⚠",
                TipoEstado.Error => "✖",
                _ => "•"
            };
            lblEstadoAccion.Text = $"{DateTime.Now:HH:mm:ss}   {icono}  {mensaje}";
            lblEstadoAccion.ForeColor = tipo switch
            {
                TipoEstado.Exito => ColorVerde,
                TipoEstado.Aviso => ColorAmbar,
                TipoEstado.Error => ColorRojo,
                _ => ColorTextoSec
            };
            ttAyuda.SetToolTip(lblEstadoAccion, mensaje);
        }

        private static string TextoFuente(OrigenBpm origen) => origen switch
        {
            OrigenBpm.Calculado => "Audio",
            OrigenBpm.Etiqueta => "ID3",
            OrigenBpm.SinBpm => "Sin BPM",
            _ => "Manual"
        };

        private static string DescripcionFuente(OrigenBpm origen) => origen switch
        {
            OrigenBpm.Calculado => "Calculado analizando el audio",
            OrigenBpm.Etiqueta => "Leído de la etiqueta ID3 del archivo",
            OrigenBpm.SinBpm => "El archivo no tiene BPM",
            _ => "Capturado manualmente"
        };

        private static Color ColorFuente(OrigenBpm origen) => origen switch
        {
            OrigenBpm.Calculado => ColorCyan,
            OrigenBpm.Etiqueta => ColorVerde,
            OrigenBpm.SinBpm => ColorAmbar,
            _ => ColorTextoSec
        };

        // Color por rango de tempo: así la "curva BPM" se ve de un vistazo
        private static Color ColorParaBpm(Pista p)
        {
            if (!p.TieneBpm) return ColorAmbar;
            if (p.Bpm < 100) return ColorCielo;
            if (p.Bpm < 130) return ColorVerde;
            return ColorLila;
        }

        private static string FormatoTiempo(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:D2}";

        // m:ss, o h:mm:ss si pasa de una hora
        private static string FormatoDuracion(int segundos)
        {
            var t = TimeSpan.FromSeconds(segundos);
            return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:D2}:{t.Seconds:D2}" : $"{t.Minutes}:{t.Seconds:D2}";
        }

        private void ConfigurarAyudas()
        {
            ttAyuda.SetToolTip(btnEncolarFinal,
                "Agrega la pista al final de la cola.\nPropia: O(n) — recorre hasta el último nodo\nLinkedList<T>: O(1) — conoce su último nodo\nList<T>: O(1) amortizado");
            ttAyuda.SetToolTip(btnReproducirSiguiente,
                "Inserta la pista justo después de la que va a sonar («Up Next»).\nPropia y LinkedList<T>: O(1) — solo se reconectan punteros\nList<T>: O(n) — desplaza los elementos del arreglo");
            ttAyuda.SetToolTip(btnAvanzar,
                "Saca la cabeza de la cola y la reproduce.\nPropia y LinkedList<T>: O(1)\nList<T>: O(n) — RemoveAt(0) desplaza todo el arreglo");
            ttAyuda.SetToolTip(btnInvertir,
                "Invierte el orden de la cola.\nPropia: O(n) in-place, reordenando 3 punteros (O(1) memoria extra)");
            ttAyuda.SetToolTip(btnOrdenarBpm,
                "Ordena de menor a mayor BPM (orden estable).\nLas pistas sin BPM quedan al final.");
            ttAyuda.SetToolTip(btnPurgar,
                "Elimina pistas con el mismo título y conserva la primera.\nPropia: O(n²) con doble puntero, sin estructuras auxiliares");
            ttAyuda.SetToolTip(btnTap,
                "Haz clic al ritmo de la canción (2 o más veces) para medir el BPM a mano.\nÚtil cuando el archivo no tiene BPM.");
            ttAyuda.SetToolTip(btnExaminar,
                "Selecciona un .mp3 o .wav (también puedes arrastrarlo a la ventana).\nSe leen la duración, el título/artista y el BPM:\nprimero de la etiqueta ID3; si no la tiene, se calcula del audio.");
            ttAyuda.SetToolTip(btnPlayPausa, "Reproducir / pausar (barra espaciadora)");
            ttAyuda.SetToolTip(btnDetener, "Detener y liberar el audio");
            ttAyuda.SetToolTip(barraProgreso, "Haz clic o arrastra para moverte dentro de la pista");
            ttAyuda.SetToolTip(indicadorBeat, "Destella al tempo (BPM) de la pista que suena");
            ttAyuda.SetToolTip(btnBenchmark,
                "Inserta 20,000 pistas idénticas (generadas sin Random) justo después de la cabeza\nen cada estructura. 5 corridas por estructura; se reporta la mediana.");
            ttAyuda.SetToolTip(rbPropia, "ListaSimpleEnlazada<T>: implementación propia con nodos y punteros");
            ttAyuda.SetToolTip(rbLinkedList, "System.Collections.Generic.LinkedList<T> (doblemente enlazada)");
            ttAyuda.SetToolTip(rbList, "System.Collections.Generic.List<T> (arreglo dinámico)");
        }

        // ====================================================================
        // Tema visual — paleta Dark Cyber-Audio
        // ====================================================================

        private static readonly Color ColorFondo = Color.FromArgb(11, 15, 25);
        private static readonly Color ColorPanel = Color.FromArgb(19, 29, 49);
        private static readonly Color ColorTarjeta = Color.FromArgb(24, 35, 60);
        private static readonly Color ColorBorde = Color.FromArgb(36, 53, 86);
        private static readonly Color ColorCyan = Color.FromArgb(0, 240, 255);
        private static readonly Color ColorVerde = Color.FromArgb(16, 185, 129);
        private static readonly Color ColorPurpura = Color.FromArgb(139, 92, 246);
        private static readonly Color ColorLila = Color.FromArgb(167, 139, 250);
        private static readonly Color ColorCielo = Color.FromArgb(56, 189, 248);
        private static readonly Color ColorAmbar = Color.FromArgb(245, 158, 11);
        private static readonly Color ColorRojo = Color.FromArgb(244, 63, 94);
        private static readonly Color ColorTexto = Color.FromArgb(241, 245, 249);
        private static readonly Color ColorTextoSec = Color.FromArgb(148, 163, 184);
        private static readonly Color ColorBotonBase = Color.FromArgb(30, 41, 59);

        private static readonly Font FuenteNormal = new("Segoe UI", 9.5F);
        private static readonly Font FuenteNegrita = new("Segoe UI", 9.5F, FontStyle.Bold);

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
                        rb.Cursor = Cursors.Hand;
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
            EstilizarBoton(btnExaminar, ColorCyan);
            EstilizarBoton(btnTap, ColorAmbar);

            EstilizarGrid();
            EstilizarDisplay();
            EstilizarEncabezado();

            // Consola de telemetría del benchmark
            txtResultadosBenchmark.BackColor = Color.FromArgb(9, 13, 22);
            txtResultadosBenchmark.ForeColor = ColorVerde;
            txtResultadosBenchmark.Font = new Font("Consolas", 9F);
            txtResultadosBenchmark.Text = "Sin resultados todavía.";

            graficoBenchmark.ColorTexto = ColorTexto;
            graficoBenchmark.ColorTextoSec = ColorTextoSec;
            graficoBenchmark.ColorRiel = ColorTarjeta;

            lblEstadisticas.ForeColor = ColorTextoSec;
            lblEstadisticas.Font = new Font("Consolas", 9.5F);

            lblArchivoSeleccionado.ForeColor = ColorTextoSec;

            // Barra de estado inferior
            pnlEstado.BackColor = Color.FromArgb(8, 12, 22);
            lblEstadoAccion.Font = new Font("Consolas", 9F);
            pnlEstado.Paint += (s, e) =>
            {
                using var lapiz = new Pen(ColorBorde);
                e.Graphics.DrawLine(lapiz, 0, 0, pnlEstado.Width, 0);
            };
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
            if (b.Font.Size < 11) b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.TextAlign = ContentAlignment.MiddleCenter;
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
            dgvCola.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            dgvCola.DefaultCellStyle.BackColor = ColorPanel;
            dgvCola.DefaultCellStyle.ForeColor = ColorTexto;
            dgvCola.DefaultCellStyle.Font = FuenteNormal;
            dgvCola.DefaultCellStyle.SelectionBackColor = Color.FromArgb(13, 92, 111); // cyan mezclado con el fondo
            dgvCola.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvCola.DefaultCellStyle.Padding = new Padding(4, 0, 4, 0);

            dgvCola.AlternatingRowsDefaultCellStyle.BackColor = ColorTarjeta;
            dgvCola.AlternatingRowsDefaultCellStyle.ForeColor = ColorTexto;
        }

        // Panel "Now Playing" con aspecto de display LED de CDJ
        private void EstilizarDisplay()
        {
            pnlNowPlaying.BackColor = Color.FromArgb(5, 10, 18);

            lblEtiquetaNowPlaying.ForeColor = ColorCyan;
            lblTituloActual.ForeColor = ColorTextoSec; // gris hasta que suene algo (btnAvanzar lo pone en verde)
            lblBpmActual.ForeColor = ColorTextoSec;
            lblTiempo.ForeColor = ColorTextoSec;

            barraProgreso.ColorPista = ColorTarjeta;
            barraProgreso.ColorInicio = ColorVerde;
            barraProgreso.ColorFin = ColorCyan;
            indicadorBeat.ColorLed = ColorVerde;

            EstilizarBoton(btnPlayPausa, ColorVerde);
            EstilizarBoton(btnDetener, ColorPurpura);

            // Borde neón dibujado a mano (BorderStyle no permite cambiar el color)
            pnlNowPlaying.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var lapiz = new Pen(ColorVerde, 2);
                using var ruta = Dibujo.Redondeado(new RectangleF(1, 1, pnlNowPlaying.Width - 3, pnlNowPlaying.Height - 3), 8);
                e.Graphics.DrawPath(lapiz, ruta);
            };
        }

        private void EstilizarEncabezado()
        {
            pnlHeader.BackColor = Color.FromArgb(8, 12, 22);
            pnlHeader.Paint += pnlHeader_Paint;
        }

        // Encabezado: logo con ecualizador + insignia con la estructura activa
        private void pnlHeader_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = pnlHeader.ClientRectangle;
            if (r.Width <= 0 || r.Height <= 0) return;

            using (var fondo = new LinearGradientBrush(r, Color.FromArgb(8, 12, 22), ColorPanel, LinearGradientMode.Horizontal))
                g.FillRectangle(fondo, r);

            var franja = new Rectangle(0, r.Bottom - 2, r.Width, 2);
            using (var linea = new LinearGradientBrush(franja, ColorCyan, ColorPurpura, LinearGradientMode.Horizontal))
                g.FillRectangle(linea, franja);

            // Ecualizador decorativo
            int[] alturas = { 14, 26, 18, 32, 22, 12 };
            for (int i = 0; i < alturas.Length; i++)
            {
                var barra = new RectangleF(22 + i * 6, 46 - alturas[i], 4, alturas[i]);
                using var brocha = new LinearGradientBrush(new RectangleF(barra.X, 12, 4, 36), ColorCyan, ColorVerde, LinearGradientMode.Vertical);
                g.FillRectangle(brocha, barra);
            }

            using var fuenteLogo = new Font("Segoe UI", 17F, FontStyle.Bold);
            using var fuenteLogoLigera = new Font("Segoe UI Light", 17F);
            using var fuenteSub = new Font("Segoe UI", 8.5F);
            using var fuenteInsignia = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            const TextFormatFlags sinRelleno = TextFormatFlags.NoPadding;
            TextRenderer.DrawText(g, "SOUNDCORE", fuenteLogo, new Point(68, 6), ColorCyan, sinRelleno);
            int anchoLogo = TextRenderer.MeasureText(g, "SOUNDCORE", fuenteLogo, Size.Empty, sinRelleno).Width;
            TextRenderer.DrawText(g, "ENGINE", fuenteLogoLigera, new Point(68 + anchoLogo + 6, 6), ColorTexto, sinRelleno);
            TextRenderer.DrawText(g, "DJ Queue Manager  ·  Estructuras Lineales  ·  TecNM Campus Monclova",
                fuenteSub, new Point(70, 40), ColorTextoSec, sinRelleno);

            // Insignia de la estructura activa (a la derecha)
            Color acento = ColorEstructuraActiva();
            string texto = $"ESTRUCTURA ACTIVA:  {NombreEstructuraActiva().ToUpperInvariant()}";
            var tam = TextRenderer.MeasureText(g, texto, fuenteInsignia, Size.Empty, sinRelleno);
            var insignia = new RectangleF(r.Width - tam.Width - 60, (r.Height - 28) / 2f, tam.Width + 40, 28);

            using (var ruta = Dibujo.Redondeado(insignia, 14))
            using (var relleno = new SolidBrush(Mezclar(ColorPanel, acento, 0.15)))
            using (var borde = new Pen(acento, 1))
            {
                g.FillPath(relleno, ruta);
                g.DrawPath(borde, ruta);
            }
            using (var punto = new SolidBrush(acento))
                g.FillEllipse(punto, insignia.X + 12, insignia.Y + 10, 8, 8);
            TextRenderer.DrawText(g, texto, fuenteInsignia,
                new Point((int)insignia.X + 26, (int)(insignia.Y + (insignia.Height - tam.Height) / 2)), acento, sinRelleno);
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

        // Panel y DataGridView no exponen DoubleBuffered públicamente; activarlo evita parpadeos
        private static void ActivarDobleBufer(Control control) =>
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(control, true);
    }
}
