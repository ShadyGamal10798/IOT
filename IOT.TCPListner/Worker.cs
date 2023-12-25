using System.Net;
using System.Net.Sockets;
using Teltonika.Codec;

namespace IOT.TCPListner
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //int decValue = Convert.ToInt16("3836", 16);
                //Console.WriteLine(decValue);

                Console.WriteLine("To Start The Listener, Click Any Key : ");
                Console.ReadKey();

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
                        var cw = new TcpClientService(client);
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
    }
}