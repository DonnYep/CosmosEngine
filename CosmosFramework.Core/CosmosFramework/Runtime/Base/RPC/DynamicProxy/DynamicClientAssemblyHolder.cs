namespace Cosmos.RPC.Client
{
    internal class DynamicClientAssemblyHolder
    {
        public const string ModuleName = "Cosmos.Client.DynamicClient";

        readonly static DynamicAssembly assembly;
        public static DynamicAssembly Assembly { get { return assembly; } }

        static DynamicClientAssemblyHolder()
        {
            assembly = new DynamicAssembly(ModuleName);
        }
    }
}
