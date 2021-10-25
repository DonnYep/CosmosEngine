using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    public interface IRpcTask
    {
        long TaskId { get;  }
        bool IsCompleted { get; }
        /// <summary>
        /// Response rpc data;
        /// </summary>
        /// <param name="rpcData"></param>
        void RspRpcData(RPCData rpcData);
    }
}
