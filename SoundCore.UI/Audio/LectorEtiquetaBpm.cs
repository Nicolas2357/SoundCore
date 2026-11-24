// ============================================================================
// Archivo:      LectorEtiquetaBpm.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      2.0
// Descripción:  Lee el BPM guardado en la etiqueta ID3v2 de un MP3
//               (frame "TBPM" en ID3v2.3/2.4, "TBP" en ID3v2.2).
//               Muchos archivos NO traen este dato; en ese caso devuelve null
//               y el AnalizadorBpm lo calcula a partir del audio.
// ============================================================================

using System.Globalization;
using System.Text;

namespace SoundCore.UI.Audio
{
    public static class LectorEtiquetaBpm
    {
        /// <summary>Devuelve el BPM de la etiqueta ID3v2, o null si el archivo no lo tiene o no es válido.</summary>
        public static double? LeerBpm(string ruta)
        {
            try
            {
                using var fs = File.OpenRead(ruta);
                var cabecera = new byte[10];
                if (fs.Read(cabecera, 0, 10) != 10) return null;
                if (cabecera[0] != 'I' || cabecera[1] != 'D' || cabecera[2] != '3') return null;

                int version = cabecera[3];          // 2, 3 o 4
                byte banderas = cabecera[5];
                int tamEtiqueta = Syncsafe(cabecera, 6);
                if (version < 2 || version > 4 || tamEtiqueta <= 0) return null;

                var etiqueta = new byte[tamEtiqueta];
                int leidos = fs.Read(etiqueta, 0, tamEtiqueta);
                if (leidos <= 0) return null;

                // Desincronización a nivel etiqueta (v2.2 / v2.3): FF 00 → FF
                if ((banderas & 0x80) != 0 && version < 4)
                    etiqueta = QuitarDesincronizacion(etiqueta, leidos, out leidos);

                int pos = 0;

                // Cabecera extendida (opcional)
                if ((banderas & 0x40) != 0 && version >= 3 && leidos >= 4)
                {
                    pos = version == 4
                        ? Syncsafe(etiqueta, 0)                 // v2.4: incluye sus propios 4 bytes
                        : EnteroBigEndian(etiqueta, 0) + 4;     // v2.3: no los incluye
                }

                int tamId = version == 2 ? 3 : 4;
                int tamCabeceraFrame = version == 2 ? 6 : 10;
                string idBuscado = version == 2 ? "TBP" : "TBPM";

                while (pos + tamCabeceraFrame <= leidos)
                {
                    if (etiqueta[pos] == 0) break; // relleno (padding): ya no hay frames

                    string id = Encoding.ASCII.GetString(etiqueta, pos, tamId);
                    int tamFrame = version switch
                    {
                        2 => (etiqueta[pos + 3] << 16) | (etiqueta[pos + 4] << 8) | etiqueta[pos + 5],
                        3 => EnteroBigEndian(etiqueta, pos + 4),
                        _ => Syncsafe(etiqueta, pos + 4)
                    };
                    if (tamFrame <= 0 || pos + tamCabeceraFrame + tamFrame > leidos) break;

                    if (id == idBuscado)
                    {
                        string texto = DecodificarTexto(etiqueta, pos + tamCabeceraFrame, tamFrame);
                        return InterpretarBpm(texto);
                    }

                    pos += tamCabeceraFrame + tamFrame;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return null;
        }

        /// <summary>Convierte el texto de la etiqueta ("128", "127.98", "128 BPM") en un número válido.</summary>
        public static double? InterpretarBpm(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            var limpio = new StringBuilder();
            foreach (char c in texto.Trim())
            {
                if (char.IsDigit(c)) limpio.Append(c);
                else if ((c == '.' || c == ',') && limpio.Length > 0) limpio.Append('.');
                else if (limpio.Length > 0) break;
            }

            if (double.TryParse(limpio.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double bpm)
                && bpm >= 40 && bpm <= 300)
                return bpm;

            return null; // "0", vacío o fuera de rango = el archivo no tiene BPM real
        }

        private static string DecodificarTexto(byte[] datos, int inicio, int longitud)
        {
            if (longitud <= 1) return string.Empty;
            byte codificacion = datos[inicio];
            int desde = inicio + 1, cuantos = longitud - 1;

            string texto = codificacion switch
            {
                0 => Encoding.Latin1.GetString(datos, desde, cuantos),
                1 => DecodificarUtf16ConBom(datos, desde, cuantos),
                2 => Encoding.BigEndianUnicode.GetString(datos, desde, cuantos),
                3 => Encoding.UTF8.GetString(datos, desde, cuantos),
                _ => string.Empty
            };
            return texto.TrimEnd('\0').Trim();
        }

        private static string DecodificarUtf16ConBom(byte[] datos, int desde, int cuantos)
        {
            if (cuantos >= 2 && datos[desde] == 0xFE && datos[desde + 1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(datos, desde + 2, cuantos - 2);
            if (cuantos >= 2 && datos[desde] == 0xFF && datos[desde + 1] == 0xFE)
                return Encoding.Unicode.GetString(datos, desde + 2, cuantos - 2);
            return Encoding.Unicode.GetString(datos, desde, cuantos);
        }

        private static int Syncsafe(byte[] b, int i) =>
            ((b[i] & 0x7F) << 21) | ((b[i + 1] & 0x7F) << 14) | ((b[i + 2] & 0x7F) << 7) | (b[i + 3] & 0x7F);

        private static int EnteroBigEndian(byte[] b, int i) =>
            (b[i] << 24) | (b[i + 1] << 16) | (b[i + 2] << 8) | b[i + 3];

        private static byte[] QuitarDesincronizacion(byte[] datos, int longitud, out int nuevaLongitud)
        {
            var salida = new byte[longitud];
            int j = 0;
            for (int i = 0; i < longitud; i++)
            {
                salida[j++] = datos[i];
                if (datos[i] == 0xFF && i + 1 < longitud && datos[i + 1] == 0x00) i++;
            }
            nuevaLongitud = j;
            return salida;
        }
    }
}
