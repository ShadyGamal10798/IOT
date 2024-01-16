using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.TCPListner.Data.Entities
{
    public class CommandTransaction
    {
        public int Id { get; set; }
        public DateTime SendingDate { get; set; }
        public string Command { get; set; }
        public string IMEI { get; set; }
        public string Status { get; set; }
    }
}
