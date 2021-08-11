using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    public class TcpNetMessage : INetworkMessage
    {
        public long Conv { get; set; }
        public UdpHeader HeaderCode{ get; set; }

        public byte[] ServiceData { get; private set; }

        public bool DecodeMessage(byte[] buffer)
        {
            return false;
        }
        public byte[] EncodeMessage()
        {
            return null;
        }

        public byte[] GetBuffer()
        {
            return EncodeMessage();
        }
    }
}
