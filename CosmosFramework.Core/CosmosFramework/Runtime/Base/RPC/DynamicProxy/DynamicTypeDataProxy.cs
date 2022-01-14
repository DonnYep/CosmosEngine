using System;
using System.Collections.Generic;
namespace Cosmos.RPC
{
    internal class DynamicTypeDataProxy
    {
        Dictionary<string, Type> stringTypeDict;
        public DynamicTypeDataProxy()
        {
            stringTypeDict = new Dictionary<string, Type>();
        }
        public bool HasType(string typeName)
        {
            return stringTypeDict.ContainsKey(typeName);
        }
        public bool TryGetType(string typeName, out Type type)
        {
            return stringTypeDict.TryGetValue(typeName, out type);
        }
        public void AddOrUpdate(string typeName, Type type)
        {
            if (stringTypeDict.ContainsKey(typeName))
                stringTypeDict[typeName] = type;
            else
                stringTypeDict.Add(typeName, type);
        }
        public bool Remove(string typeName,out Type type)
        {
            return stringTypeDict.Remove(typeName, out type);
        }
    }
}
