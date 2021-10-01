using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosEngine
{
    public interface IServiceManager:IModuleManager
    {
        event Action<int> OnConnected;
        event Action<int> OnDisconnected;
        event Action<int, byte[]> OnReceiveData;
        void SendMessage(byte[] data, int conv);
    }
}
