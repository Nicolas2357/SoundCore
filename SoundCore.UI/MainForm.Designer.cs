// ============================================================================
// Archivo:      MainForm.Designer.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.1
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
            gbRegistro = new GroupBox();
            lblTitulo = new Label();
            txtTitulo = new TextBox();
            lblArtista = new Label();
            txtArtista = new TextBox();
            lblBpm = new Label();
            numBpm = new NumericUpDown();
            lblDuracion = new Label();
            numDuracion = new NumericUpDown();
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
            lblTituloActual = new Label();
            tbProgreso = new TrackBar();
            lblTiempo = new Label();
            btnPlayPausa = new Button();
            btnDetener = new Button();
            dgvCola = new DataGridView();
            lblEstadisticas = new Label();
            gbBenchmark = new GroupBox();
            btnBenchmark = new Button();
            txtResultadosBenchmark = new TextBox();
            timerProgreso = new System.Windows.Forms.Timer();

            ((System.ComponentModel.ISupportInitialize)numBpm).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDuracion).BeginInit();
            ((System.ComponentModel.ISupportInitialize)tbProgreso).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvCola).BeginInit();
            SuspendLayout();

            Font = new Font("Segoe UI", 9.5F);

            // ====== gbRegistro ======
            gbRegistro.Text = "  REGISTRO DE PISTA";
            gbRegistro.Location = new Point(20, 16);
            gbRegistro.Size = new Size(320, 270);
            gbRegistro.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbRegistro.Controls.Add(lblTitulo);
            gbRegistro.Controls.Add(txtTitulo);
            gbRegistro.Controls.Add(lblArtista);
            gbRegistro.Controls.Add(txtArtista);
            gbRegistro.Controls.Add(lblBpm);
            gbRegistro.Controls.Add(numBpm);
            gbRegistro.Controls.Add(lblDuracion);
            gbRegistro.Controls.Add(numDuracion);
            gbRegistro.Controls.Add(lblArchivo);
            gbRegistro.Controls.Add(btnExaminar);
            gbRegistro.Controls.Add(lblArchivoSeleccionado);

            lblTitulo.Text = "TÍTULO";
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblTitulo.Location = new Point(20, 36);

            txtTitulo.Location = new Point(20, 56);
            txtTitulo.Size = new Size(280, 26);
            txtTitulo.Font = new Font("Segoe UI", 10F);

            lblArtista.Text = "ARTISTA";
            lblArtista.AutoSize = true;
            lblArtista.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblArtista.Location = new Point(20, 88);

            txtArtista.Location = new Point(20, 108);
            txtArtista.Size = new Size(280, 26);
            txtArtista.Font = new Font("Segoe UI", 10F);

            lblBpm.Text = "BPM";
            lblBpm.AutoSize = true;
            lblBpm.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblBpm.Location = new Point(20, 140);

            numBpm.Location = new Point(20, 160);
            numBpm.Size = new Size(120, 26);
            numBpm.Font = new Font("Segoe UI", 10F);
            numBpm.Minimum = 60;
            numBpm.Maximum = 200;
            numBpm.Value = 120;
            numBpm.TextAlign = HorizontalAlignment.Center;

            lblDuracion.Text = "DURACIÓN (SEG)";
            lblDuracion.AutoSize = true;
            lblDuracion.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblDuracion.Location = new Point(180, 140);

            numDuracion.Location = new Point(180, 160);
            numDuracion.Size = new Size(120, 26);
            numDuracion.Font = new Font("Segoe UI", 10F);
            numDuracion.Minimum = 30;
            numDuracion.Maximum = 900;
            numDuracion.Value = 180;
            numDuracion.TextAlign = HorizontalAlignment.Center;

            lblArchivo.Text = "ARCHIVO DE AUDIO";
            lblArchivo.AutoSize = true;
            lblArchivo.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblArchivo.Location = new Point(20, 194);

            btnExaminar.Text = "Examinar...";
            btnExaminar.Location = new Point(20, 214);
            btnExaminar.Size = new Size(110, 28);
            btnExaminar.Click += btnExaminar_Click;

            lblArchivoSeleccionado.Text = "Sin archivo seleccionado";
            lblArchivoSeleccionado.Location = new Point(140, 219);
            lblArchivoSeleccionado.Size = new Size(160, 36);
            lblArchivoSeleccionado.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblArchivoSeleccionado.AutoEllipsis = true;

            // ====== gbEstructura ======
            gbEstructura.Text = "  ESTRUCTURA ACTIVA";
            gbEstructura.Location = new Point(20, 300);
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
            gbAcciones.Location = new Point(20, 434);
            gbAcciones.Size = new Size(320, 270);
            gbAcciones.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbAcciones.Controls.Add(btnEncolarFinal);
            gbAcciones.Controls.Add(btnReproducirSiguiente);
            gbAcciones.Controls.Add(btnAvanzar);
            gbAcciones.Controls.Add(btnInvertir);
            gbAcciones.Controls.Add(btnOrdenarBpm);
            gbAcciones.Controls.Add(btnPurgar);

            btnEncolarFinal.Text = "＋ ENCOLAR AL FINAL";
            btnEncolarFinal.Location = new Point(20, 30);
            btnEncolarFinal.Size = new Size(280, 32);
            btnEncolarFinal.Click += btnEncolarFinal_Click;

            btnReproducirSiguiente.Text = "REPRODUCIR SIGUIENTE";
            btnReproducirSiguiente.Location = new Point(20, 68);
            btnReproducirSiguiente.Size = new Size(280, 32);
            btnReproducirSiguiente.Click += btnReproducirSiguiente_Click;

            btnAvanzar.Text = "▶  AVANZAR PISTA";
            btnAvanzar.Location = new Point(20, 106);
            btnAvanzar.Size = new Size(280, 32);
            btnAvanzar.Click += btnAvanzar_Click;

            btnInvertir.Text = "⇅  INVERTIR LISTA";
            btnInvertir.Location = new Point(20, 144);
            btnInvertir.Size = new Size(280, 32);
            btnInvertir.Click += btnInvertir_Click;

            btnOrdenarBpm.Text = "ORDENAR POR CURVA BPM";
            btnOrdenarBpm.Location = new Point(20, 182);
            btnOrdenarBpm.Size = new Size(280, 32);
            btnOrdenarBpm.Click += btnOrdenarBpm_Click;

            btnPurgar.Text = "PURGAR DUPLICADOS";
            btnPurgar.Location = new Point(20, 220);
            btnPurgar.Size = new Size(280, 32);
            btnPurgar.Click += btnPurgar_Click;

            // ====== pnlNowPlaying (panel grande tipo display de CDJ) ======
            pnlNowPlaying.Location = new Point(360, 16);
            pnlNowPlaying.Size = new Size(700, 130);
            pnlNowPlaying.Controls.Add(lblTituloActual);
            pnlNowPlaying.Controls.Add(tbProgreso);
            pnlNowPlaying.Controls.Add(lblTiempo);
            pnlNowPlaying.Controls.Add(btnPlayPausa);
            pnlNowPlaying.Controls.Add(btnDetener);

            lblTituloActual.Text = "SIN REPRODUCCIÓN";
            lblTituloActual.Font = new Font("Consolas", 15F, FontStyle.Bold);
            lblTituloActual.Location = new Point(20, 14);
            lblTituloActual.Size = new Size(660, 30);
            lblTituloActual.TextAlign = ContentAlignment.MiddleLeft;
            lblTituloActual.AutoEllipsis = true;

            tbProgreso.Location = new Point(20, 54);
            tbProgreso.Size = new Size(660, 30);
            tbProgreso.Minimum = 0;
            tbProgreso.Maximum = 100;
            tbProgreso.TickStyle = TickStyle.None;
            tbProgreso.Enabled = false;
            tbProgreso.MouseUp += tbProgreso_MouseUp;

            lblTiempo.Text = "0:00 / 0:00";
            lblTiempo.Font = new Font("Consolas", 9.5F);
            lblTiempo.Location = new Point(20, 92);
            lblTiempo.Size = new Size(120, 22);

            btnPlayPausa.Text = "▶";
            btnPlayPausa.Location = new Point(560, 86);
            btnPlayPausa.Size = new Size(56, 32);
            btnPlayPausa.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnPlayPausa.Enabled = false;
            btnPlayPausa.Click += btnPlayPausa_Click;

            btnDetener.Text = "⏹";
            btnDetener.Location = new Point(624, 86);
            btnDetener.Size = new Size(56, 32);
            btnDetener.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnDetener.Enabled = false;
            btnDetener.Click += btnDetener_Click;

            // ====== dgvCola ======
            dgvCola.Location = new Point(360, 156);
            dgvCola.Size = new Size(700, 342);
            dgvCola.AllowUserToAddRows = false;
            dgvCola.AllowUserToDeleteRows = false;
            dgvCola.ReadOnly = true;
            dgvCola.RowHeadersVisible = false;
            dgvCola.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCola.MultiSelect = false;

            lblEstadisticas.Text = "TOTAL EN COLA: 0";
            lblEstadisticas.Location = new Point(360, 504);
            lblEstadisticas.Size = new Size(700, 22);
            lblEstadisticas.TextAlign = ContentAlignment.MiddleLeft;

            // ====== gbBenchmark ======
            gbBenchmark.Text = "  BENCHMARK DE ESTRÉS";
            gbBenchmark.Location = new Point(360, 532);
            gbBenchmark.Size = new Size(700, 168);
            gbBenchmark.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gbBenchmark.Controls.Add(btnBenchmark);
            gbBenchmark.Controls.Add(txtResultadosBenchmark);

            btnBenchmark.Text = "▶  EJECUTAR BENCHMARK (20,000 ITEMS)";
            btnBenchmark.Location = new Point(20, 30);
            btnBenchmark.Size = new Size(660, 32);
            btnBenchmark.Click += btnBenchmark_Click;

            txtResultadosBenchmark.Location = new Point(20, 70);
            txtResultadosBenchmark.Size = new Size(660, 84);
            txtResultadosBenchmark.Multiline = true;
            txtResultadosBenchmark.ReadOnly = true;
            txtResultadosBenchmark.ScrollBars = ScrollBars.Vertical;
            txtResultadosBenchmark.Font = new Font("Consolas", 9.5F);

            // ====== timerProgreso ======
            timerProgreso.Interval = 500;
            timerProgreso.Tick += timerProgreso_Tick;

            // ====== MainForm ======
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1100, 720);
            MinimumSize = new Size(900, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            AutoScroll = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SoundCore Engine — DJ Queue Manager";
            Padding = new Padding(0);
            FormClosing += MainForm_FormClosing;
            Controls.Add(gbRegistro);
            Controls.Add(gbEstructura);
            Controls.Add(gbAcciones);
            Controls.Add(pnlNowPlaying);
            Controls.Add(dgvCola);
            Controls.Add(lblEstadisticas);
            Controls.Add(gbBenchmark);

            ((System.ComponentModel.ISupportInitialize)numBpm).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDuracion).EndInit();
            ((System.ComponentModel.ISupportInitialize)tbProgreso).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvCola).EndInit();
            ResumeLayout(false);
        }

        private GroupBox gbRegistro;
        private Label lblTitulo;
        private TextBox txtTitulo;
        private Label lblArtista;
        private TextBox txtArtista;
        private Label lblBpm;
        private NumericUpDown numBpm;
        private Label lblDuracion;
        private NumericUpDown numDuracion;
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
        private Label lblTituloActual;
        private TrackBar tbProgreso;
        private Label lblTiempo;
        private Button btnPlayPausa;
        private Button btnDetener;
        private DataGridView dgvCola;
        private Label lblEstadisticas;
        private GroupBox gbBenchmark;
        private Button btnBenchmark;
        private TextBox txtResultadosBenchmark;
        private System.Windows.Forms.Timer timerProgreso;
    }
}