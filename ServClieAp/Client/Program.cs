using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static bool endOfDirectory=false;
    static void Main(string[] args)
    {
        const int port = 7778;
        IPAddress serverAddress=null;

        while (true)
        {
            Console.Write("Enter IP address(xxx.xxx.x.xxx) for connect, or enter 0 to scan automatically>> ");
            string ipAddressString = Console.ReadLine();

            if (ipAddressString == "0")
            {
                // Сканирование доступных IP-адресов на локальной сети
                Console.WriteLine("Scanning for available IP addresses on the local network...");
                IPAddress localIpAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

                if (localIpAddress == null)
                {
                    Console.WriteLine("Unable to determine the local IP address.");
                    continue;
                }

                string[] ipParts = localIpAddress.ToString().Split('.');
                string baseIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}.";

                for (int i = 120; i <= 122; i++)
                {
                    string currentIp = $"{baseIp}{i}";
                    try
                    {
                        IPAddress.TryParse(currentIp, out serverAddress);
                        TcpClient client1 = new TcpClient();
                        client1.Connect(serverAddress, port);
                        Console.WriteLine($"Connected to server at IP address: {currentIp}");
                        client1.Close(); // Закрываем соединение после успешного подключения
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("IP Addres " + currentIp + " dont avaliveble");
                        // Произошла ошибка при подключении, продолжаем сканирование
                    }
                }
            }
            else
            {
                if (!IPAddress.TryParse(ipAddressString, out serverAddress))
                {
                    Console.WriteLine("Invalid IP address format. Please enter a valid IPv4 address.");
                    continue;
                }
            }

            break; // Выходим из цикла после успешного получения IP-адреса
        }
        if (serverAddress == null) { return; }

        // Теперь у вас есть serverAddress для подключения к серверу
        TcpClient client = new TcpClient();
        client.Connect(serverAddress, port);
        Console.WriteLine("Connected to server.");

        try
        {
            NetworkStream stream = client.GetStream();

            while (true)
            {
                try
                {
                    Console.Write("Enter command: ");
                    string command = Console.ReadLine();
                    if (command.ToLower() == "exit")
                        break;
                    if(command.ToLower() == "help")
                    {
                        HelpMenu();
                    }
                    SendCommand(stream, command);
                    if (!command.StartsWith("copy") && !command.StartsWith("send"))
                    {
                        if (command.StartsWith("delete"))
                        {
                            string response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                            response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                        }
                        else
                        {
                            string response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                        }
                    }else if (command.StartsWith("copy"))
                    {
                        ReceiveFileOrDirectory(stream);
                    }
                    else
                    {
                        string response = ReceiveResponse(stream);
                        Console.WriteLine(response);

                        if (response.StartsWith("Directory does not exist")) continue;

                        while (true)
                        {
                            Console.Write("Enter source path: ");
                            string sourcePath = Console.ReadLine();
                            if (sourcePath[0] == '\"')
                            {
                                sourcePath = sourcePath.Remove(0, 1);
                            }
                            if (sourcePath[sourcePath.Length - 1] == '\"')
                            {
                                sourcePath = sourcePath.Remove(sourcePath.Length - 1, 1);
                            }
                            if (!Directory.Exists(sourcePath)&&!File.Exists(sourcePath)) { Console.WriteLine("Directory on local does not exist!"); continue; }
                            
                            SendFileOrDirectory(sourcePath, stream);
                            response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                            response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                            if (response.StartsWith("Recevide"))
                            {
                                break;
                            }
                        }
                    }           
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exeption in main: " + ex.Message);
                    
                }
                endOfDirectory = false;
                
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
    static void HelpMenu()
    {
        Console.WriteLine("Welcome to SysRat Console Application");
        Console.WriteLine("=======================================");
        Console.WriteLine("Description:");
        Console.WriteLine("SysRat is a console application that provides remote management capabilities for a server. It allows you to perform various file and process operations on the server remotely.");
        Console.WriteLine();

        Console.WriteLine("Functions:");
        PrintFunctionDescription("dir <serverPath>", "Get files and directories at the specified path on the server.");
        PrintFunctionDescription("copy <serverPath>", "Copy a file from the server at the specified path.");
        PrintFunctionDescription("send <serverPath> <localPath>", "Send a folder or file from the client to the specified path on the server.");
        PrintFunctionDescription("delete <servverPath>", "Delete a file or folder at the specified path on the server.");
        PrintFunctionDescription("move <sourcePath> <destinationPath>", "Move a file or folder from the source path to the destination path on the server.");
        PrintFunctionDescription("run <pathToFileOrExe>", "Run a file or application at the specified path on the server.");
        PrintFunctionDescription("proc <type(get)>", "Get all processes in alphabetical order on the server.");
        PrintFunctionDescription("proc <type(kill)> <processID>", "Terminate a process on the server by its ID.");
        Console.WriteLine();

        Console.WriteLine("Usage:");
        PrintFunctionUsage(@"dir <serverPath>", @"dir c:\");
        PrintFunctionUsage(@"copy <serverPath>", @"copy c:\Program Files\Light Shot -- its a test folder");
        PrintFunctionUsage(@"send <serverPath> <localPath>", @"send c:\  and type u local folder с:\Program Files\My Program");
        PrintFunctionUsage(@"delete <path>", @"delete c:\Program Files\Test server folder");
        PrintFunctionUsage(@"move <sourcePath> <destinationPath>", @"move c:\Program Files\TestFolder c:\Program Files\AnotherFolder");
        PrintFunctionUsage(@"run <pathToFileOrExe>", @"run c:\Program Files\TestProgram\test.exe");
        PrintFunctionUsage(@"proc <type(get)>", @"proc get");
        PrintFunctionUsage(@"proc <type(kill)> <processID>", @"proc kill 1234 >> 1234 its a process ID");
    }
    static void PrintFunctionDescription(string command, string description)
    {
        Console.WriteLine($"- {command}: {description}");
    }

    static void PrintFunctionUsage(string command, string usage)
    {
        Console.WriteLine($"- {command}: {usage}");
    }
    static void SendCommand(NetworkStream stream, string command)
    {
        byte[] commandBytes = Encoding.UTF8.GetBytes(command);
        stream.Write(commandBytes, 0, commandBytes.Length);
        stream.Flush();
    }

    static string ReceiveResponse(NetworkStream stream)
    {
        byte[] buffer = new byte[20000];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    static void ReceiveFileOrDirectory(NetworkStream stream, string currentDirectory = "")
    {
        while (true)
        {
            byte[] buffer = new byte[8192];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if (message.StartsWith("Directory"))
            {
                string directoryName = message.Split('\\').LastOrDefault();
                string directoryPath = Path.Combine(currentDirectory, directoryName);

                Directory.CreateDirectory(directoryPath);
                Console.WriteLine($"Directory created: {directoryPath}");
                EndProcessMessage(stream, "Directory get ok");
                ReceiveFileOrDirectory(stream, directoryPath); // Рекурсивно обрабатываем подпапки
            }
            else if (message.StartsWith("End of directory"))
            {
                Console.WriteLine(message);
                EndProcessMessage(stream, "End of directory ok");
                return; // Возвращаемся к предыдущей папке после завершения обработки текущего каталога
            }
            else
            {
                string fileName = message.Split('\'')[1];
                string filePath = Path.Combine(currentDirectory, fileName);
                ReceiveFile(stream, filePath);
            }
        }
    }

    static void EndProcessMessage(NetworkStream stream, string processName)
    {
        byte[] confirmationMessage = Encoding.UTF8.GetBytes(processName);
        stream.Write(confirmationMessage, 0, confirmationMessage.Length);
        stream.Flush();
    }
    static void ReceiveFile(NetworkStream stream, string defaultFileName)
    {
        EndProcessMessage(stream, "FileNameReceived");
        byte[] fileSizeBuffer = new byte[sizeof(long)];
        stream.Read(fileSizeBuffer, 0, fileSizeBuffer.Length);
        long fileSize = BitConverter.ToInt64(fileSizeBuffer, 0);
        EndProcessMessage(stream, "FileSizeReceived");
        using (FileStream fileStream = new FileStream(defaultFileName, FileMode.Create))
        {
            byte[] buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while (totalBytesRead < fileSize && (bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }
        }
        // Отправляем подтверждение серверу о том, что файл был успешно принят
        EndProcessMessage(stream, "FileReceived");
        Console.WriteLine($"File '{defaultFileName}' received from server.");
    }

    static string SendFileOrDirectory(string path, NetworkStream stream)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return "Please provide a file or directory path.";

            if (File.Exists(path))
            {
                SendFile(path, stream);
                return $"File '{Path.GetFileName(path)}' sent.";
            }
            else if (Directory.Exists(path))
            {
                SendDirectory(path, stream);
                return $"Directory '{Path.GetFileName(path)}' sent.";
            }
            else
            {
                return "File or directory not found.";
            }
        }
        catch (Exception ex)
        {
            return $"Error sending file or directory: {ex.Message}";
        }
        finally
        {
            SendResponse(stream, "End of directory");
        }
    }

    static void SendDirectory(string directoryPath, NetworkStream stream)
    {
        if (directoryPath.Last() != '\\')
        {
            SendResponse(stream, $"Directory: {Path.GetDirectoryName(directoryPath + "\\")}");
            if (!EndProcessMessageGeter(stream, "Directory get ok"))
            {
                Console.WriteLine("Directory get error");
            }
        }
        else
        {
            SendResponse(stream, $"Directory: {Path.GetDirectoryName(directoryPath)}");
            if (!EndProcessMessageGeter(stream, "Directory get ok"))
            {
                Console.WriteLine("Directory get error");
            }
        }
        // Отправляем сообщение о начале передачи содержимого каталога


        // Отправляем файлы в каталоге
        foreach (string filePath in Directory.GetFiles(directoryPath))
        {
            SendFile(filePath, stream);
        }

        // Рекурсивно отправляем содержимое подкаталогов
        foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
        {
            SendDirectory(subDirectoryPath, stream);
        }

        // Сообщаем о завершении передачи каталога
        SendResponse(stream, "End of directory");
        if (!EndProcessMessageGeter(stream, "End of directory ok"))
        {
            Console.WriteLine("End of directory client error");
        }
    }
    static bool EndProcessMessageGeter(NetworkStream stream, string processName)
    {
        byte[] confirmationBuffer = new byte[8192];
        int confirmationBytesRead = stream.Read(confirmationBuffer, 0, confirmationBuffer.Length);
        string confirmationMessage = Encoding.UTF8.GetString(confirmationBuffer, 0, confirmationBytesRead).Trim();
        if (confirmationMessage == processName)
        {
            return true;
        }
        else
        {
            return false;
        }
        // НАДО ЗАКОНЧИТЬ ЕТУ ФУНКЦИЮ ПРИЕМА ПОДТВЕРЖДЕНИЯ ОТ КЛИЕНТА!!!!!!!!!!!!!!!!!
    }
    static void SendFile(string filePath, NetworkStream stream)
    {
        long fileSize = new FileInfo(filePath).Length;
        string fileNameMessage = $"File: '{Path.GetFileName(filePath)}'";
        byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileNameMessage);
        stream.Write(fileNameBytes, 0, fileNameBytes.Length);
        stream.Flush();

        // Ожидаем подтверждение от клиента о том, что файл был успешно принят

        if (!EndProcessMessageGeter(stream, "FileNameReceived"))
        {
            Console.WriteLine("Error: File name not received confirmation received.");
            return;
        }

        byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
        stream.Write(fileSizeBytes, 0, fileSizeBytes.Length);
        stream.Flush();
        if (!EndProcessMessageGeter(stream, "FileSizeReceived"))
        {
            Console.WriteLine("Error: File size not received confirmation received.");
            return;
        }
        using (var fileStream = File.OpenRead(filePath))
        {
            fileStream.CopyTo(stream);
        }
        if (!EndProcessMessageGeter(stream, "FileReceived"))
        {
            Console.WriteLine("Error: File not received confirmation received.");
            return;
        }
    }
    static void SendResponse(NetworkStream stream, string response)
    {
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);
        stream.Flush();
    }
}
