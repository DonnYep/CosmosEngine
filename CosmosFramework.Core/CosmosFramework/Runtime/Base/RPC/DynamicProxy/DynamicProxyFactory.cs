using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Linq;
using Cosmos.RPC.Client;
using Cosmos.RPC;
namespace Cosmos.RPC
{
    /// <summary>
    /// https://blog.csdn.net/xiaouncle/article/details/52776007
    /// </summary>
    public class DynamicProxyFactory
    {
        public static T CreateProxy<T>()
            where T:IService<T>
        {
            var type = typeof(T);
            var iinterfaceType = type.GetTypeInfo();
            if (!iinterfaceType.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + iinterfaceType.Name);

            var interfaceMethods = iinterfaceType.GetMethods();

            //var assemblyName = new AssemblyName("RPCServer");
            //var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            //var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicProxy");
            //var typeBuilder = moduleBuilder.DefineType(typeof(T).Name, TypeAttributes.Public, typeof(object));

            var typeBuilder = DynamicClientAssemblyHolder.Assembly.DefineType(typeof(T).Name, TypeAttributes.Public, typeof(object));

            typeBuilder.AddInterfaceImplementation(typeof(T));

            var methodBuilderLst = new List<MethodBuilder>();

            var mLength = interfaceMethods.Length;


            var printMethod = typeof(DynamicProxyFactory).GetMethod("PrintString", new Type[] { typeof(string), typeof(string) });

            for (int i = 0; i < mLength; i++)
            {
                var methodInfo = interfaceMethods[i];
                var paramTypes = methodInfo.GetParameters().Select(t=>t.ParameterType).ToArray();
                var m = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes .Virtual| MethodAttributes.Public, methodInfo.ReturnType, paramTypes);

                methodBuilderLst.Add(m);
                var il = m.GetILGenerator();


                LocalBuilder local = il.DeclareLocal(methodInfo.ReturnType);
                int paramIndex = 1;
                for (int j = 0; j <paramTypes.Length; j++)
                {
                    il.Emit(OpCodes.Ldarg_S, paramIndex);
                    paramIndex++;
                }
                //il.Emit(OpCodes.Ldarg_1);
                //il.Emit(OpCodes.Ldarg_2);

                //il.Emit(OpCodes.Ldarg_S,1);
                //il.Emit(OpCodes.Ldarg_S,2);

                il.Emit(OpCodes.Call,  printMethod);
                il.Emit(OpCodes.Stloc_0, local);
                il.Emit(OpCodes.Ldloc_0, local);
                il.Emit(OpCodes.Ret);
            }
            //Console.WriteLine(typeBuilder.CreateType());
            var newObject = (T)Activator.CreateInstance(typeBuilder.CreateType());
            return newObject;
        }


        public static T CreateProxy<T>(RPCClient client)
    where T : IService<T>
        {
            var type = typeof(T);
            var iinterfaceType = type.GetTypeInfo();
            if (!iinterfaceType.IsInterface) throw new Exception("Client Proxy only allows interface. Type:" + iinterfaceType.Name);

            var interfaceMethods = iinterfaceType.GetMethods();

            var typeBuilder = DynamicClientAssemblyHolder.Assembly.DefineType(typeof(T).Name, TypeAttributes.Public, typeof(object));

            typeBuilder.AddInterfaceImplementation(typeof(T));

            var methodBuilderLst = new List<MethodBuilder>();

            var mLength = interfaceMethods.Length;


            var printMethod = typeof(DynamicProxyFactory).GetMethod("PrintString", new Type[] { typeof(string), typeof(string) });

            for (int i = 0; i < mLength; i++)
            {
                var methodInfo = interfaceMethods[i];
                var paramTypes = methodInfo.GetParameters().Select(t => t.ParameterType).ToArray();
                var m = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Virtual | MethodAttributes.Public, methodInfo.ReturnType, paramTypes);

                methodBuilderLst.Add(m);
                var il = m.GetILGenerator();

                LocalBuilder local = il.DeclareLocal(methodInfo.ReturnType);
                int paramIndex = 1;
                for (int j = 0; j < paramTypes.Length; j++)
                {
                    il.Emit(OpCodes.Ldarg_S, paramIndex);
                    paramIndex++;
                }
                //il.Emit(OpCodes.Ldarg_1);
                //il.Emit(OpCodes.Ldarg_2);

                //il.Emit(OpCodes.Ldarg_S,1);
                //il.Emit(OpCodes.Ldarg_S,2);

                il.Emit(OpCodes.Call, printMethod);
                il.Emit(OpCodes.Stloc_0, local);
                il.Emit(OpCodes.Ldloc_0, local);
                il.Emit(OpCodes.Ret);
            }
            //Console.WriteLine(typeBuilder.CreateType());
            var newObject = (T)Activator.CreateInstance(typeBuilder.CreateType());
            return newObject;
        }
        public static string PrintString(string str,string now)
        {

            return "Print <-> " + str +now;
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
