using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosEngine
{
    public class EngineEntry:CosmosEntry
    {
        public static IServiceManager ServiceManager { get { return GetModule<IServiceManager>(); } }
    }
}
