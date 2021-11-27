namespace Cosmos.RPC
{
    public class RPCConstants
    {
        /// <summary>
        /// TCP 单包最大尺寸 1<<18;
        /// 262144===256KB
        /// </summary>
        public const int TcpMaxMessageSize = 1 << 18;
        /// <summary>
        /// 一个rpc包最大的容量；240KB+
        /// </summary>
        public const int MaxRpcPackSize = 244800;
        /// <summary>
        /// RPC数据子包发送的毫秒间隔；
        /// 当用到这个参数时，表示RPC数据包超过了MaxRpcPackSize ，并被拆分成多个子包发送；
        /// </summary>
        public const int RpcSubpackageSendMSInterval = 100;
    }
}
