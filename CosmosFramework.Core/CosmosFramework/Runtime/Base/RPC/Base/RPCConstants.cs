namespace Cosmos.RPC
{
    public class RPCConstants
    {
        /// <summary>
        /// 一个RPC包最大的容量，约80KB；
        /// var snddata= new byte[81920];
        /// </summary>
        public const int MaxRpcPackSize = 81920;
        /// <summary>
        /// RPC数据子包发送的毫秒间隔；
        /// 当用到这个参数时，表示RPC数据包超过了MaxRpcPackSize ，并被拆分成多个子包发送；
        /// </summary>
        public const int RpcSubpackageSendMSInterval = 50;

    }
}
