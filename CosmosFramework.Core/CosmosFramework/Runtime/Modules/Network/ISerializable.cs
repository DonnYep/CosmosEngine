using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 序列化接口；
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <returns>二进制数组</returns>
        byte[] Serialize();
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="data">需要反序列化的二进制数组</param>
        void Deserialize(byte[] data);
    }
}
