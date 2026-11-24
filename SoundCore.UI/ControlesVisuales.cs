// ============================================================================
// Archivo:      ControlesVisuales.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// Descripción:  Controles dibujados a mano para el tema oscuro:
//               • BarraProgreso   → barra de reproducción con arrastre.
//               • IndicadorBeat   → LED que parpadea al tempo de la pista.
//               • GraficoBenchmark→ barras horizontales con los tiempos.
// ============================================================================

using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace SoundCore.UI
{
    /// <summary>Base común: doble búfer y repintado al cambiar de tamaño.</summary>
    public abstract class ControlDibujado : Control
    {
        protected ControlDibujado()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
        }

        protected static GraphicsPath Redondeado(RectangleF r, float radio) => Dibujo.Redondeado(r, radio);
    }

    /// <summary>Utilidades de dibujo compartidas por los controles y el formulario.</summary>
    internal static class Dibujo
    {
        /// <summary>Rectángulo con esquinas redondeadas.</summary>
        public static GraphicsPath Redondeado(RectangleF r, float radio)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radio * 2, Math.Min(r.Width, r.Height));
            if (d <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>Barra de progreso de reproducción: clic o arrastre para buscar dentro de la pista.</summary>
    public sealed class BarraProgreso : ControlDibujado
    {
        private double _valor;
        private bool _arrastrando;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorPista { get; set; } = Color.FromArgb(30, 41, 59);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorInicio { get; set; } = Color.FromArgb(16, 185, 129);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorFin { get; set; } = Color.FromArgb(0, 240, 255);

        /// <summary>Se dispara al soltar el mouse, con la proporción (0 a 1) elegida.</summary>
        public event EventHandler<double>? BusquedaSolicitada;

        /// <summary>True mientras el usuario arrastra (el timer no debe mover la barra).</summary>
        public bool Arrastrando => _arrastrando;

        /// <summary>Proporción reproducida, de 0 a 1.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Valor
        {
            get => _valor;
            set
            {
                double v = Math.Clamp(double.IsFinite(value) ? value : 0, 0, 1);
                if (Math.Abs(v - _valor) < 0.0005) return;
                _valor = v;
                Invalidate();
            }
        }

        public BarraProgreso()
        {
            Cursor = Cursors.Hand;
            Height = 20;
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Enabled || e.Button != MouseButtons.Left) return;
            _arrastrando = true;
            Valor = ProporcionEn(e.X);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_arrastrando) Valor = ProporcionEn(e.X);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_arrastrando) return;
            _arrastrando = false;
            Valor = ProporcionEn(e.X);
            BusquedaSolicitada?.Invoke(this, Valor);
        }

        private double ProporcionEn(int x)
        {
            float margen = Height / 2f;
            float ancho = Math.Max(1, Width - 2 * margen);
            return Math.Clamp((x - margen) / ancho, 0, 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float margen = Height / 2f;
            float altoPista = 6;
            var pista = new RectangleF(margen, (Height - altoPista) / 2f, Math.Max(1, Width - 2 * margen), altoPista);

            using (var fondo = new SolidBrush(ColorPista))
            using (var ruta = Redondeado(pista, altoPista / 2))
                g.FillPath(fondo, ruta);

            float xActual = pista.X + (float)(pista.Width * _valor);
            if (_valor > 0)
            {
                var relleno = new RectangleF(pista.X, pista.Y, Math.Max(altoPista, xActual - pista.X), altoPista);
                Color inicio = Enabled ? ColorInicio : Color.FromArgb(71, 85, 105);
                Color fin = Enabled ? ColorFin : Color.FromArgb(100, 116, 139);
                using var degradado = new LinearGradientBrush(pista, inicio, fin, LinearGradientMode.Horizontal);
                using var ruta = Redondeado(relleno, altoPista / 2);
                g.FillPath(degradado, ruta);
            }

            if (Enabled)
            {
                float r = _arrastrando ? 8 : 6;
                using var halo = new SolidBrush(Color.FromArgb(70, ColorFin));
                g.FillEllipse(halo, xActual - r - 3, Height / 2f - r - 3, (r + 3) * 2, (r + 3) * 2);
                using var punto = new SolidBrush(Color.White);
                g.FillEllipse(punto, xActual - r, Height / 2f - r, r * 2, r * 2);
            }
        }
    }

    /// <summary>LED que destella en cada beat (la intensidad la calcula el formulario a partir del BPM).</summary>
    public sealed class IndicadorBeat : ControlDibujado
    {
        private double _intensidad;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorLed { get; set; } = Color.FromArgb(16, 185, 129);

        /// <summary>Brillo del LED, de 0 (apagado) a 1 (golpe).</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Intensidad
        {
            get => _intensidad;
            set
            {
                double v = Math.Clamp(value, 0, 1);
                if (Math.Abs(v - _intensidad) < 0.02) return;
                _intensidad = v;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float d = Math.Min(Width, Height) - 2;
            var circulo = new RectangleF((Width - d) / 2f, (Height - d) / 2f, d, d);

            int alfaHalo = (int)(110 * _intensidad);
            if (alfaHalo > 0)
            {
                using var halo = new SolidBrush(Color.FromArgb(alfaHalo, ColorLed));
                g.FillEllipse(halo, circulo);
            }

            var nucleo = RectangleF.Inflate(circulo, -d * 0.22f, -d * 0.22f);
            int alfa = 60 + (int)(195 * _intensidad);
            using (var relleno = new SolidBrush(Color.FromArgb(alfa, ColorLed)))
                g.FillEllipse(relleno, nucleo);
            using (var borde = new Pen(Color.FromArgb(120, ColorLed), 1))
                g.DrawEllipse(borde, nucleo);
        }
    }

    /// <summary>Gráfico de barras horizontales para comparar los tiempos del benchmark.</summary>
    public sealed class GraficoBenchmark : ControlDibujado
    {
        private IReadOnlyList<(string Nombre, double Ms, Color Color)> _barras = Array.Empty<(string, double, Color)>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorTexto { get; set; } = Color.FromArgb(241, 245, 249);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorTextoSec { get; set; } = Color.FromArgb(148, 163, 184);
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorRiel { get; set; } = Color.FromArgb(24, 35, 60);

        public void MostrarResultados(IReadOnlyList<(string Nombre, double Ms, Color Color)> barras)
        {
            _barras = barras;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var fuente = new Font("Segoe UI", 9F, FontStyle.Bold);
            using var fuenteValor = new Font("Consolas", 9.5F, FontStyle.Bold);

            if (_barras.Count == 0)
            {
                TextRenderer.DrawText(g, "Ejecuta el benchmark para comparar las 3 estructuras con los mismos datos.",
                    fuente, ClientRectangle, ColorTextoSec,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            const int anchoNombre = 170;
            const int anchoValor = 110;
            double maximo = Math.Max(1e-9, _barras.Max(b => b.Ms));
            float altoFila = Height / (float)_barras.Count;
            float altoBarra = Math.Min(16, altoFila - 8);
            float anchoUtil = Math.Max(10, Width - anchoNombre - anchoValor - 8);

            for (int i = 0; i < _barras.Count; i++)
            {
                var (nombre, ms, color) = _barras[i];
                float y = i * altoFila;
                float yBarra = y + (altoFila - altoBarra) / 2f;

                TextRenderer.DrawText(g, nombre, fuente, new Rectangle(0, (int)y, anchoNombre, (int)altoFila), ColorTexto,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                var riel = new RectangleF(anchoNombre, yBarra, anchoUtil, altoBarra);
                using (var brochaRiel = new SolidBrush(ColorRiel))
                using (var rutaRiel = Redondeado(riel, 4))
                    g.FillPath(brochaRiel, rutaRiel);

                // Escala lineal: así se aprecia lo enorme que es la diferencia O(1) vs O(n)
                float largo = Math.Max(4, (float)(anchoUtil * ms / maximo));
                var barra = new RectangleF(anchoNombre, yBarra, largo, altoBarra);
                using (var brocha = new SolidBrush(color))
                using (var ruta = Redondeado(barra, 4))
                    g.FillPath(brocha, ruta);

                TextRenderer.DrawText(g, $"{ms:0.000} ms", fuenteValor,
                    new Rectangle(Width - anchoValor, (int)y, anchoValor, (int)altoFila), color,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
