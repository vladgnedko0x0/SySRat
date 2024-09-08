using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static bool endOfDirectory = false;

    static void Main(string[] args)
    {
        const int port = 7778;
        IPAddress serverAddress = null;

        while (true)
        {
            Console.Write("Enter IP address(xxx.xxx.x.xxx) to connect, or enter 0 to scan automatically>> ");
            string ipAddressString = Console.ReadLine();

            if (ipAddressString == "0")
            {
                // Scan available IP addresses on the local network
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
                        client1.Close(); // Close connection after successful connection
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("IP Address " + currentIp + " not available");
                        // An error occurred while connecting, continue scanning
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

            break; // Exit the loop after successfully obtaining the IP address
        }

        if (serverAddress == null) { return; }

        // Now you have serverAddress for connecting to the server
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
                    if (command.ToLower() == "help")
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
                    }
                    else if (command.StartsWith("copy"))
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
                            if (!Directory.Exists(sourcePath) && !File.Exists(sourcePath)) 
                            { 
                                Console.WriteLine("Directory on local does not exist!"); 
                                continue; 
                            }
                            
                            SendFileOrDirectory(sourcePath, stream);
                            response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                            response = ReceiveResponse(stream);
                            Console.WriteLine(response);
                            if (response.StartsWith("Received"))
                            {
                                break;
                            }
                        }
                    }           
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception in main: " + ex.Message);
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
       
