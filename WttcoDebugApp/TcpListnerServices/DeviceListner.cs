using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WttcoDebugApp.TcpListnerServices
{
    internal class DeviceListner
    {
        public void startService()
        {
            //IPAddress ip = IPAddress.Parse("172.31.30.51");
            //int port = 8061;

            //Console.WriteLine($"Starting listener Through... {ip}:{port}");

            ////Create Tcp Listner
            //var server = new TcpListener(ip, port);
            //try
            //{
            //    server.Start();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"{ex}");
            //}

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        var client = await server.AcceptTcpClientAsync();
            //        var cw = new TcpClientService(client);
            //        clientServices[0] = cw;  // Store the service instance in the dictionary.
            //        _ = Task.Run(() => cw.Run());

            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"{ex} ,Error handling client connection");
            //        break;
            //    }
            //}
        }
    }
}
