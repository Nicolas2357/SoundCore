// ============================================================================
// Archivo:      MainForm.Designer.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// ============================================================================

#nullable disable
namespace SoundCore.UI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlHeader = new Panel();
            gbRegistro = new GroupBox();
            lblTitulo = new Label();
            txtTitulo = new TextBox();
            lblArtista = new Label();
            txtArtista = new TextBox();
            lblBpm = new Label();
            numBpm = new NumericUpDown();
            btnTap = new Button();
            lblDuracion = new Label();
            numDuracion = new NumericUpDown();
            lblEstadoBpm = new Label();
            lblArchivo = new Label();
            btnExaminar = new Button();
            lblArchivoSeleccionado = new Label();
            gbEstructura = new GroupBox();
            rbPropia = new RadioButton();
            rbLinkedList = new RadioButton();
            rbList = new RadioButton();
            gbAcciones = new GroupBox();
            btnEncolarFinal = new Button();
            btnReproducirSiguiente = new Button();
            btnAvanzar = new Button();
            btnInvertir = new Button();
            btnOrdenarBpm = new Button();
            btnPurgar = new Button();
            pnlNowPlaying = new Panel();
            lblEtiquetaNowPlaying = new Label();
            indicadorBeat = new IndicadorBeat();
            lblTituloActual = new Label();
            lblBpmActual = new Label();
            barraProgreso = new BarraProgreso();
            lblTiempo = new Label();
            btnPlayPausa = new Button();
            btnDetener = new Button();
            dgvCola = new DataGridView();
            lblEstadisticas = new Label();
            gbBenchmark = new GroupBox();
            btnBenchmark = new Button();
            graficoBenchmark = new GraficoBenchmark();
            txtResultadosBenchmark = new TextBox();
            pnlEstado = new Panel();
            lblEstadoAccion = new Label();
            timerProgreso = new System.Windows.Forms.Timer(components);
            ttAyuda = new ToolTip(components);

            ((System.ComponentModel.ISupportInitialize)numBpm).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDuracion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCola).BeginInit();
            SuspendLayout();

            Font = new Font("Segoe UI", 9.5F);

            // ====== pnlHeader (logo + estructura activa; se dibuja en MainForm) ======
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Size = new Size(1120, 64);
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ====== gbRegistro ======
            gbRegistro.Text = "  REGISTRO DE PISTA";
            gbRegistro.Location = new Point(20, 76);
            gbRegistro.Size = new Size(320, 300);
            gbRegistro.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbRegistro.Controls.Add(lblTitulo);
            gbRegistro.Controls.Add(txtTitulo);
            gbRegistro.Controls.Add(lblArtista);
            gbRegistro.Controls.Add(txtArtista);
            gbRegistro.Controls.Add(lblBpm);
            gbRegistro.Controls.Add(numBpm);
            gbRegistro.Controls.Add(btnTap);
            gbRegistro.Controls.Add(lblDuracion);
            gbRegistro.Controls.Add(numDuracion);
            gbRegistro.Controls.Add(lblEstadoBpm);
            gbRegistro.Controls.Add(lblArchivo);
            gbRegistro.Controls.Add(btnExaminar);
            gbRegistro.Controls.Add(lblArchivoSeleccionado);

            lblTitulo.Text = "TÍTULO";
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblTitulo.Location = new Point(20, 32);

            txtTitulo.Location = new Point(20, 52);
            txtTitulo.Size = new Size(280, 26);
            txtTitulo.Font = new Font("Segoe UI", 10F);
            txtTitulo.PlaceholderText = "Ej. Fireball";

            lblArtista.Text = "ARTISTA";
            lblArtista.AutoSize = true;
            lblArtista.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblArtista.Location = new Point(20, 86);

            txtArtista.Location = new Point(20, 106);
            txtArtista.Size = new Size(280, 26);
            txtArtista.Font = new Font("Segoe UI", 10F);
            txtArtista.PlaceholderText = "Ej. Pitbull";

            lblBpm.Text = "BPM";
            lblBpm.AutoSize = true;
            lblBpm.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblBpm.Location = new Point(20, 140);

            numBpm.Location = new Point(20, 160);
            numBpm.Size = new Size(96, 26);
            numBpm.Font = new Font("Segoe UI", 10F);
            numBpm.Minimum = 40;
            numBpm.Maximum = 300;
            numBpm.Value = 120;
            numBpm.TextAlign = HorizontalAlignment.Center;
            numBpm.ValueChanged += numBpm_ValueChanged;

            btnTap.Text = "TAP";
            btnTap.Location = new Point(122, 159);
            btnTap.Size = new Size(58, 28);
            btnTap.Click += btnTap_Click;

            lblDuracion.Text = "DURACIÓN (SEG)";
            lblDuracion.AutoSize = true;
            lblDuracion.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblDuracion.Location = new Point(196, 140);

            numDuracion.Location = new Point(196, 160);
            numDuracion.Size = new Size(104, 26);
            numDuracion.Font = new Font("Segoe UI", 10F);
            numDuracion.Minimum = 1;
            numDuracion.Maximum = 3600;
            numDuracion.Value = 180;
            numDuracion.TextAlign = HorizontalAlignment.Center;

            lblEstadoBpm.Text = "BPM manual · carga un archivo para calcularlo";
            lblEstadoBpm.Location = new Point(20, 192);
            lblEstadoBpm.Size = new Size(280, 34);
            lblEstadoBpm.Font = new Font("Segoe UI", 8F);

            lblArchivo.Text = "ARCHIVO DE AUDIO";
            lblArchivo.AutoSize = true;
            lblArchivo.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblArchivo.Location = new Point(20, 230);

            btnExaminar.Text = "Examinar...";
            btnExaminar.Location = new Point(20, 250);
            btnExaminar.Size = new Size(110, 30);
            btnExaminar.Click += btnExaminar_Click;

            lblArchivoSeleccionado.Text = "Sin archivo · o arrástralo aquí";
            lblArchivoSeleccionado.Location = new Point(138, 250);
            lblArchivoSeleccionado.Size = new Size(166, 30);
            lblArchivoSeleccionado.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblArchivoSeleccionado.TextAlign = ContentAlignment.MiddleLeft;
            lblArchivoSeleccionado.AutoEllipsis = true;

            // ====== gbEstructura ======
            gbEstructura.Text = "  ESTRUCTURA ACTIVA";
            gbEstructura.Location = new Point(20, 388);
            gbEstructura.Size = new Size(320, 118);
            gbEstructura.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbEstructura.Controls.Add(rbPropia);
            gbEstructura.Controls.Add(rbLinkedList);
            gbEstructura.Controls.Add(rbList);

            rbPropia.Text = "Lista Propia (Nodos)";
            rbPropia.AutoSize = true;
            rbPropia.Checked = true;
            rbPropia.Font = new Font("Segoe UI", 9.5F);
            rbPropia.Location = new Point(20, 32);
            rbPropia.CheckedChanged += rbEstructura_CheckedChanged;

            rbLinkedList.Text = "LinkedList<T> (.NET)";
            rbLinkedList.AutoSize = true;
            rbLinkedList.Font = new Font("Segoe UI", 9.5F);
            rbLinkedList.Location = new Point(20, 60);
            rbLinkedList.CheckedChanged += rbEstructura_CheckedChanged;

            rbList.Text = "List<T> (.NET)";
            rbList.AutoSize = true;
            rbList.Font = new Font("Segoe UI", 9.5F);
            rbList.Location = new Point(20, 88);
            rbList.CheckedChanged += rbEstructura_CheckedChanged;

            // ====== gbAcciones ======
            gbAcciones.Text = "  ACCIONES DE COLA";
            gbAcciones.Location = new Point(20, 518);
            gbAcciones.Size = new Size(320, 282);
            gbAcciones.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbAcciones.Controls.Add(btnEncolarFinal);
            gbAcciones.Controls.Add(btnReproducirSiguiente);
            gbAcciones.Controls.Add(btnAvanzar);
            gbAcciones.Controls.Add(btnInvertir);
            gbAcciones.Controls.Add(btnOrdenarBpm);
            gbAcciones.Controls.Add(btnPurgar);

            btnEncolarFinal.Text = "＋  ENCOLAR AL FINAL";
            btnEncolarFinal.Location = new Point(20, 32);
            btnEncolarFinal.Size = new Size(280, 34);
            btnEncolarFinal.Click += btnEncolarFinal_Click;

            btnReproducirSiguiente.Text = "⤴  REPRODUCIR SIGUIENTE";
            btnReproducirSiguiente.Location = new Point(20, 72);
            btnReproducirSiguiente.Size = new Size(280, 34);
            btnReproducirSiguiente.Click += btnReproducirSiguiente_Click;

            btnAvanzar.Text = "▶  AVANZAR PISTA";
            btnAvanzar.Location = new Point(20, 112);
            btnAvanzar.Size = new Size(280, 34);
            btnAvanzar.Click += btnAvanzar_Click;

            btnInvertir.Text = "⇅  INVERTIR LISTA";
            btnInvertir.Location = new Point(20, 152);
            btnInvertir.Size = new Size(280, 34);
            btnInvertir.Click += btnInvertir_Click;

            btnOrdenarBpm.Text = "♫  ORDENAR POR CURVA BPM";
            btnOrdenarBpm.Location = new Point(20, 192);
            btnOrdenarBpm.Size = new Size(280, 34);
            btnOrdenarBpm.Click += btnOrdenarBpm_Click;

            btnPurgar.Text = "✕  PURGAR DUPLICADOS";
            btnPurgar.Location = new Point(20, 232);
            btnPurgar.Size = new Size(280, 34);
            btnPurgar.Click += btnPurgar_Click;

            // ====== pnlNowPlaying (display tipo CDJ) ======
            pnlNowPlaying.Location = new Point(360, 76);
            pnlNowPlaying.Size = new Size(740, 140);
            pnlNowPlaying.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlNowPlaying.Controls.Add(lblEtiquetaNowPlaying);
            pnlNowPlaying.Controls.Add(indicadorBeat);
            pnlNowPlaying.Controls.Add(lblTituloActual);
            pnlNowPlaying.Controls.Add(lblBpmActual);
            pnlNowPlaying.Controls.Add(barraProgreso);
            pnlNowPlaying.Controls.Add(lblTiempo);
            pnlNowPlaying.Controls.Add(btnPlayPausa);
            pnlNowPlaying.Controls.Add(btnDetener);

            lblEtiquetaNowPlaying.Text = "NOW PLAYING";
            lblEtiquetaNowPlaying.AutoSize = true;
            lblEtiquetaNowPlaying.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblEtiquetaNowPlaying.Location = new Point(20, 12);

            indicadorBeat.Location = new Point(18, 40);
            indicadorBeat.Size = new Size(24, 24);

            lblTituloActual.Text = "SIN REPRODUCCIÓN";
            lblTituloActual.Font = new Font("Consolas", 15F, FontStyle.Bold);
            lblTituloActual.Location = new Point(48, 36);
            lblTituloActual.Size = new Size(540, 32);
            lblTituloActual.TextAlign = ContentAlignment.MiddleLeft;
            lblTituloActual.AutoEllipsis = true;
            lblTituloActual.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblBpmActual.Text = "— BPM";
            lblBpmActual.Font = new Font("Consolas", 17F, FontStyle.Bold);
            lblBpmActual.Location = new Point(596, 32);
            lblBpmActual.Size = new Size(124, 40);
            lblBpmActual.TextAlign = ContentAlignment.MiddleRight;
            lblBpmActual.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            barraProgreso.Location = new Point(14, 78);
            barraProgreso.Size = new Size(712, 20);
            barraProgreso.Enabled = false;
            barraProgreso.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            barraProgreso.BusquedaSolicitada += barraProgreso_BusquedaSolicitada;

            lblTiempo.Text = "0:00 / 0:00";
            lblTiempo.Font = new Font("Consolas", 10F);
            lblTiempo.Location = new Point(20, 104);
            lblTiempo.Size = new Size(260, 26);
            lblTiempo.TextAlign = ContentAlignment.MiddleLeft;

            btnPlayPausa.Text = "▶";
            btnPlayPausa.Location = new Point(600, 102);
            btnPlayPausa.Size = new Size(56, 30);
            btnPlayPausa.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPlayPausa.Enabled = false;
            btnPlayPausa.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPlayPausa.Click += btnPlayPausa_Click;

            btnDetener.Text = "⏹";
            btnDetener.Location = new Point(664, 102);
            btnDetener.Size = new Size(56, 30);
            btnDetener.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnDetener.Enabled = false;
            btnDetener.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDetener.Click += btnDetener_Click;

            // ====== dgvCola ======
            dgvCola.Location = new Point(360, 226);
            dgvCola.Size = new Size(740, 290);
            dgvCola.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgvCola.AllowUserToAddRows = false;
            dgvCola.AllowUserToDeleteRows = false;
            dgvCola.AllowUserToResizeRows = false;
            dgvCola.ReadOnly = true;
            dgvCola.RowHeadersVisible = false;
            dgvCola.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCola.MultiSelect = false;
            dgvCola.ShowCellToolTips = true;

            lblEstadisticas.Text = "TOTAL EN COLA: 0";
            lblEstadisticas.Location = new Point(360, 522);
            lblEstadisticas.Size = new Size(740, 22);
            lblEstadisticas.TextAlign = ContentAlignment.MiddleLeft;
            lblEstadisticas.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ====== gbBenchmark ======
            gbBenchmark.Text = "  BENCHMARK DE ESTRÉS";
            gbBenchmark.Location = new Point(360, 552);
            gbBenchmark.Size = new Size(740, 248);
            gbBenchmark.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbBenchmark.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbBenchmark.Controls.Add(btnBenchmark);
            gbBenchmark.Controls.Add(graficoBenchmark);
            gbBenchmark.Controls.Add(txtResultadosBenchmark);

            btnBenchmark.Text = "▶  EJECUTAR BENCHMARK (20,000 ITEMS)";
            btnBenchmark.Location = new Point(20, 30);
            btnBenchmark.Size = new Size(700, 34);
            btnBenchmark.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnBenchmark.Click += btnBenchmark_Click;

            graficoBenchmark.Location = new Point(20, 72);
            graficoBenchmark.Size = new Size(700, 84);
            graficoBenchmark.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            txtResultadosBenchmark.Location = new Point(20, 162);
            txtResultadosBenchmark.Size = new Size(700, 74);
            txtResultadosBenchmark.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtResultadosBenchmark.Multiline = true;
            txtResultadosBenchmark.ReadOnly = true;
            txtResultadosBenchmark.ScrollBars = ScrollBars.Vertical;
            txtResultadosBenchmark.Font = new Font("Consolas", 9F);

            // ====== pnlEstado (barra de estado inferior) ======
            pnlEstado.Location = new Point(0, 812);
            pnlEstado.Size = new Size(1120, 28);
            pnlEstado.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlEstado.Controls.Add(lblEstadoAccion);

            lblEstadoAccion.Dock = DockStyle.Fill;
            lblEstadoAccion.Padding = new Padding(16, 0, 16, 0);
            lblEstadoAccion.TextAlign = ContentAlignment.MiddleLeft;
            lblEstadoAccion.AutoEllipsis = true;
            lblEstadoAccion.Text = "Listo.";

            // ====== timerProgreso (barra de progreso + LED de beat) ======
            timerProgreso.Interval = 40;
            timerProgreso.Tick += timerProgreso_Tick;

            // ====== ttAyuda ======
            ttAyuda.AutoPopDelay = 12000;
            ttAyuda.InitialDelay = 400;
            ttAyuda.ReshowDelay = 150;

            // ====== MainForm ======
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 840);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            AutoScroll = true;
            KeyPreview = true;
            AllowDrop = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SoundCore Engine — DJ Queue Manager";
            Padding = new Padding(0);
            FormClosing += MainForm_FormClosing;
            Shown += MainForm_Shown;
            KeyDown += MainForm_KeyDown;
            Resize += MainForm_Resize;
            Controls.Add(pnlHeader);
            Controls.Add(gbRegistro);
            Controls.Add(gbEstructura);
            Controls.Add(gbAcciones);
            Controls.Add(pnlNowPlaying);
            Controls.Add(dgvCola);
            Controls.Add(lblEstadisticas);
            Controls.Add(gbBenchmark);
            Controls.Add(pnlEstado);

            ((System.ComponentModel.ISupportInitialize)numBpm).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDuracion).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCola).EndInit();
            ResumeLayout(false);
        }

        private Panel pnlHeader;
        private GroupBox gbRegistro;
        private Label lblTitulo;
        private TextBox txtTitulo;
        private Label lblArtista;
        private TextBox txtArtista;
        private Label lblBpm;
        private NumericUpDown numBpm;
        private Button btnTap;
        private Label lblDuracion;
        private NumericUpDown numDuracion;
        private Label lblEstadoBpm;
        private Label lblArchivo;
        private Button btnExaminar;
        private Label lblArchivoSeleccionado;
        private GroupBox gbEstructura;
        private RadioButton rbPropia;
        private RadioButton rbLinkedList;
        private RadioButton rbList;
        private GroupBox gbAcciones;
        private Button btnEncolarFinal;
        private Button btnReproducirSiguiente;
        private Button btnAvanzar;
        private Button btnInvertir;
        private Button btnOrdenarBpm;
        private Button btnPurgar;
        private Panel pnlNowPlaying;
        private Label lblEtiquetaNowPlaying;
        private IndicadorBeat indicadorBeat;
        private Label lblTituloActual;
        private Label lblBpmActual;
        private BarraProgreso barraProgreso;
        private Label lblTiempo;
        private Button btnPlayPausa;
        private Button btnDetener;
        private DataGridView dgvCola;
        private Label lblEstadisticas;
        private GroupBox gbBenchmark;
        private Button btnBenchmark;
        private GraficoBenchmark graficoBenchmark;
        private TextBox txtResultadosBenchmark;
        private Panel pnlEstado;
        private Label lblEstadoAccion;
        private System.Windows.Forms.Timer timerProgreso;
        private ToolTip ttAyuda;
    }
}
