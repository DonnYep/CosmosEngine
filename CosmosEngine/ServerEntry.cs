using Cosmos;

namespace CosmosEngine
{
    public class ServerEntry : CosmosEntry
    {
        public static IServiceManager ServiceManager{ get { return GetModule<IServiceManager>(); } }
    }
}
