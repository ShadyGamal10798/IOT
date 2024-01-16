using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.TCPListner.Data.Enums
{
    public enum Command
    {
        Lock,
        Unlock,
        OpenallDoors,
        CloseAllDoors,
        OpenTrunk,
        TurnLights,
        WindowClose,
        Unknown // for undefined commands
    }

    public static class CommandHelper
    {
        private static readonly Dictionary<string, Command> hexToCommandMap = new Dictionary<string, Command>
    {
        { "00000000000000140C01050000000C7365746469676F75742031300100002ED4", Command.Lock },
        { "00000000000000140C01050000000C7365746469676F75742030310100007E84", Command.Unlock },
        { "000000000000001B0C0105000000136C7663616E6F70656E616C6C646F6F72730D0A0100005D32", Command.OpenallDoors },
        { "000000000000001C0C0105000000146C7663616E636C6F7365616C6C646F6F72730D0A010000AC69", Command.CloseAllDoors },
        { "00000000000000180C0105000000106C7663616E6F70656E7472756E6B0D0A010000C663", Command.OpenTrunk },
        { "000000000000001C0C0105000000146C7663616E7475726E696E676C69676874730D0A010000975F", Command.TurnLights },
        { "000000000000001d0c0105000000156c7663616e77696e646f7773636c6f73653a310d0a0100006261", Command.WindowClose }
    };

        public static Command GetCommandFromHex(string hexString)
        {
            if (hexToCommandMap.TryGetValue(hexString, out Command command))
            {
                return command;
            }

            return Command.Unknown;
        }
    }

}
