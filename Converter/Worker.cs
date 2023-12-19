using System.Globalization;

namespace Converter
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string hexString = "000F333536333037303432343431303133"; // Ignore the leading "00"

            long decimalValue = 0;
            int baseValue = 16; // Base 16 for hexadecimal

            // Skip the leading "00"
            for (int i = 2; i < hexString.Length; i += 2)
            {
                // Extract a pair of hex digits
                string twoDigits = hexString.Substring(i, 2);

                // Convert the hex digits to an integer
                int hexDigitValue = int.Parse(twoDigits, NumberStyles.HexNumber);

                // Add the value of the pair to the total, with appropriate base placement
                decimalValue += (long)hexDigitValue * (long)Math.Pow(baseValue, hexString.Length - 1 - i);
            }

            Console.WriteLine(decimalValue); // This will print 356307042441013
        }
    }
}