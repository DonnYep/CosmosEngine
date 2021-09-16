using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosEngine
{
    public class EngineEntry:CosmosEntry
    {
        public static IMultiplayManager  MultiplayManager{ get { return GameManager.GetModule<IMultiplayManager>(); } }
    }
}
