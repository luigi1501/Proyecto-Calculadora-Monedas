using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using TasaPlus.Shared.Services;

namespace TasaPlusDesktop
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddSingleton<TasasDataService>();
            var serviceProvider = services.BuildServiceProvider();

            var tasasService = serviceProvider.GetRequiredService<TasasDataService>();
            tasasService.StartPeriodicRefresh(TimeSpan.FromSeconds(30));

            // Abrir como programa de escritorio independiente (Ventana App dedicada sin pestanas de navegador)
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string profileDir = Path.Combine(appData, "TasaPlus", "DesktopProfile");
            Directory.CreateDirectory(profileDir);

            string url = "https://tasa-plus.vercel.app/";
            bool launched = false;

            try
            {
                string edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe");
                if (!File.Exists(edgePath))
                {
                    edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe");
                }

                if (File.Exists(edgePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = edgePath,
                        Arguments = $"--app=\"{url}\" --user-data-dir=\"{profileDir}\" --name=\"TasaPlus\"",
                        UseShellExecute = true
                    });
                    launched = true;
                }
            }
            catch { }

            if (!launched)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }
    }
}
