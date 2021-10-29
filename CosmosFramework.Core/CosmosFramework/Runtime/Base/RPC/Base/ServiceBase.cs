using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Cosmos.RPC
{
    internal class ServiceBase
    {
        protected RPCClient rpcClient;
        protected RpcTask<T> CreateResultfulRpcTask<T>(string typeFullName, string methodName, params object[] parameters)
        {
            Console.WriteLine(" 进入 CreateResultfulRpcTask");
            var reqRpcData = RPCUtility.Serialization.EncodeRpcData(typeFullName, methodName, typeof(T), parameters);
            var bin = RPCUtility.Serialization.Serialize(reqRpcData);
            rpcClient.SendMessage(bin);
            return new RpcTask<T>(reqRpcData);
        }
        protected RpcTask CreateVoidRpcTask(string typeFullName, string methodName, params object[] parameters)
        {
            var reqRpcData = RPCUtility.Serialization.EncodeRpcData(typeFullName, methodName, typeof(void), parameters);
            var bin = RPCUtility.Serialization.Serialize(reqRpcData);
            rpcClient.SendMessage(bin);
            return new RpcTask(reqRpcData);
        }
        protected async Task<T> AwaitResultfulRpcTask<T>(RpcTask<T> rpcTask)
            where T:class
        {
            return await rpcTask;
        }
        protected async Task AwaitVoidRpcTask(RpcTask rpcTask)
        {
            await rpcTask;
        }
        Task<string> RPCCallString()
        {
            var rpcTask= CreateResultfulRpcTask<string>("ServiceBase", "RPCCallString");
            return AwaitResultfulRpcTask(rpcTask);
        }
    }
}
