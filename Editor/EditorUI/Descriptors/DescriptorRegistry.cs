using System;
using System.Collections.Generic;
using System.Linq;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// 描述器注册中心。管理所有 <see cref="IPropertyDescriptor"/> 和 <see cref="IClassDescriptor"/> 的注册与查询。
    /// 项目在 <c>[InitializeOnLoad]</c> 中调用注册方法添加自定义属性处理器。
    /// </summary>
    public static class DescriptorRegistry
    {
        private static readonly List<IPropertyDescriptor> s_propertyDescriptors = new List<IPropertyDescriptor>();
        private static readonly List<IClassDescriptor> s_classDescriptors = new List<IClassDescriptor>();
        private static readonly object s_lock = new object();
        private static bool s_dirty = true;

        // 按属性类型缓存的查询结果
        private static Dictionary<Type, IPropertyDescriptor[]> s_propertyDescriptorCache = new Dictionary<Type, IPropertyDescriptor[]>();
        private static Dictionary<Type, IClassDescriptor[]> s_classDescriptorCache = new Dictionary<Type, IClassDescriptor[]>();

        /// <summary>
        /// 注册一个字段级属性描述器。
        /// </summary>
        /// <param name="descriptor">描述器实例</param>
        public static void RegisterPropertyDescriptor(IPropertyDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            lock (s_lock)
            {
                s_propertyDescriptors.Add(descriptor);
                s_dirty = true;
            }
        }

        /// <summary>
        /// 注册一个类级属性处理器。
        /// </summary>
        /// <param name="descriptor">描述器实例</param>
        public static void RegisterClassDescriptor(IClassDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            lock (s_lock)
            {
                s_classDescriptors.Add(descriptor);
                s_dirty = true;
            }
        }

        /// <summary>
        /// 获取能处理指定属性类型的所有字段级描述器。
        /// </summary>
        /// <param name="attributeType">属性类型</param>
        /// <returns>匹配的描述器数组</returns>
        public static IPropertyDescriptor[] GetPropertyDescriptors(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            RebuildCacheIfDirty();
            return s_propertyDescriptorCache.TryGetValue(attributeType, out var result)
                ? result
                : Array.Empty<IPropertyDescriptor>();
        }

        /// <summary>
        /// 获取能处理指定属性类型的所有类级描述器，按 <see cref="IClassDescriptor.Order"/> 升序排列。
        /// </summary>
        /// <param name="attributeType">属性类型</param>
        /// <returns>匹配的描述器数组（已排序）</returns>
        public static IClassDescriptor[] GetClassDescriptors(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            RebuildCacheIfDirty();
            return s_classDescriptorCache.TryGetValue(attributeType, out var result)
                ? result
                : Array.Empty<IClassDescriptor>();
        }

        /// <summary>
        /// 获取所有已注册的字段级描述器。
        /// </summary>
        public static IPropertyDescriptor[] GetAllPropertyDescriptors()
        {
            RebuildCacheIfDirty();
            return s_propertyDescriptors.ToArray();
        }

        /// <summary>
        /// 获取所有已注册的类级描述器（按 Order 排序）。
        /// </summary>
        public static IClassDescriptor[] GetAllClassDescriptors()
        {
            RebuildCacheIfDirty();
            return s_classDescriptors.OrderBy(d => d.Order).ToArray();
        }

        /// <summary>
        /// 清空所有注册和缓存。
        /// </summary>
        public static void Clear()
        {
            lock (s_lock)
            {
                s_propertyDescriptors.Clear();
                s_classDescriptors.Clear();
                s_propertyDescriptorCache.Clear();
                s_classDescriptorCache.Clear();
                s_dirty = false;
            }
        }

        private static void RebuildCacheIfDirty()
        {
            if (!s_dirty) return;
            lock (s_lock)
            {
                if (!s_dirty) return;
                var newPropCache = new Dictionary<Type, IPropertyDescriptor[]>();
                var newClassCache = new Dictionary<Type, IClassDescriptor[]>();

                foreach (var descriptor in s_propertyDescriptors)
                {
                    foreach (var attrType in descriptor.SupportedAttributeTypes)
                    {
                        if (!newPropCache.ContainsKey(attrType))
                            newPropCache[attrType] = s_propertyDescriptors
                                .Where(d => d.SupportedAttributeTypes.Contains(attrType)).ToArray();
                    }
                }

                foreach (var descriptor in s_classDescriptors)
                {
                    foreach (var attrType in descriptor.SupportedAttributeTypes)
                    {
                        if (!newClassCache.ContainsKey(attrType))
                            newClassCache[attrType] = s_classDescriptors
                                .Where(d => d.SupportedAttributeTypes.Contains(attrType))
                                .OrderBy(d => d.Order).ToArray();
                    }
                }

                s_propertyDescriptorCache = newPropCache;
                s_classDescriptorCache = newClassCache;
                s_dirty = false;
            }
        }
    }
}
