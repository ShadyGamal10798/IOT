using Newtonsoft.Json;
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
                    Console.WriteLine("Imei\n");
                    Console.WriteLine(string.Format("{0} - received [{1}]", DateTime.Now, string.Join("", bytes.Take(length).Select(x => x.ToString("X2")).ToArray())));

                    

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
            // Step 1: Send 01
            //var responseAck = new byte[] { 0x01 };
            //await stream.WriteAsync(responseAck, 0, responseAck.Length);

            //// Read and print response after sending 01
            //byte[] ackResponseBuffer = new byte[1];
            //await stream.ReadAsync(ackResponseBuffer, 0, ackResponseBuffer.Length);
            //Console.WriteLine($"Response after sending 01: {BitConverter.ToString(ackResponseBuffer)}");

            //// Step 2: Send 00000009
            //var countCommand = HexStringToByteArray(str);
            //await stream.WriteAsync(countCommand, 0, countCommand.Length);

            //// Read and print response after sending 00000009
            //byte[] countResponseBuffer = new byte[100];
            //await stream.ReadAsync(countResponseBuffer, 0, countResponseBuffer.Length);
            //Console.WriteLine($"Response after sending count: {BitConverter.ToString(countResponseBuffer)}");

            // Step 3: Send command
            var byteCommand = HexStringToByteArray("00000000000000180C0105000000107365746469676F757420313020322030010000DAF8");
            await stream.WriteAsync(byteCommand, 0, byteCommand.Length);

            // Read and print response after sending command
            byte[] commandResponseBuffer = new byte[100];
            await stream.ReadAsync(commandResponseBuffer, 0, commandResponseBuffer.Length);
            Console.WriteLine($"Response after sending command: {BitConverter.ToString(commandResponseBuffer)}");
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
    }
}
