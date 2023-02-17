using System.Linq;
using System.Reflection;

namespace Cosmos
{
    public static class ObjectExts
    {
        public static T As<T>(this object @this) where T : class
        {
            return @this as T;
        }
        public static T CastTo<T>(this object @this) where T : class
        {
            return (T)@this;
        }
        public static T InvokeMethod<T>(this object obj, string methodName, object[] args)
        {
            return (T)obj.GetType().GetMethod(methodName, args.Select(o => o.GetType()).ToArray()).Invoke(obj, args);
        }
        public static void InvokeMethod(this object obj, string methodName, object[] args)
        {
            var type = obj.GetType();
            type.GetMethod(methodName, args.Select(o => o.GetType()).ToArray()).Invoke(obj, args);
        }
        public static FieldInfo[] GetFields(this object obj)
        {
            FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return fieldInfos;
        }
    }
}
