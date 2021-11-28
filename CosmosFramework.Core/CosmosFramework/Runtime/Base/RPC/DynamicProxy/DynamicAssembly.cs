using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
namespace Cosmos.RPC
{
    internal class DynamicAssembly
    {
        readonly object locker = new object();
        readonly AssemblyBuilder assemblyBuilder;
        readonly ModuleBuilder moduleBuilder;
        readonly Dictionary<string, Type> stringTypeDict;
        public DynamicAssembly(string moduleName)
        {
            this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(moduleName), AssemblyBuilderAccess.Run);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
            this.stringTypeDict = new Dictionary<string, Type>();
        }
        public TypeBuilder DefineType(string name, TypeAttributes attr)
        {
            lock (locker)
            {
                return moduleBuilder.DefineType(name, attr);
            }
        }
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
        {
            lock (locker)
            {
                return moduleBuilder.DefineType(name, attr, parent);
            }
        }
        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent, Type[] interfaces)
        {
            lock (locker)
            {
                return moduleBuilder.DefineType(name, attr, parent, interfaces);
            }
        }
        public void AddOrUpdate(string typeName,Type type)
        {
            stringTypeDict.AddOrUpdate(typeName, type);
        }
        public bool PeekType(string typeName,out Type type)
        {
            return stringTypeDict.TryGetValue(typeName, out type);
        }
    }
}
