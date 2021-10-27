using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    internal interface IRpcTask
    {
        long TaskId { get;  }
        bool IsCompleted { get; }
        /// <summary>
        /// Response rpc data;
        /// </summary>
        void RspRpcData(RPCData rpcData);
        void RspRpcSegment(int rspFullLength, byte[] segment);
    }
}
