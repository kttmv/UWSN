using System.Numerics;
using static UWSN.Model.Protocols.NetworkProtocol;

namespace UWSN.Model
{
    public class Frame
    {
        public const double FRAME_SIZE_IN_BITS = 256.0;

        public enum FrameType
        {
            RegularFrame,
            Ack,
            Data,
            Hello,
            Warning
        }

        public required FrameType Type { get; set; }
        public required int SenderId { get; set; }
        public required Vector3 SenderPosition { get; set; }
        public required int ReceiverId { get; set; }
        public required DateTime TimeSend { get; set; }
        public required bool AckIsNeeded { get; set; }
        public required List<Neighbour>? NeighboursData { get; set; }
        public required double BatteryLeft { get; set; }
        public required List<int>? DeadSensors { get; set; }
    }
}