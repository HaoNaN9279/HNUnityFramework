using System;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 将标注类型注册为图编辑器可创建的节点类型。
    /// 标注类型必须实现 <see cref="IGraphNodeFactory"/> 且提供公共无参构造函数。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GraphNodeTypeAttribute : Attribute
    {
        /// <summary>
        /// 初始化节点类型标注。
        /// </summary>
        /// <param name="path">节点在搜索菜单中的分类路径（如 "Animation/State"）。路径首段用于领域过滤。</param>
        public GraphNodeTypeAttribute(string path)
        {
            Path = path ?? string.Empty;
        }

        /// <summary>节点在搜索菜单中的分类路径。</summary>
        public string Path { get; }
    }
}
