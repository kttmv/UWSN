﻿namespace UWSN.Model.DataLink
{
    public abstract class DataLinkProtocol: ProtocolBase
    {
        public abstract void ReceiveFrame(Frame frame);

        public abstract void SendFrame(Frame frame);
    }
}
