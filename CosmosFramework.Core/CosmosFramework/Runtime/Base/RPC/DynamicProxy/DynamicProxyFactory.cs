using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using Cosmos.RPC.Client;
using Cosmos.RPC;
using System.Threading.Tasks;

namespace Cosmos.RPC
{
    //================================================
    /*
    *1、泛型约束的类型必须是派生自IService<T>的接口有类型；
    *
    *2、接口的返回方法都必须为Task类型
    *
    */
    //================================================
    /// <summary>
    /// https://blog.csdn.net/xiaouncle/article/details/52776007
    /// </summary>
    public class DynamicProxyFactory
    {
        public static T CreateProxy<T>(RPCClient client)
    where T : IService<T>
        {
            var type = typeof(T);
            var interfaceType = type.GetTypeInfo();
            if (!interfaceType.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + interfaceType.Name);

            var interfaceMethods = interfaceType.GetMethods();

            var typeBuilder = DynamicClientAssemblyHolder.Assembly.DefineType(typeof(T).Name, TypeAttributes.Public, typeof(object));

            //对象实现接口
            typeBuilder.AddInterfaceImplementation(typeof(T));

            //对象的构造方法 pulic ClassName(RPCClient client){}
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis | CallingConventions.Standard, new Type[] { typeof(RPCClient) });

            //对象定义一个字段；
            var rpcClientField = typeBuilder.DefineField("rpcClient", typeof(RPCClient), FieldAttributes.Private);

            var printPRCClientMethod = typeof(RPCClient).GetMethod("PrintPRCClient", BindingFlags.Instance | BindingFlags.Public);

            var appendMethod = typeof(RPCClient).GetMethod("Append", BindingFlags.Instance | BindingFlags.Public);

            var sendMessageMethod = typeof(RPCClient).GetMethod("SendMessage", BindingFlags.Instance | BindingFlags.Public);

            var addMethod = typeof(RPCClient).GetMethod("Add", BindingFlags.Static | BindingFlags.Public);



            var serializeToRpcDataMethod = typeof(RPCUtility.Serialization).GetMethod("SerializeToRpcData", BindingFlags.Static | BindingFlags.Public);

            var serializeBytesMethod = typeof(RPCUtility.Serialization).GetMethod("SerializeBytes", BindingFlags.Static | BindingFlags.Public);

            var methodBuilderLst = new List<MethodBuilder>();

            var mLength = interfaceMethods.Length;
            /*
             * namespace Cosmos.RPC.RPCClient
             * {
             *      RPCClient rpcClient;
             *      public ClassName(RPCClient client)
             *      {
             *          rpcClient=client
             *      }
             * }
             */
            var constructorBody = constructorBuilder.GetILGenerator();
            constructorBody.Emit(OpCodes.Ldarg_0);
            constructorBody.Emit(OpCodes.Ldarg_1);
            constructorBody.Emit(OpCodes.Stfld, rpcClientField);
            constructorBody.Emit(OpCodes.Ret);

            //实现接口的方法；
            for (int i = 0; i < mLength; i++)
            {
                var methodInfo = interfaceMethods[i];
                //if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                //{
                //    throw new Exception("Client Proxy ReturnType only allows Task Type:" + methodInfo.Name);
                //}
                var paramTypes = methodInfo.GetParameters().Select(t => t.ParameterType).ToArray();
                var m = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Virtual | MethodAttributes.Public, methodInfo.ReturnType, paramTypes);
                methodBuilderLst.Add(m);
                var il = m.GetILGenerator();


                LocalBuilder local_PrintPRCClient = il.DeclareLocal(typeof(string));

                LocalBuilder local_AppendStr = il.DeclareLocal(typeof(string));

                LocalBuilder local_Add = il.DeclareLocal(typeof(int));


                //LocalBuilder local_retrunType = il.DeclareLocal(methodInfo.ReturnType);

                //LocalBuilder local_reqRpcData = il.DeclareLocal(typeof(RPCData));

                //LocalBuilder local_reqRpcDataBytes = il.DeclareLocal(typeof(byte[]));


                //返回值Task<T>中，T的类型；
                //var retrunParameterTypes = methodInfo.ReturnParameter.ParameterType.GetTypeInfo().GenericTypeArguments;

