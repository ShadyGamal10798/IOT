// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

string colorCode = Environment.GetCommandLineArgs()[1];
string message = args.Length > 1 ? args[1] : "Default Message";
Console.WriteLine("Received colorCode: '" + colorCode + "'");
Console.WriteLine(Environment.GetCommandLineArgs()[1]);
if (colorCode.Equals("blue", StringComparison.OrdinalIgnoreCase))
{
    Console.ForegroundColor = ConsoleColor.Blue;
}
else if (colorCode.Equals("green", StringComparison.OrdinalIgnoreCase))
{
    Console.ForegroundColor = ConsoleColor.Green;
}
else
{
    Console.WriteLine("Received colorCode: '" + colorCode + "'");
    Console.ForegroundColor = ConsoleColor.White;
}


Console.WriteLine(message);
Console.WriteLine("---------------------------------------------");
Console.ReadLine(); // Wait for user input to close
