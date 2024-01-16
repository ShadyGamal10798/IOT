using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOT.TCPListner.Data.Entities
{
    public class AVLData
    {
        public int Id { get; set; }
        public string IMEI { get; set; }
        public DateTime ListeningDate { get; set; }
        public int CodecID { get; set; }
        public int DataCount { get; set; }
        public string Longitude { get; set; }
        public string Latitiude { get; set; }
        public float Altitude { get; set; }
        public int Satellites { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }

        public string OriginalJson { get; set; }
    }
}
