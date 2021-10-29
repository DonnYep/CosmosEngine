# CosmosEngine

 CosmosEngine是一款轻量级的服务器框架，提供高速可靠UDP传输协议。网络模块支持多服务器交互，可扩展成分布式服务。默认使用KCP进行传输，服务稳定性能高效。

## 环境

- .NetCore 3.1 及以上。

## 功能简介

- **Event**：标准事件中心。提供的事件监听、派发皆为线程安全类型。

- **Network**：网络模块。网络模块提供了几种可靠高速的UDP协议,默认使用KCP。网络以通道(Channel)形式区分各个连接，支持多种网络类型同时连接。可同时实现作为客户端(Client)以及服务器(Server),可自定义扩展成分布式。INetworkChannel支持Async/await语法。

- **Config**：配置模块。提供服务器初始化时加载解析配置文件服务。Runtime时可由其他模块进行存取修改。

- **ReferencePool**：全局线程安全引用池。通过对象引用池可以减少服务器运行时产生的GC，，实现了IReferenc接口的对象可通过引用池生成、回收以达到重复使用的效果。

- **FSM**：有限状态机。服务端的有限状态机可计算客户端对象在服务端的实时状态，令逻辑在服务器执行，减少客户端作弊。

- **Utility**：提供了反射、算法、断言、转换、Debug富文本、IO、加密、Json、MessagePack、Time、Text等常用工具。

- **Singleton**：单例基类。提供了线程安全、非线程安全、MONO单例基类。

- **DataStructure**：常用数据结构。链表、双向链表、二叉树、LRU、线程锁等数据结构。

- **EventCore** ：轻量级事件模块，可自定义监听的数据类型；

- **RPC** ：RPC功能模块。客户端只需要接口即可生成动态代理对象，无需手动实现。服务器只需在被调用的方法上标记[RPCMemberAttribute]特性，就能实现被客户端RPC调用。若RPC方法返回的数据量十分庞大，则自动转换为流式传输，客户端只需要使用async/await方法等待数据结果。


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

- 自定义模块请参考原生模块的写法：
    - 1、继承自Module，并打上ModuleAttribute的特性。
    - 2、自定义一个与模块类名相同的接口，此接口派生自IModuleManager。
    - 3、在此接口写入需要开放给外部调用的方法属性等。

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

- 数据结构中，提供了池的的底层对象“Pool”，引用池以及其他自定义实现的池皆为“Pool”作为底层实现；

## 其他

- KCP地址：https://github.com/skywind3000/kcp

- MVVM的纯C#版本：https://github.com/DonnYep/CosmosMVVM

- Cosmos Unity客户端框架：https://github.com/DonnYep/CosmosFramework