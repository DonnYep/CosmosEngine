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
    */
    //================================================
    /// <summary>
    /// https://blog.csdn.net/xiaouncle/article/details/52776007
    /// </summary>
    internal class DynamicProxyFactory
    {
        public static T CreateDynamicProxy<T>(RPCClient client)
where T : IService<T>
        {
            /*
             * namespace Cosmos.RPC.RPCClient
             * {
             *      public class ProxyClass
             *      {
             *          RPCClient rpcClient;
             *          public ProxyClass(RPCClient client)
             *          {
             *              rpcClient=client
             *          }
             *      }
             * }
             */
            if (client == null) throw new ArgumentNullException("RPCClient  is invaild !");
            var interfaceType = typeof(T);

            var interfaceTypeInfo = interfaceType.GetTypeInfo();
            if (!interfaceTypeInfo.IsInterface)
                throw new Exception("Client Proxy only allows interface. Type : " + interfaceTypeInfo.Name);
            if (!typeof(IService<>).MakeGenericType(interfaceType).IsAssignableFrom(interfaceType))
                throw new NotImplementedException("Client Proxy interface is not inherit from IService<> : " + interfaceType);

            var baseType = typeof(ServiceBase);
            var interfaceMethods = interfaceTypeInfo.GetMethods();

            var typeBuilder = DynamicClientAssemblyHolder.Assembly.DefineType(typeof(T).Name, TypeAttributes.Public, typeof(object));
            //设置基类；
            typeBuilder.SetParent(baseType);
            //对象实现接口
            typeBuilder.AddInterfaceImplementation(typeof(T));
            //对象的构造方法 pulic ClassName(RPCClient client){}
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis | CallingConventions.Standard, new Type[] { typeof(RPCClient) });

            //对象定义一个字段；
            //var rpcClientField = typeBuilder.DefineField("rpcClient", typeof(RPCClient), FieldAttributes.Private);

           

            var createVoidRpcTaskMethod = baseType.GetMethod("CreateVoidRpcTask", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var awaitVoidRpcTaskMethod = baseType.GetMethod("AwaitVoidRpcTask", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var protectRpcClient = baseType.GetField("rpcClient", BindingFlags.Instance| BindingFlags.NonPublic| BindingFlags.Public);

            var methodLength = interfaceMethods.Length;
            //构造函数
            var constructorBody = ctorBuilder.GetILGenerator();
            constructorBody.Emit(OpCodes.Ldarg_0);
            constructorBody.Emit(OpCodes.Ldarg_1);
            //constructorBody.Emit(OpCodes.Stfld, rpcClientField);
            constructorBody.Emit(OpCodes.Stfld, protectRpcClient);
            constructorBody.Emit(OpCodes.Ret);
            //实现接口的方法；
            for (int i = 0; i < methodLength; i++)
            {
                var methodInfo = interfaceMethods[i];
                //约束返回值必须为Task类型；
                if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                {
                    throw new Exception("Client Proxy ReturnType only allows Task Type:" + methodInfo.Name);
                }
                //函数的形参；
                var formalParamTypes = methodInfo.GetParameters().Select(t => t.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Virtual | MethodAttributes.Public, methodInfo.ReturnType, formalParamTypes);
                var methodIL = methodBuilder.GetILGenerator();
                //返回值Task<T> 中，T的类型；
                var returnParameterTypes = methodInfo.ReturnParameter.ParameterType.GetTypeInfo().GenericTypeArguments;

                //返回值为Task<T>
                if (returnParameterTypes.Length > 0)
                {
                    var returnParamType = returnParameterTypes[0];
                    var createResultfulRpcTaskMethod = baseType.GetMethod("CreateResultfulRpcTask", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(returnParamType);
                    var awaitResultfulRpcTaskMethod = baseType.GetMethod("AwaitResultfulRpcTask", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(returnParamType); 

                    var rpcTaskType = typeof(RpcTask<>).MakeGenericType(returnParamType);
                    var taskType = typeof(Task<>).MakeGenericType(returnParamType);

                    var localRpcTask = methodIL.DeclareLocal(rpcTaskType);
                    var localTask = methodIL.DeclareLocal(taskType);

                    // var rpcTask =  CreateResultfulRpcTask<T>("TypeFullName", "MethodName");
                    methodIL.Emit(OpCodes.Ldarg_0);
                    methodIL.Emit(OpCodes.Ldstr, interfaceType.FullName);
                    methodIL.Emit(OpCodes.Ldstr, methodInfo.Name);

                    var formalParamCount = formalParamTypes.Length;

                    methodIL.Emit(OpCodes.Ldc_I4_S, formalParamCount);
                    methodIL.Emit(OpCodes.Newarr, typeof(object));
                    //params object[] parameter
                    for (int j = 0; j < formalParamCount; j++)
                    {
                        methodIL.Emit(OpCodes.Dup);
                        methodIL.Emit(OpCodes.Ldc_I4_S, j);
                        methodIL.Emit(OpCodes.Ldarg_S, j + 1);
                        //不是引用类型；
                        if (!formalParamTypes[j].IsClass)
                            methodIL.Emit(OpCodes.Box, formalParamTypes[j]);
                        methodIL.Emit(OpCodes.Stelem_Ref);
                    }
                    methodIL.Emit(OpCodes.Callvirt, createResultfulRpcTaskMethod);
                    methodIL.Emit(OpCodes.Stloc, localRpcTask);

                    //AwaitResultfulRpcTask<T>(rpcTask);
                    methodIL.Emit(OpCodes.Ldarg_0);
                    methodIL.Emit(OpCodes.Ldloc, localRpcTask);
                    methodIL.Emit(OpCodes.Callvirt, awaitResultfulRpcTaskMethod);
                    methodIL.Emit(OpCodes.Stloc, localTask);
                    methodIL.Emit(OpCodes.Ldloc, localTask);
                    methodIL.Emit(OpCodes.Ret);
                }
                else//返回值为Task
                {
                    //设置方法的泛型；
                    var localRpcTask = methodIL.DeclareLocal(typeof(RpcTask));
                    var localTask = methodIL.DeclareLocal(typeof(Task));

                    // var rpcTask =  CreateResultfulRpcTask("TypeFullName", "MethodName");
                    methodIL.Emit(OpCodes.Ldarg_0);
                    methodIL.Emit(OpCodes.Ldstr, interfaceType.FullName);
                    methodIL.Emit(OpCodes.Ldstr, methodInfo.Name);

                    var formalParamCount = formalParamTypes.Length;

                    methodIL.Emit(OpCodes.Ldc_I4_S, formalParamCount);
                    methodIL.Emit(OpCodes.Newarr,typeof(object));
                    //params object[] parameter
                    for (int j = 0; j < formalParamCount; j++)
                    {
                        methodIL.Emit(OpCodes.Dup);
                        methodIL.Emit(OpCodes.Ldc_I4_S, j);
                        methodIL.Emit(OpCodes.Ldarg_S, j+1);
                        //不是引用类型；
                        if (!formalParamTypes[j].IsClass)
                            methodIL.Emit(OpCodes.Box, formalParamTypes[j]);
                        methodIL.Emit(OpCodes.Stelem_Ref);
                    }

                    methodIL.Emit(OpCodes.Callvirt, createVoidRpcTaskMethod);
                    methodIL.Emit(OpCodes.Stloc, localRpcTask);

                    //AwaitResultfulRpcTask(rpcTask);
                    methodIL.Emit(OpCodes.Ldarg_0);
                    methodIL.Emit(OpCodes.Ldloc, localRpcTask);
                    methodIL.Emit(OpCodes.Callvirt, awaitVoidRpcTaskMethod);
                    methodIL.Emit(OpCodes.Stloc, localTask);
                    methodIL.Emit(OpCodes.Ldloc, localTask);
                    methodIL.Emit(OpCodes.Ret);
                }
                methodIL.Emit(OpCodes.Ret);
            }
            var newObject = (T)Activator.CreateInstance(typeBuilder.CreateType(), client);
            return newObject;
        }
    }
}
