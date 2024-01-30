using IOT.TCPListner.Data;
using IOT.TCPListner.Data.Entities;
using IOT.TCPListner.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Pipes;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Teltonika.Codec;
using Teltonika.Codec.Model;

namespace IOT.TCPListner
{
    public class TcpClientService :ITcpClientService
    {
        readonly TcpClient _client;
        private readonly IConfiguration _configuration;
        private static List<ConnectedClient> _connectedClients = new List<ConnectedClient>();

        public TcpClientService(TcpClient client,IConfiguration configuration)
        {
            _client = client;
            _configuration = configuration;
        }

        public async Task Run()
        {



            var stream = _client.GetStream();
            

                Console.WriteLine(DateTime.Now + " Received connection request from " + _client.Client.RemoteEndPoint);

                var fullPacket = new List<byte>(); // Vector
                int? avlDataLength = null;
                
                var bytes = new byte[4096];
                var connected = false;
                int length;

                // Loop to receive all the data sent by the client.
                while ((length = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
                {
                    Console.WriteLine(string.Format("\n {0} - received [{1}] -> [{2}] \n", DateTime.Now, string.Join("", bytes.Take(length).Select(x => x.ToString("X2")).ToArray()) , GetIMEI(string.Join("", bytes.Take(length).Select(x => x.ToString("X2")).ToArray())) ));
                    var iemi = "";
                    if(!_connectedClients.Exists(i => i.TcpClient == _client))
                    {
                        iemi = GetIMEI(string.Join("", bytes.Take(length).Select(x => x.ToString("X2")).ToArray()));
                        Console.WriteLine($"iemi::::: {iemi}");

                    }
                    else
                    {
                        var iemiPrevious = _connectedClients.FirstOrDefault(i=>i.TcpClient == _client).Imei;
                        iemi =iemiPrevious ;
                    }
                    byte[] response;
                    

                    
                    if (!_connectedClients.Exists(i => i.Imei == iemi))
                    {
                        if (iemi.ToString() != "")
                        {
                            var connectedClient = new ConnectedClient
                            {
                                TcpClient = _client,
                                Imei = iemi.ToString()
                            };
                            _connectedClients.Add(connectedClient);
                        }
                        }
                        else {

                        var client = _connectedClients.FirstOrDefault(i => i.Imei == iemi);
                        
                        client.TcpClient = _client;
                        
                        iemi = client.Imei;
                    }

                    if (!connected)
                    {
                        // Accept imei
                        response = new byte[] { 01 };
                        connected = true;
                        await stream.WriteAsync(response, 0, response.Length);
                        Array.Clear(bytes, 0, bytes.Length);
                        Console.WriteLine(string.Format("{0} - responded [{1}]", DateTime.Now, string.Join("", response.Select(x => x.ToString("X2")).ToArray())));
                    }
                    else
                    {
                        fullPacket.AddRange(bytes.Take(length));
                        Array.Clear(bytes, 0, bytes.Length);

                        var count = fullPacket.Count;

                        // continue if there is not enough bytes to get avl data array length
                        if (count < 8) continue;

                        avlDataLength = avlDataLength ?? BytesSwapper.Swap(BitConverter.ToInt32(fullPacket.GetRange(4, 4).ToArray(), 0));

                        var packetLength = 8 + avlDataLength + 4;
                        if (count > packetLength)
                        {
                            Console.WriteLine("Too much data received.");
                            throw new ArgumentException("Too much data received.");
                        }
                        // continue if not all data received
                        if (count != packetLength) continue;

                        // Decode tcp packet
                        var decodedData = DecodeTcpPacket(fullPacket.ToArray());

                        // Serialize the object to JSON
                        string jsonString = JsonConvert.SerializeObject(decodedData, Formatting.Indented);

                        

                        response = BitConverter.GetBytes(BytesSwapper.Swap(decodedData.AvlData.DataCount));

                        await stream.WriteAsync(response, 0, response.Length);

                        avlDataLength = null;
                        fullPacket.Clear();
                        CalcMemory();
                        var deserialized  = JsonConvert.DeserializeObject<object>(jsonString);
                        foreach (var c in _connectedClients)
                        {
                            string message = jsonString; // jsonString contains the message you want to print

                            string color = iemi == "863540062368775" ? "green" : "blue";
                            string windowsPrinterPath = @"C:\Users\Administrator\source\repos\IOT\WinPrinter\bin\Debug\net7.0-windows\WinPrinter.exe";
                            using (var clientt = new NamedPipeClientStream(".", "WinPrinterPipe", PipeDirection.Out))
                            {
                                try
                                {
                                    clientt.Connect(1000); // Timeout to avoid hanging
                                    using (var writer = new StreamWriter(clientt))
                                    {
                                        writer.AutoFlush = true;
                                        await writer.WriteAsync($"{color}:{message}"); // Write the message to the pipe
                                    }
                                }
                                catch (TimeoutException)
                                {
                                    Console.WriteLine("Failed to connect to the pipe server.");
                                }
                            }

                        //add to db

                        var optionsBuilder = new DbContextOptionsBuilder<ListenerDbContext>();
                        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")); 

                        var options = optionsBuilder.Options;

                        using (var context = new ListenerDbContext(options))
                        {
                            var avlDataDB = new AVLData()
                            {
                                IMEI = iemi,
                                ListeningDate = DateTime.Now,
                                CodecID = decodedData.AvlData.CodecId,
                                DataCount = decodedData.AvlData.DataCount,
                                Longitude = decodedData.AvlData.Data.FirstOrDefault().GpsElement.X.ToString(),
                                Latitiude = decodedData.AvlData.Data.FirstOrDefault().GpsElement.Y.ToString(),
                                Altitude = decodedData.AvlData.Data.FirstOrDefault().GpsElement.Altitude,
                                Angle = decodedData.AvlData.Data.FirstOrDefault().GpsElement.Angle,
                                Satellites = decodedData.AvlData.Data.FirstOrDefault().GpsElement.Satellites,
                                Speed = decodedData.AvlData.Data.FirstOrDefault().GpsElement.Speed,
                                OriginalJson = jsonString,
                                  
    };

                            // Add the new object to the context
                            await context.AVLsData.AddAsync(avlDataDB);

                            // Save changes to the database
                            await context.SaveChangesAsync();
                        }
                        }


                        var client = _connectedClients.Where(i => i.Imei == iemi).FirstOrDefault();
                         client.DataCount = response;
                        
                        

                        Console.WriteLine(string.Format("{0} - responded [{1}]", DateTime.Now, string.Join("", response.Select(x => x.ToString("X2")).ToArray())));
                        
                         //SendCommandToClient("863540062368775", "00000000000000140C01050000000C7365746469676F75742031300100002ED4","before sleep");
                        //Thread.Sleep(9000);
                         //SendCommandToClient("863540062368775", "00000000000000140C01050000000C7365746469676F75742030310100007E84","after sleep");

                        //Console.ReadKey();
                    

                }
            }
        }

        private static TcpDataPacket DecodeTcpPacket(byte[] request)
        {
            var reader = new ReverseBinaryReader(new MemoryStream(request));
            var decoder = new DataDecoder(reader);

            return decoder.DecodeTcpData();
        }
        public async Task SendCommandToClient(string imei, string command)
        {
            TcpClient targetClient = GetClientByImei(imei);

            if (targetClient != null)
            {
                await SendCommand(targetClient, command,imei);
            }
            else
            {
                Console.WriteLine($"Device with IMEI {imei} not found.");
                // Handle the case when the device is not connected
            }


           
        }
        private async Task SendCommand(TcpClient client, string command, string imei)
        {
            try
            {
                int maxTries = 10;
                int attempt = 0;
               
                    var checkConnection = client.Connected;
                    //while (!_client.Connected )
                    //{
                    //    await Task.Delay(2000);
                    //    attempt++;
                    //}
                    if(client.Connected)
                    {
                    Console.WriteLine("Hash:::",client.Client.GetHashCode());
                    var stream = client.GetStream();
                    Console.WriteLine($"{imei}::::",client.GetHashCode());
                            //send 01
                            //var response = new byte[] { 01 };
                            //await stream.WriteAsync(response, 0, response.Length);
                            //byte[] responseFirst = new byte[100];
                            ////await stream.ReadAsync(responseFirst, 0, responseFirst.Length);
                            ////Console.WriteLine($"Response after 01: {BitConverter.ToString(responseFirst)}");

                            ////send packets count
                            //response = _connectedClients.FirstOrDefault(c => c.TcpClient == client)?.DataCount;
                            //await stream.WriteAsync(response, 0, response.Length);

                            //  Send command
                            // Lock
                            //00000000000000140C01050000000C7365746469676F75742031300100002ED4

                            //UnLock
                            //00000000000000140C01050000000C7365746469676F75742030310100007E84


                            //863540062368775
                            var byteCommand = HexStringToByteArray(command);
                            await stream.WriteAsync(byteCommand, 0, byteCommand.Length);

                            // Read and print response after sending command
                            byte[] commandResponseBuffer = new byte[100];
                            await stream.ReadAsync(commandResponseBuffer, 0, commandResponseBuffer.Length);
                            Console.WriteLine($"Response after sending command: {BitConverter.ToString(commandResponseBuffer)}");
                            
                            //save command transactions
                            var optionsBuilder = new DbContextOptionsBuilder<ListenerDbContext>();
                            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

                            var options = optionsBuilder.Options;

                            using (var context = new ListenerDbContext(options))
                            {
                                var commandTransaction = new CommandTransaction()
                                {
                                    SendingDate = DateTime.Now,
                                    Command = CommandHelper.GetCommandFromHex(command).ToString(),
                                    IMEI = imei,
                                    Status = "OK"
                                };

                                    
                                    await context.CommandTransactions.AddAsync(commandTransaction);
                                    await context.SaveChangesAsync();
                            }
                        // var dataReceived = DecodeTcpPacket(commandResponseBuffer);
                        //string jsonString = JsonConvert.SerializeObject(dataReceived, Formatting.Indented);
                        // Console.WriteLine("response parsed::::"+ jsonString);
                    }
                else
                {
                    //save command transactions
                    var optionsBuilder = new DbContextOptionsBuilder<ListenerDbContext>();
                    optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

                    var options = optionsBuilder.Options;

                    using (var context = new ListenerDbContext(options))
                    {
                        var commandTransaction = new CommandTransaction()
                        {
                            SendingDate = DateTime.Now,
                            Command = CommandHelper.GetCommandFromHex(command).ToString(),
                            IMEI = imei,
                            Status = "Error"
                        };


                        await context.CommandTransactions.AddAsync(commandTransaction);
                        await context.SaveChangesAsync();
                    }
                    Console.WriteLine("Falied to connect to server, please try again later");
                }
                    
                
                }
            catch (Exception ex)
            {

                Console.WriteLine("Failed to start stream of client");
            }
            
            
        }
        private TcpClient GetClientByImei(string imei)
        {
            var x = _connectedClients.FirstOrDefault(c => c.Imei == imei);
            return x.TcpClient;
        }
        static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length / 2;
            byte[] byteArray = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byteArray[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return byteArray;
        }
        

        static void CalcMemory()
        {
            Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            long totalBytesOfMemoryUsed = currentProcess.WorkingSet64;

            double totalMegabytes = (double)totalBytesOfMemoryUsed / (1024 * 1024);
            Console.WriteLine($"Total Memory Usage: {totalMegabytes} MB");

            //var n0 = System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
            //var n1 = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
            //var n2 = System.Diagnostics.Process.GetCurrentProcess().VirtualMemorySize64;

            //float f0 = ((float)n0) / (1024 * 1024);
            //float f1 = ((float)n1) / (1024 * 1024);
            //float f2 = ((float)n2) / (1024 * 1024);
            //Console.WriteLine("private = " + f0 + " MB");
            //Console.WriteLine("working = " + f1 + " MB");
            //Console.WriteLine("virtual = " + f2 + " MB");
        }

        static string GetIMEI(string IMEI)
        {
            string ans="";
            for (int i = 5; i < IMEI.Length; i+=2)
            {
                ans += IMEI[i];
            }
            return ans;
        }
    }
}
