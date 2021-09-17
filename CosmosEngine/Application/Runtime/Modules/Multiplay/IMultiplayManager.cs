using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using Cosmos.Network;

namespace CosmosEngine
{
    public interface IMultiplayManager:IModuleManager
    {
        void SetNetworkChannel(INetworkChannel channel);
    }
}
