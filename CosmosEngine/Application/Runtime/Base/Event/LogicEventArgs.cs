using Cosmos;
namespace CosmosEngine
{
    public class LogicEventArgs : GameEventArgs
    {
        public object Data { get; set; }
        public LogicEventArgs() { }
        public LogicEventArgs(object data)
        {
            Data = data;
        }
        public override void Release()
        {
            Data = null;
        }
    }
}
