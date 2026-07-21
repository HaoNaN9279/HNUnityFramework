using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 将同组的字段垂直排列。组路径支持层级。
    /// </summary>
    public class VerticalGroupAttribute : PropertyAttribute
    {
        public string Path { get; }

        /// <param name="path">组路径</param>
        public VerticalGroupAttribute(string path)
        {
            Path = path;
        }
    }
}