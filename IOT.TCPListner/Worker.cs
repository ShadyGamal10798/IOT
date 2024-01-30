using IOT.TCPListner.Data;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using Teltonika.Codec;

namespace IOT.TCPListner
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;
        private static readonly Dictionary<int, TcpClientService> clientServices = new Dictionary<int, TcpClientService>();

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = StartCommandPipeServer(); // New command listener

            while (!stoppingToken.IsCancellationRequested)
            {
                //int decValue = Convert.ToInt16("3836", 16);
                //Console.WriteLine(decValue);

                //Console.WriteLine("To Start The Listener, Click Any Key : ");
                //Console.ReadKey();

                //Get Ip And Port From App Settings
                IPAddress ip = IPAddress.Parse(configuration.GetValue<string>("IpAddress"));
                int port = configuration.GetValue<int>("Port");

                Console.WriteLine($"Starting listener Through... {ip}:{port}");

                //Create Tcp Listner
                var server = new TcpListener(ip, port);
                try
                {
                    server.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex}");
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var client = await server.AcceptTcpClientAsync();
                        try
                        {
                            SetKeepAlive(client);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting keep-alive: {ex.Message}");
                        }
                        var cw = new TcpClientService(client,configuration);
                        clientServices[0] = cw;  // Store the service instance in the dictionary.
                        _ = Task.Run(() => cw.Run());

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex} ,Error handling client connection");
                        break;
                    }
                }
            }



        }
        private void SetKeepAlive(TcpClient client)
        {
            int size = sizeof(uint);
            uint time = 5000; // e.g., start sending keep-alive packets after 5 seconds of inactivity
            uint interval = 1000; // e.g., send subsequent packets every 1 second

            byte[] inOptionValues = new byte[size * 3];
            BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);       // Enable keep-alive
            BitConverter.GetBytes(time).CopyTo(inOptionValues, size);       // Time to start sending keep-alive packets
            BitConverter.GetBytes(interval).CopyTo(inOptionValues, size * 2); // Interval between packets

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            client.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        private static async Task StartCommandPipeServer()
        {
            while (true)
            {
                using (var server = new NamedPipeServerStream("CommandPipe", PipeDirection.In))
                {
                    Console.WriteLine("Waiting for connection...");
                    await server.WaitForConnectionAsync();
                    Console.WriteLine("Client connected.");

                    using (var reader = new StreamReader(server))
                    {
                        string message = await reader.ReadToEndAsync();
                        await HandleIncomingMessage(message);
                    }
                }
            }
        }
        private static async Task HandleIncomingMessage(string message)
        {
            try
            {
                Console.WriteLine($"Received: {message}");

                var parts = message.Split(new[] { ':' }, 3);
                if (parts.Length == 3)
                {
                    string type = parts[0];
                    string identifier = parts[1];
                    string content = parts[2];

                    if (type == "COMMAND")
                    {
                        // Handle the command using ITcpClientService
                        // For example, you might have a method like this:
                        await SendCommandToClient(identifier, content);
                    }
                    // You can add more types or handling logic as needed
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message: {ex.Message}");
            }
        }

        // Dummy implementation - replace this with your actual ITcpClientService method
        private static async Task SendCommandToClient(string imei, string command)
        {

            clientServices[0].SendCommandToClient(imei, command);
            Console.WriteLine($"Sending '{command}' to device with IMEI: {imei}");
            // Implement the actual logic to send the command to the device here...
            await Task.Delay(10); // Simulate async work
        }
    }
}