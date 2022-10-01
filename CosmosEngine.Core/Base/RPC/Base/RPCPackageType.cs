namespace Cosmos.RPC.Core
{
    internal enum RPCPackageType:byte
    {
        /// <summary>
        /// 整包；
        /// </summary>
        Fullpackage=0x0,
        /// <summary>
        /// 片段；
        /// </summary>
        Segment=0x1
    }
}
