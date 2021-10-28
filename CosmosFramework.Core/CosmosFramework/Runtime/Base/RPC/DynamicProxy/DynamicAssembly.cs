using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Cosmos.RPC
{
    internal class DynamicAssembly
    {
        readonly object locker = new object();
        readonly AssemblyBuilder assemblyBuilder;
        readonly ModuleBuilder moduleBuilder;

        public DynamicAssembly(string moduleName)
        {
            this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(moduleName), AssemblyBuilderAccess.Run);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
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
    }
}
