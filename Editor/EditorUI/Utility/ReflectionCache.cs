using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace HN.Framework.Editor.EditorUI.Utility
{
    /// <summary>
    /// 反射结果缓存，避免每帧重复调用 Type.GetFields / GetMethods / GetCustomAttributes。
    /// 线程安全，基于 ConcurrentDictionary 实现。
    /// </summary>
    public static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, FieldInfo[]> s_fieldCache = new ConcurrentDictionary<Type, FieldInfo[]>();
        private static readonly ConcurrentDictionary<Type, MethodInfo[]> s_methodCache = new ConcurrentDictionary<Type, MethodInfo[]>();
        private static readonly ConcurrentDictionary<(Type, Type), Attribute[]> s_attributeCache = new ConcurrentDictionary<(Type, Type), Attribute[]>();

        /// <summary>
        /// 获取指定类型的所有实例字段（public + nonpublic）。
        /// 结果被缓存，多次调用不会重复反射。
        /// </summary>
        public static FieldInfo[] GetFields(Type type)
        {
            return s_fieldCache.GetOrAdd(type, t =>
                t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        }

        /// <summary>
        /// 获取指定类型的所有实例方法（public + nonpublic）。
        /// 结果被缓存。
        /// </summary>
        public static MethodInfo[] GetMethods(Type type)
        {
            return s_methodCache.GetOrAdd(type, t =>
                t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
        }

        /// <summary>
        /// 获取指定类型上指定类型的自定义特性。
        /// 结果被缓存。
        /// </summary>
        /// <typeparam name="TAttribute">要查找的特性类型</typeparam>
        /// <param name="type">目标类型</param>
        /// <returns>匹配的特性数组</returns>
        public static TAttribute[] GetAttributes<TAttribute>(Type type) where TAttribute : Attribute
        {
            var key = (type, typeof(TAttribute));
            return (TAttribute[])s_attributeCache.GetOrAdd(key, k =>
                (Attribute[])type.GetCustomAttributes(typeof(TAttribute), true));
        }

        /// <summary>
        /// 清除所有缓存。在 AssemblyReload 或需要强制刷新时调用。
        /// </summary>
        public static void Clear()
        {
            s_fieldCache.Clear();
            s_methodCache.Clear();
            s_attributeCache.Clear();
        }
    }
}