using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWSN.Model.Network
{
    public interface INetworkLayer
    {
        public void ReceiveFrame(Frame frame);

        public void SendFrame(Frame frame);
    }
}