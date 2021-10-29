using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    internal enum RPCDataPackageType:byte
    {
        /// <summary>
        /// 整包；
        /// </summary>
        Fullpackage=0x0,
        /// <summary>
        /// 分包；
        /// </summary>
        Subpackage=0x1
    }
}
