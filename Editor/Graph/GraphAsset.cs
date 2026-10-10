using System;
using System.Reflection;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Core.Level.Logic.Graph;
using UnityEngine;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 图数据资产。以 MemoryPack 二进制负载持久化 <see cref="GraphData"/>，支持领域派生的图类型。
    /// </summary>
    public class GraphAsset : ScriptableObject
    {
        [SerializeField]
        private byte[] _payload;

        [SerializeField]
        private string _graphTypeName;

        private static readonly MethodInfo SerializeMethod = FindSerializeMethod();
        private static readonly MethodInfo DeserializeMethod = FindDeserializeMethod();

        /// <summary>是否包含有效负载。</summary>
        public bool HasPayload => _payload != null && _payload.Length > 0;

        /// <summary>图数据的类型名。</summary>
        public string GraphTypeName => _graphTypeName;

        /// <summary>
        /// 读取图中的数据。
        /// </summary>
        /// <returns>反序列化后的图数据；无负载时返回已初始化的空图。</returns>
        public GraphData Load()
        {
            if (!HasPayload)
            {
                return new GraphData();
            }

            var type = string.IsNullOrEmpty(_graphTypeName) ? typeof(GraphData) : Type.GetType(_graphTypeName);
            if (type == null || !typeof(GraphData).IsAssignableFrom(type))
            {
                type = typeof(GraphData);
            }

            var method = DeserializeMethod.MakeGenericMethod(type);
            return (GraphData)method.Invoke(null, new object[] { _payload });
        }

        /// <summary>
        /// 写入图数据。
        /// </summary>
        /// <param name="graph">要写入的图数据。</param>
        public void SetGraph(GraphData graph)
        {
            if (graph == null)
            {
                _payload = null;
                _graphTypeName = null;
                return;
            }

            var type = graph.GetType();
            _graphTypeName = type.AssemblyQualifiedName;
            var method = SerializeMethod.MakeGenericMethod(type);
            _payload = (byte[])method.Invoke(null, new object[] { graph });
        }

        private static MethodInfo FindSerializeMethod()
        {
            var methods = typeof(MemoryPackSerializer).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == nameof(MemoryPackSerializer.Serialize) &&
                    method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 1)
                {
                    return method;
                }
            }

            return null;
        }

        private static MethodInfo FindDeserializeMethod()
        {
            var methods = typeof(MemoryPackSerializer).GetMethods(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == nameof(MemoryPackSerializer.Deserialize) &&
                    method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(byte[]))
                {
                    return method;
                }
            }

            return null;
        }
    }
}
