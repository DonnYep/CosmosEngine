using Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// ����ͨѶ������ģ�ͣ�
/// </summary>
namespace CosmosEngine
{
    [Serializable]
    public class OperationData : IDisposable
    {
        public byte OperationCode { get; set; }
        public ushort SubOperationCode { get; set; }
        public object DataContract { get; set; }
        public short ReturnCode { get; set; }
        public OperationData() { }
        public OperationData(byte operationCode)
        {
            OperationCode = operationCode;
        }
        public void Dispose()
        {
            OperationCode = 0;
            DataContract = null;
            ReturnCode = 0;
            SubOperationCode = 0;
        }
    }
}