using System;
namespace Cosmos.RPC.Core
{
    internal struct RPCDataSegment
    {
        public long RspDataId;
        public int RspDataLength;
        public byte[] Segment;
        public RPCDataSegment(long rspDataId, int rspDataLength, byte[] segment)
        {
            RspDataId = rspDataId;
            RspDataLength = rspDataLength;
            Segment = segment;
        }
        public static byte[] Serialize(RPCDataSegment segment)
        {
            var rspDataIdBytes = BitConverter.GetBytes(segment.RspDataId);
            var rspDataLengthBytes = BitConverter.GetBytes(segment.RspDataLength);
            var seg = segment.Segment;
            var segBytes = new byte[seg.Length + 12];
            Array.Copy(rspDataIdBytes, 0, segBytes, 0, 8);
            Array.Copy(rspDataLengthBytes, 0, segBytes, 8, 4);
            Array.Copy(seg, 0, segBytes, 12, seg.Length);
            return segBytes;
        }
        public static RPCDataSegment Deserialize(byte[] srcData)
        {
            var rspDataIdBytes = new byte[8];
            var rspDataLengthBytes = new byte[4];
            var seg = new byte[srcData.Length - 12];

            Array.Copy(srcData, 0, rspDataIdBytes, 0, 8);
            Array.Copy(srcData, 8, rspDataLengthBytes, 0, 4);
            Array.Copy(srcData, 12, seg, 0, seg.Length);

            var rspDataId = BitConverter.ToInt64(rspDataIdBytes);
            var rspDataLength = BitConverter.ToInt32(rspDataLengthBytes);
            return new RPCDataSegment(rspDataId, rspDataLength, seg);
        }
    }
}
