using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace TaskManager.Services
{
    public static class UpdateService
    {
        static double CurrentFileVersion;
        static double ServerFileVersion;
        const string SERVER_FILE_PATH = @"\\moscow\hdfs\WORK\Архив необычных операций\ОРППА\Programs\Tasks\TasksDK.exe";
        const string SERVER_ROOT_PATH = @"\\moscow\hdfs\WORK\Архив необычных операций\ОРППА\\Programs\Tasks";

        static UpdateService()
        {
            GetCurrentVersion();
            TryGetServerVersion();
        }

        public static void UpdateIfNewer()
        {
            if (!IsUpdatePossible())
            {
                ShowUpdateErrorMessage();
                return;
            }

            if (IsServerVersionNewer())
            {
                ShowUpdateMessage();
                Update();
            }
        }

        private static bool IsUpdatePossible()
        {
            return Properties.Settings.Default.IsWorkingAtHome || !Directory.Exists(SERVER_FILE_PATH);
        }

        private static bool IsServerVersionNewer()
        {
            return CurrentFileVersion < ServerFileVersion;
        }

        /// <summary>
        /// UpdaterForm.exe принимает текстовую строку в качестве аргумента.
        /// При запуске UpdaterForm.exe удаляет все файлы из директории,  где он находится,
        /// а потом выполняет все указания, указанные в аргументе командной строки.
        /// В текстовой строке приняты следующие обозначения:
        /// -g (Get) Например '-g "FileName" "FileName" ... "FileName"' копирует файлы в место расположения UpdaterForm.exe
        /// -k (Kill) Например '-k "ProcessName"' Уничтожает указанный процесс
        /// -r (Run) Например '-r "FileName"' Запускает указанный файл
        /// </summary>
        private static void Update()
        {
            List<string> UpdaterFormCommands = new List<string>();

            UpdaterFormCommands.Add("-g");
            foreach (string fileName in Directory.GetFiles(SERVER_ROOT_PATH))
            {
                UpdaterFormCommands.Add($"\"{fileName}\"");
            }
            UpdaterFormCommands.Add("-k");
            UpdaterFormCommands.Add($"{Process.GetCurrentProcess().ProcessName}");
            UpdaterFormCommands.Add("-r");
            UpdaterFormCommands.Add($"TasksDK.exe");

            Process.Start("updaterForm.exe", string.Join(" ", UpdaterFormCommands.ToArray()));
        }

        private static void ShowUpdateMessage()
        {
            MessageBox.Show("Обнаружена новая версия программы. TasksDK будет перезапущен после обновления", 
                "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private static void ShowUpdateErrorMessage()
        {
            MessageBox.Show("Приложению не удалось проверить наличие обновлений. Обратитесь к разработчику.", 
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void GetCurrentVersion()
        {
            double.TryParse(Assembly.GetExecutingAssembly().GetName().Version.ToString().Replace(".", ""), out CurrentFileVersion);
        }

        private static void TryGetServerVersion()
        {
            if (IsUpdatePossible())
            {
                FileVersionInfo ServerFileVersion = FileVersionInfo.GetVersionInfo(SERVER_FILE_PATH);
                double.TryParse(ServerFileVersion.FileVersion.Replace(".", string.Empty), out UpdateService.ServerFileVersion);
            }
        }
    }
}
