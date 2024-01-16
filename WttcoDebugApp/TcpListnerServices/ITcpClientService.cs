using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WttcoDebugApp.TcpListnerServices
{
    public interface ITcpClientService
    {
        Task SendCommandToClient(string imei, string command);
    }
}
