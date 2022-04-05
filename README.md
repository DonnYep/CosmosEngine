[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/DonnYep/CosmosEngine/blob/main/LICENSE)
[![Issues:Welcome](https://img.shields.io/badge/Issues-welcome-blue.svg)](https://github.com/DonnYep/CosmosEngine/issues)
# CosmosEngine

 CosmosEngine是一款轻量级的.NET服务器。支持网络多通道、服务器间RPC大数据流传输通讯、分布式部署。此项目长期维护更新，LTS。

## 环境

- .NetCore 3.1 及以上。

## RPC
- RPC功能模块。客户端只需要接口即可生成动态代理对象，无需手动实现。服务器只需在被调用的方法上标记[RPCMemberAttribute]特性，就能实现被客户端RPC调用。RPC底层使用TCP协议，无需担心RPC方法返回的数据量，大数据会自动转换为流式传输，接收端只需要使用async/await方法等待数据结果，若数据解析错误，则抛出异常。

## Library link

- CosmosFramework：https://github.com/DonnYep/CosmosFramework

- KCP C:https://github.com/skywind3000/kcp
    
- KCP CSharp:https://github.com/vis2k/kcp2k
    
- TCP：https://github.com/vis2k/Telepathy

- PureMVC：https://github.com/DonnYep/PureMVC

- Mirror:https://github.com/vis2k/Mirror
