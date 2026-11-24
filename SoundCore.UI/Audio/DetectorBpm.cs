// ============================================================================
// Archivo:      DetectorBpm.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// Descripción:  Estimación del tempo (BPM) a partir de las muestras de audio.
//               No depende de NAudio ni de Windows Forms: recibe un arreglo de
//               muestras mono y su frecuencia de muestreo.
//
//               Algoritmo (clásico de recuperación de información musical):
//                 1. Diezmado a ~22 kHz (más rápido, conserva bombo/caja).
//                 2. Flujo espectral (FFT con ventana Hann): mide cuánta
//                    energía "nueva" aparece en cada cuadro → curva de ataques.
//                 3. Se resta la media local y se rectifica (solo subidas).
//                 4. Autocorrelación de la curva de ataques: los golpes se
//                    repiten cada periodo de beat, así que hay picos en ese
//                    retardo y en sus múltiplos.
//                 5. Para cada BPM candidato (60.0 a 200.0, paso 0.05) se suma
//                    la autocorrelación en 1, 2, 3 y 4 periodos (filtro peine),
//                    ponderada por una curva que favorece tempos cercanos a
//                    120 BPM para resolver errores de "doble / mitad" de tempo.
// ============================================================================

namespace SoundCore.UI.Audio
{
    /// <summary>Resultado de la detección: BPM estimado y una confianza entre 0 y 1.</summary>
    public readonly record struct ResultadoDeteccion(double Bpm, double Confianza);

    public static class DetectorBpm
    {
        public const double BpmMinimo = 60.0;
        public const double BpmMaximo = 200.0;

        /// <summary>Por debajo de esta confianza se considera que el audio no tiene un pulso claro.</summary>
        public const double ConfianzaMinima = 0.06;

        private const int TamVentana = 1024;      // ≈ 46 ms a 22 kHz
        private const int Salto = 128;            // ≈ 172 cuadros por segundo
        private const double SegundosMinimos = 10; // con menos audio no hay suficientes beats
        private const double FuerzaMinimaAtaques = 1.0; // música real: 5–30; tono continuo: ≈0.2
        private const double CentroPreferido = 120.0;
        private const double AnchoPreferenciaOctavas = 0.9;

        /// <summary>
        /// Estima el BPM de una señal mono. Devuelve null si el audio es muy corto,
        /// está en silencio o no tiene un pulso rítmico reconocible.
        /// </summary>
        public static ResultadoDeteccion? Detectar(float[] mono, int frecuenciaMuestreo)
        {
            if (mono == null || frecuenciaMuestreo <= 0) return null;

            // ---- 1. Diezmado (promedio de bloques = filtro pasa-bajas sencillo) ----
            int factor = Math.Max(1, (int)Math.Round(frecuenciaMuestreo / 22050.0));
            double fs = frecuenciaMuestreo / (double)factor;
            float[] x = Diezmar(mono, factor);

            if (x.Length < fs * SegundosMinimos) return null;
            if (Rms(x) < 1e-4) return null; // silencio

            // ---- 2. Flujo espectral ----
            double[] ataques = FlujoEspectral(x);
            double cps = fs / Salto; // cuadros por segundo de la curva de ataques

            // ---- 3. Quitar la tendencia local y quedarse solo con las subidas ----
            RestarMediaLocal(ataques, (int)Math.Round(cps * 0.5));

            // Sin ataques marcados (tono sostenido, drone, voz hablada muy plana) no hay pulso que medir
            double media = ataques.Average();
            if (media < FuerzaMinimaAtaques) return null;
            for (int i = 0; i < ataques.Length; i++) ataques[i] -= media;

            // ---- 4. Autocorrelación normalizada (ρ(0) = 1) ----
            int retardoMaximo = (int)Math.Ceiling(4 * 60.0 * cps / BpmMinimo) + 2;
            if (retardoMaximo >= ataques.Length / 2) return null;
            double[]? rho = Autocorrelacion(ataques, retardoMaximo);
            if (rho == null) return null;

            // ---- 5. Filtro peine sobre los BPM candidatos ----
            double mejorBpm = 0, mejorPuntaje = double.NegativeInfinity, crudoDelMejor = 0;
            for (double bpm = BpmMinimo; bpm <= BpmMaximo + 1e-9; bpm += 0.05)
            {
                double periodo = 60.0 * cps / bpm;
                double crudo = (Interpolar(rho, periodo)
                              + Interpolar(rho, 2 * periodo)
                              + Interpolar(rho, 3 * periodo)
                              + Interpolar(rho, 4 * periodo)) / 4.0;

                double octavas = Math.Log2(bpm / CentroPreferido) / AnchoPreferenciaOctavas;
                double puntaje = crudo * Math.Exp(-0.5 * octavas * octavas);

                if (puntaje > mejorPuntaje)
                {
                    mejorPuntaje = puntaje;
                    mejorBpm = bpm;
                    crudoDelMejor = crudo;
                }
            }

            double confianza = Math.Clamp(crudoDelMejor, 0, 1);
            if (confianza < ConfianzaMinima) return null;

            return new ResultadoDeteccion(Math.Round(mejorBpm, 1), confianza);
        }

        // ------------------------------------------------------------------
        // Pasos auxiliares
        // ------------------------------------------------------------------