                //if (retrunParameterTypes.Length > 0)
                //    il.Emit(OpCodes.Ldarg, retrunParameterTypes[0]);
                //else
                //    il.Emit(OpCodes.Ldarg, typeof(void));

                //实现方法的形参
                //int paramIndex = 1;
                //for (int j = 0; j < paramTypes.Length; j++)
                //{
                //    il.Emit(OpCodes.Ldarg_S, paramIndex);
                //    paramIndex++;
                //}


                //rpcClient.Append(type.FullName,methodInfo.Name);

                //il.Emit(OpCodes.Ldarg_0);
                //il.Emit(OpCodes.Ldfld, rpcClientField);

                //加载this指针
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, rpcClientField);
                il.Emit(OpCodes.Ldstr, type.FullName);
                il.Emit(OpCodes.Ldstr, methodInfo.Name);
                il.Emit(OpCodes.Callvirt, appendMethod);
                il.Emit(OpCodes.Stloc, local_AppendStr);

                il.Emit(OpCodes.Ldc_I4, 45);
                il.Emit(OpCodes.Ldc_I4, 15);
                il.Emit(OpCodes.Call, addMethod);
                il.Emit(OpCodes.Stloc, local_Add);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, rpcClientField);
                il.Emit(OpCodes.Ldloc, local_AppendStr);
                il.Emit(OpCodes.Ldloc, local_Add);
                il.Emit(OpCodes.Callvirt, printPRCClientMethod);
                il.Emit(OpCodes.Stloc, local_PrintPRCClient);
                il.Emit(OpCodes.Ldloc, local_PrintPRCClient);
                il.Emit(OpCodes.Ret);

                ////rpcClient.printPRCClient();
                //il.Emit(OpCodes.Callvirt, printPRCClientMethod);
                //il.Emit(OpCodes.Ret);
            }
            var newObject = (T)Activator.CreateInstance(typeBuilder.CreateType(), client);
            return newObject;
        }

        public static T CreateProxy<T>(RPCClient client, bool loc)
