using System.Numerics;

namespace UWSN.Model
{
    public class Frame
    {
        public enum FrameType
        {
            RegularFrame,
            Ack,
            Data,
            Hello
        }

        public required FrameType Type { get; set; }
        public required int SenderId { get; set; }
        public required Vector3 SenderPosition { get; set; }
        public required int ReceiverId { get; set; }
        public required DateTime TimeSend { get; set; }
        public required bool AckIsNeeded { get; set; }
    }
}