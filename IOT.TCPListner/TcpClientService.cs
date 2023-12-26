using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Sockets;
using Teltonika.Codec;
using Teltonika.Codec.Model;

namespace IOT.TCPListner
{
    public class TcpClientService
    {
        readonly TcpClient _client;

        public TcpClientService(TcpClient client)
        {
            _client = client;
        }

        public async Task Run()
        {

            using (_client)
            using (var stream = _client.GetStream())
            {
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
                    byte[] response;
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

                        // Print the JSON string
                        Console.WriteLine(jsonString);
                        Console.WriteLine("---------------------------------------------");

                        response = BitConverter.GetBytes(BytesSwapper.Swap(decodedData.AvlData.DataCount));

                        await stream.WriteAsync(response, 0, response.Length);

                        avlDataLength = null;
                        fullPacket.Clear();
                        CalcMemory();

                        Console.WriteLine(string.Format("{0} - responded [{1}]", DateTime.Now, string.Join("", response.Select(x => x.ToString("X2")).ToArray())));
                        SendCommand(stream, string.Join("", response.Select(x => x.ToString("X2")).ToArray()));
                        Console.ReadKey();
                    }

                }
            }
        }

        private static TcpDataPacket DecodeTcpPacket(byte[] request)
        {
            var reader = new ReverseBinaryReader(new MemoryStream(request));
            var decoder = new DataDecoder(reader);

            return decoder.DecodeTcpData();
        }
        private static async Task SendCommand(NetworkStream stream , string str)
        {

            //  Send command
            // Lock
            //00000000000000140C01050000000C7365746469676F75742031300100002ED4

            //UnLock
            //00000000000000140C01050000000C7365746469676F75742030310100007E84
            var byteCommand = HexStringToByteArray("00000000000000140C01050000000C7365746469676F75742031300100002ED4");
            await stream.WriteAsync(byteCommand, 0, byteCommand.Length);

            // Read and print response after sending command
            byte[] commandResponseBuffer = new byte[100];
            await stream.ReadAsync(commandResponseBuffer, 0, commandResponseBuffer.Length);
            Console.WriteLine($"Response after sending command: {BitConverter.ToString(commandResponseBuffer)}");
            
            Console.WriteLine(DecodeTcpPacket(commandResponseBuffer).AvlData);
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
        static byte HexStringToByte(string hex)
        {
            if (hex.Length != 2)
            {
                throw new ArgumentException("Hex string must represent a single byte (two characters).", nameof(hex));
            }

            return Convert.ToByte(hex, 16);
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
