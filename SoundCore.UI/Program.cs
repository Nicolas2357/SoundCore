// ============================================================================
// Archivo:      Program.cs
// Proyecto:     SoundCore Engine GUI
// Integrantes:  Nicolas Ponce Carmona (No. Control: I25050376)
// Fecha:        Martes 29 de Septiembre de 2026
// Versión:      1.0
// ============================================================================
namespace SoundCore.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}