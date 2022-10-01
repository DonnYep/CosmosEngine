using System;
namespace Cosmos.RPC
{
    internal interface IRpcTask
    {
        long TaskId { get; }
        bool IsCompleted { get; }
        void RspRpcData(byte[] returnDataBytes, Type returnDataType);
        void RspRpcSegment(int rspFullLength, byte[] segment);
    }
}
