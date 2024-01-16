using IOT.TCPListner;
using IOT.TCPListner.Data;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();

    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
       
        services.AddSingleton<System.Net.Sockets.TcpClient>();
        services.AddSingleton<TcpClientService>();

        // Add the DbContext to the service collection
        services.AddDbContext<ListenerDbContext>(options =>
        {
            // Replace with your connection string
            var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });
    })
    .Build();

host.Run();
