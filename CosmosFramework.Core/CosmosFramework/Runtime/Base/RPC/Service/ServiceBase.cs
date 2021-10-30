using Cosmos.RPC.Client;
using System.Threading.Tasks;
namespace Cosmos.RPC
{
    public class ServiceBase
    {
        protected RPCClient rpcClient;
       protected RpcTask<T> CreateResultfulRpcTask<T>(string typeFullName, string methodName, params object[] parameters)
        {
            var reqRpcData = RPCUtility.Serialization.EncodeRpcData(typeFullName, methodName, typeof(T), parameters);
            rpcClient.SendMessage(reqRpcData.RpcDataBytes);
            return new RpcTask<T>(reqRpcData.RpcDataId);
        }
         protected RpcTask CreateVoidRpcTask(string typeFullName, string methodName, params object[] parameters)
        {
            var reqRpcData = RPCUtility.Serialization.EncodeRpcData(typeFullName, methodName, typeof(void), parameters);
            rpcClient.SendMessage(reqRpcData.RpcDataBytes);
            return new RpcTask(reqRpcData.RpcDataId);
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
    }
}
