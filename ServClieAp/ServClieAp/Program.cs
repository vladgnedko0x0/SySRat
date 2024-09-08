static async Task Main(string[] args)
{
    IPAddress localAddress = IPAddress.Any;
    const int port = 7778;
    var server = new TcpListener(localAddress, port);
    server.Start();

    Console.WriteLine("Server started. Listening for connections...");
    Task.Run(() => Logger.ReaderKeys()); // Start reading keys asynchronously

    while (true)
    {
        TcpClient client = await server.AcceptTcpClientAsync();
        _ = Task.Run(() => HandleClientAsync(client)); // Handle clients asynchronously
    }
}

static async Task HandleClientAsync(TcpClient client)
{
    NetworkStream stream = client.GetStream();
    try
    {
        while (true)
        {
            string command = await ReceiveCommandAsync(stream);
            if (command == null)
                break;

            string response = ProcessCommand(command, stream);
            await SendResponseAsync(stream, response);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
    }
    finally
    {
        stream.Close();
        client.Close();
    }
}
