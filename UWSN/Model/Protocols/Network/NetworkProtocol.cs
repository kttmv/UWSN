namespace UWSN.Model.Protocols.Network;

public abstract class NetworkProtocol : ProtocolBase
{
    public abstract void ReceiveFrame(Frame frame);

    public abstract void SendFrame(Frame frame);

    public abstract void StopAllAction();

    public abstract void SendDeathWarning();

    public abstract void SendCollectedData(CollectedData data);
}
