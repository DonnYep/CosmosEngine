namespace Cosmos.RPC.Client
{
    internal class DynamicClientAssemblyHolder
    {
        public const string ModuleName = "Cosmos.Client.DynamicClient";

        readonly static DynamicAssembly assembly;
        public static DynamicAssembly Assembly { get { return assembly; } }

        readonly static DynamicTypeDataProxy typeDataProxy;
        public static DynamicTypeDataProxy TypeDataProxy { get { return typeDataProxy; } }

        static DynamicClientAssemblyHolder()
        {
            assembly = new DynamicAssembly(ModuleName);
            typeDataProxy = new DynamicTypeDataProxy();
        }
    }
}
