namespace UWSN.Model
{
    public class Frame
    {
        public Type FrameType { get; set; }
        public int IdSend { get; set; }
        public int IdReceive { get; set; }
        public DateTime TimeSend { get; set; }

        public enum Type
        {
            RegularFrame,
            Ack,
            Data
        }
    }
}