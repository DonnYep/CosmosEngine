[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](https://github.com/DonnYep/CosmosEngine/blob/main/LICENSE)
# CosmosEngine

 CosmosEngine是一款轻量级的.NET服务器。支持网络多通道、服务器间RPC大数据流传输通讯、分布式部署。此项目长期维护更新，LTS。

## 环境

- .NetCore 3.1 及以上。

## 功能简介

- **Event**：标准事件中心。提供的事件监听、派发皆为线程安全类型。

- **Network**：网络模块。提供了多种高速可靠的UDP协议，如RUDP、SUDP、KCP、TCP等，默认使用KCP协议。网络以通道(Channel)形式区分各个连接，支持多种网络类型同时连接。可实现(Client-Server)模式。支持async/await语法；

- **Config**：配置模块。提供服务器初始化时加载解析配置文件服务。Runtime时可由其他模块进行存取修改。

- **ReferencePool**：全局线程安全引用池。通过对象引用池可以减少服务器运行时产生的GC，，实现了IReferenc接口的对象可通过引用池生成、回收以达到重复使用的效果。

- **FSM**：有限状态机。完全抽象的有限状态机，可针对不同类型的拥有者做状态机实现。

- **Utility**：提供了反射、算法、断言、转换、Debug富文本、IO、加密、Json、MessagePack、Time、Text等常用工具。

- **Singleton**：单例基类。提供了线程安全、非线程安全。

- **DataStructure**：常用数据结构。链表、双向链表、二叉树、四叉树、LRU、线程锁、双向字典等数据结构。

- **EventCore** ：轻量级事件模块，可自定义监听的数据类型；

- **RPC** ：RPC功能模块。客户端只需要接口即可生成动态代理对象，无需手动实现。服务器只需在被调用的方法上标记[RPCMemberAttribute]特性，就能实现被客户端RPC调用。RPC底层使用TCP协议，无需担心RPC方法返回的数据量，大数据会自动转换为流式传输，接收端只需要使用async/await方法等待数据结果，若数据解析错误，则抛出异常。


## 内置架构 PureMVC

- 基于原始PureMVC改进的更适于理解的架构。
    框架提供了基于特性更加简洁的注册方式：
    - 1、MVCCommandAttribute，对应Command，即C层；
    - 2、MVCMediatorAttribute，对应Mediator，即V层；
    - 3、MVCProxyAttribute，对应Proxy，即M层；
    
- MVC自动注册只需在入口调用MVC.RegisterAttributedMVC()方法即可。

- 派生的代理类需要覆写构造函数，并传入NAME参数。

- 需要注意，MVC.RegisterAttributedMVC()方法需要传入对应的程序集。支持多程序集反射。

## 注意事项

- 内置生命周期适用于原生模块与自定义模块。若实现了自定义的扩展Module，则原生Module享有完全相同的生命周期以及调用级别，生命周期优先级依次为：
    - OnInitialization
    - OnActive
    - OnPreparatory
    - OnFixRefresh
    - OnRefresh
    - OnLateRefresh
    - OnDeactive
    - OnTermination

- 原生模块调用可通过CosmosEntry调用。

- 部分带有Helper的模块可由使用者进行自定义实现，也可使用提供的Default对象；

## Library link

- CosmosFramework：https://github.com/DonnYep/CosmosFramework

- KCP C:https://github.com/skywind3000/kcp
    
- KCP CSharp:https://github.com/vis2k/kcp2k
    
- TCP：https://github.com/vis2k/Telepathy

- PureMVC：https://github.com/DonnYep/PureMVC

- Mirror:https://github.com/vis2k/Mirror
