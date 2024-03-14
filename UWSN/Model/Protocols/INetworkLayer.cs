namespace UWSN.Model.Network
{
    public abstract class NetworkProtocol: ProtocolBase
    {
        public abstract void ReceiveFrame(Frame frame);

        public abstract void SendFrame(Frame frame);
    }
}
