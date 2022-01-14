using System.Collections.Generic;

namespace CosmosEngine
{
    public class MultiplayConnection
    {
        public int Conv { get; set; }
        public List<byte[]> TransportData { get; set; }
        public MultiplayConnection()
        {
            TransportData = new List<byte[]>();
        }
    }
}
