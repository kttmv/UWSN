using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model
{
    public interface INetworkLayer
    {
        public void ReceiveFrame(Frame frame, Sensor sensor);

        public void SendFrame(Frame frame, Sensor sensor); 
    }
}