where T : IService<T>
        {
            var type = typeof(T);
            var interfaceType = type.GetTypeInfo();
            if (!interfaceType.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + interfaceType.Name);

            var interfaceMethods = interfaceType.GetMethods();

            var typeBuilder = DynamicClientAssemblyHolder.Assembly.DefineType(typeof(T).Name, TypeAttributes.Public, typeof(object));

            //对象实现接口
            typeBuilder.AddInterfaceImplementation(typeof(T));

            //对象的构造方法 pulic ClassName(RPCClient client){}
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis | CallingConventions.Standard, new Type[] { typeof(RPCClient) });

            //对象定义一个字段；
            var rpcClientField = typeBuilder.DefineField("rpcClient", typeof(RPCClient), FieldAttributes.Private);

            var printPRCClientMethod = typeof(RPCClient).GetMethod("PrintPRCClient", BindingFlags.Instance | BindingFlags.Public);

            var appendMethod = typeof(RPCClient).GetMethod("Append", BindingFlags.Instance | BindingFlags.Public);

            var sendMessageMethod = typeof(RPCClient).GetMethod("SendMessage", BindingFlags.Instance | BindingFlags.Public);

            var addMethod = typeof(RPCClient).GetMethod("Add", BindingFlags.Static | BindingFlags.Public);


            var tempSendMessageMethod = typeof(RPCClient).GetMethod("TempSendMessage", BindingFlags.Instance | BindingFlags.Public);



            var serializeToRpcDataMethod = typeof(RPCUtility.Serialization).GetMethod("SerializeToRpcData", BindingFlags.Static | BindingFlags.Public);

            var serializeBytesMethod = typeof(RPCUtility.Serialization).GetMethod("SerializeBytes", BindingFlags.Static | BindingFlags.Public);

            var TempSerializeToRpcDataMethod = typeof(RPCUtility.Serialization).GetMethod("TempSerializeToRpcData", BindingFlags.Static | BindingFlags.Public);
            var tempSerializeBytesMethod = typeof(RPCUtility.Serialization).GetMethod("TempSerializeBytes", BindingFlags.Static | BindingFlags.Public);

            var methodBuilderLst = new List<MethodBuilder>();

            var mLength = interfaceMethods.Length;
            /*
             * namespace Cosmos.RPC.RPCClient
             * {
             *      RPCClient rpcClient;
             *      public ClassName(RPCClient client)
             *      {
             *          rpcClient=client
             *      }
             * }
             */
            var constructorBody = constructorBuilder.GetILGenerator();
            constructorBody.Emit(OpCodes.Ldarg_0);
            constructorBody.Emit(OpCodes.Ldarg_1);
            constructorBody.Emit(OpCodes.Stfld, rpcClientField);
            constructorBody.Emit(OpCodes.Ret);

            //实现接口的方法；
            for (int i = 0; i < mLength; i++)
            {
                var methodInfo = interfaceMethods[i];
                //if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                //{
                //    throw new Exception("Client Proxy ReturnType only allows Task Type:" + methodInfo.Name);
                //}
                var paramTypes = methodInfo.GetParameters().Select(t => t.ParameterType).ToArray();
                var m = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Virtual | MethodAttributes.Public, methodInfo.ReturnType, paramTypes);
                methodBuilderLst.Add(m);
                var il = m.GetILGenerator();

                //LocalBuilder local_retrunType = il.DeclareLocal(methodInfo.ReturnType);

                LocalBuilder local_reqRpcData = il.DeclareLocal(typeof(RPCData));
                LocalBuilder local_reqRpcDataBytes = il.DeclareLocal(typeof(byte[]));

                //返回值Task<T> 中，T的类型；
                var retrunParameterTypes = methodInfo.ReturnParameter.ParameterType.GetTypeInfo().GenericTypeArguments;


                il.Emit(OpCodes.Ldstr, type.FullName);
                il.Emit(OpCodes.Ldstr, methodInfo.Name);

                if (retrunParameterTypes.Length > 0)
                    il.Emit(OpCodes.Ldtoken, retrunParameterTypes[0]);//TODO应该是这条有bug；
                else
                    il.Emit(OpCodes.Ldtoken, typeof(void));

                //静态方法生成RPCData
                il.Emit(OpCodes.Call, TempSerializeToRpcDataMethod);
                il.Emit(OpCodes.Stloc, local_reqRpcData);
                il.Emit(OpCodes.Ldloc, local_reqRpcData);


                //调用静态泛型方法
                il.Emit(OpCodes.Call, tempSerializeBytesMethod.MakeGenericMethod(typeof(RPCData)));
                il.Emit(OpCodes.Stloc, local_reqRpcDataBytes);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, rpcClientField);
                il.Emit(OpCodes.Ldloc, local_reqRpcDataBytes);
                il.Emit(OpCodes.Callvirt, tempSendMessageMethod);

                //这里声明并构造RpcTask<T> ；new RpcTask<T> (RpcData,false);
                var awaitRpcTaskType = typeof(RpcTask<>).MakeGenericType(retrunParameterTypes[0]);
                ConstructorInfo rpcTaskCtor = awaitRpcTaskType.GetConstructor(new Type[] { typeof(RPCData), typeof(bool) });
                il.Emit(OpCodes.Ldloc, local_reqRpcData);
                il.Emit(OpCodes.Ldc_I4_1); //load 1('true')
                il.Emit(OpCodes.Newobj, rpcTaskCtor);

                LocalBuilder local_rpcTaskTye = il.DeclareLocal(awaitRpcTaskType);
                //缓存new RpcTask<T> (RpcData,false)
                il.Emit(OpCodes.Stloc, local_rpcTaskTye);
                il.Emit(OpCodes.Ldloc, local_rpcTaskTye);


                il.Emit(OpCodes.Ret);
            }
            var newObject = (T)Activator.CreateInstance(typeBuilder.CreateType(), client);
            return newObject;
        }
        public async static Task<T> TaskRun<T>(RpcTask<T> rpcTask)
        {
            return await rpcTask;
        }
        //public async static Task<string> PrintNetMessage(string abc, string efg)
        //{
        //    var reqRpcData = RPCUtility.Serialization.SerializeToRpcData(typeof(RPCMessage), nameof(GetRoleData), typeof(RoleData), roleId);
        //    var bin = RPCUtility.Serialization.SerializeBytes(reqRpcData);
        //    SendMessage(bin);
        //    return await new RpcTask<string>(reqRpcData);
        //}
    }
}
