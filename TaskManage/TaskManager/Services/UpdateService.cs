using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace TaskManager.Services
{
    public static class UpdateService
    {
        static double currentVer;
        static double targetVer;
        public static void CheckForUpdate()
        {
            double.TryParse(Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(".", ""), out currentVer);
            FileVersionInfo ServerFileVersion = FileVersionInfo.GetVersionInfo(@"\\moscow\hdfs\WORK\Архив необычных операций\ОРППА\Programs\Tasks\TasksDK.exe");
            double.TryParse(ServerFileVersion.FileVersion.Replace(".", string.Empty), out targetVer);
            if (currentVer < targetVer)
            {
                MessageBox.Show("Обнаружена новая версия программы. TasksDK будет перезапущен после обновления", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
                List<string> updateText = new List<string>();

                updateText.Add("-g");

                foreach (string fileName in Directory.GetFiles(@"\\moscow\hdfs\WORK\Архив необычных операций\ОРППА\\Programs\Tasks"))
                {
                    updateText.Add($"\"{fileName}\"");
                }

                updateText.Add("-k");
                updateText.Add($"{Process.GetCurrentProcess().ProcessName}");

                updateText.Add("-r");
                updateText.Add($"TasksDK.exe");

                Process.Start("updaterForm.exe", string.Join(" ", updateText.ToArray()));
            }
        }
    }
}
