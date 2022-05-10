using NLog;
using System;
using System.IO;
using System.Windows.Forms;

namespace StardewUpdater
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            #region Logging
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("SU_LogFile") { FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StardewUpdater", "Logs", "su.log") };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");     
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            #endregion

            Application.Run(new StardewUpdater());
        }
    }
}
