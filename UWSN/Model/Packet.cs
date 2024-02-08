using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public class Packet
    {
        public bool IsDamaged { get; set; } = false;

        public Packet() 
        { 

        }

        public Packet(Packet packet)
        {
            IsDamaged = packet.IsDamaged;
            //...
        }
    }
}