        private static float[] Diezmar(float[] entrada, int factor)
        {
            if (factor == 1) return entrada;
            var salida = new float[entrada.Length / factor];
            for (int i = 0; i < salida.Length; i++)
            {
                float suma = 0;
                int baseIdx = i * factor;
                for (int j = 0; j < factor; j++) suma += entrada[baseIdx + j];
                salida[i] = suma / factor;
            }
            return salida;
        }

        private static double Rms(float[] x)
        {
            double suma = 0;
            for (int i = 0; i < x.Length; i++) suma += x[i] * (double)x[i];
            return Math.Sqrt(suma / x.Length);
        }

        private static double[] FlujoEspectral(float[] x)
        {
            int cuadros = (x.Length - TamVentana) / Salto + 1;
            int mitad = TamVentana / 2;

            var hann = new double[TamVentana];
            for (int i = 0; i < TamVentana; i++)
                hann[i] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (TamVentana - 1));

            var re = new double[TamVentana];
            var im = new double[TamVentana];
            var anterior = new double[mitad];
            var flujo = new double[cuadros];
            var fft = new Fft(TamVentana);

            for (int c = 0; c < cuadros; c++)
            {
                int inicio = c * Salto;
                for (int i = 0; i < TamVentana; i++)
                {
                    re[i] = x[inicio + i] * hann[i];
                    im[i] = 0;
                }
                fft.Transformar(re, im);

                double suma = 0;
                for (int k = 1; k < mitad; k++)
                {
                    // Compresión logarítmica: los ataques suaves también cuentan
                    double magnitud = Math.Log(1 + 100 * Math.Sqrt(re[k] * re[k] + im[k] * im[k]));
                    double diferencia = magnitud - anterior[k];
                    if (diferencia > 0 && c > 0) suma += diferencia;
                    anterior[k] = magnitud;
                }
                flujo[c] = suma;
            }
            return flujo;
        }

        private static void RestarMediaLocal(double[] v, int radio)
        {
            radio = Math.Max(1, radio);
            var acumulado = new double[v.Length + 1];
            for (int i = 0; i < v.Length; i++) acumulado[i + 1] = acumulado[i] + v[i];

            var resultado = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                int a = Math.Max(0, i - radio);
                int b = Math.Min(v.Length, i + radio + 1);
                double mediaLocal = (acumulado[b] - acumulado[a]) / (b - a);
                resultado[i] = Math.Max(0, v[i] - mediaLocal);
            }
            Array.Copy(resultado, v, v.Length);
        }

        private static double[]? Autocorrelacion(double[] v, int retardoMaximo)
        {
            int n = v.Length;
            var r = new double[retardoMaximo + 1];
            for (int retardo = 0; retardo <= retardoMaximo; retardo++)
            {
                double suma = 0;
                for (int i = 0; i + retardo < n; i++) suma += v[i] * v[i + retardo];
                r[retardo] = suma / (n - retardo); // estimador insesgado
            }
            if (r[0] <= 1e-12) return null;
            for (int retardo = retardoMaximo; retardo >= 0; retardo--) r[retardo] /= r[0];
            return r;
        }

        private static double Interpolar(double[] v, double posicion)
        {
            int i = (int)Math.Floor(posicion);
            if (i < 0 || i + 1 >= v.Length) return 0;
            double t = posicion - i;
            return v[i] * (1 - t) + v[i + 1] * t;
        }

        /// <summary>FFT radix-2 iterativa (in-place), con tablas precalculadas.</summary>
        private sealed class Fft
        {
            private readonly int _n;
            private readonly int[] _inversion;
            private readonly double[] _cos;
            private readonly double[] _sen;

            public Fft(int n)
            {
                _n = n;
                int bits = (int)Math.Log2(n);
                _inversion = new int[n];
                for (int i = 0; i < n; i++)
                {
                    int r = 0;
                    for (int b = 0; b < bits; b++) r |= ((i >> b) & 1) << (bits - 1 - b);
                    _inversion[i] = r;
                }
                _cos = new double[n / 2];
                _sen = new double[n / 2];
                for (int i = 0; i < n / 2; i++)
                {
                    _cos[i] = Math.Cos(-2 * Math.PI * i / n);
                    _sen[i] = Math.Sin(-2 * Math.PI * i / n);
                }
            }

            public void Transformar(double[] re, double[] im)
            {
                for (int i = 0; i < _n; i++)
                {
                    int j = _inversion[i];
                    if (j > i)
                    {
                        (re[i], re[j]) = (re[j], re[i]);
                        (im[i], im[j]) = (im[j], im[i]);
                    }
                }

                for (int tam = 2; tam <= _n; tam <<= 1)
                {
                    int mitad = tam >> 1;
                    int paso = _n / tam;
                    for (int inicio = 0; inicio < _n; inicio += tam)
                    {
                        for (int k = 0; k < mitad; k++)
                        {
                            double wr = _cos[k * paso], wi = _sen[k * paso];
                            int a = inicio + k, b = a + mitad;
                            double tr = re[b] * wr - im[b] * wi;
                            double ti = re[b] * wi + im[b] * wr;
                            re[b] = re[a] - tr; im[b] = im[a] - ti;
                            re[a] += tr;        im[a] += ti;
                        }
                    }
                }
            }
        }
    }
}
