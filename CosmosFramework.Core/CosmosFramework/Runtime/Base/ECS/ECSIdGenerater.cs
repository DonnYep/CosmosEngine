using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace Cosmos.ECS
{
    public class ECSIdGenerater:Singleton<ECSIdGenerater>
    {

        //不检测溢出
        uint instanceIndex = 0;
        public long GenerateInstanceId()
        {
            instanceIndex++;
            return instanceIndex;
        }
    }
}
