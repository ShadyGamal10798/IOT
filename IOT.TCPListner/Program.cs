using IOT.TCPListner;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();

    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
       
        services.AddSingleton<System.Net.Sockets.TcpClient>();
        services.AddSingleton<TcpClientService>();
    })
    .Build();

host.Run();
