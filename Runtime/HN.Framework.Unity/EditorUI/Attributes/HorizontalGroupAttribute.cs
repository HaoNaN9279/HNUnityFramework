using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 将同组的字段水平排列在一行中。组路径支持同一行中的多个字段分组。
    /// </summary>
    public class HorizontalGroupAttribute : PropertyAttribute
    {
        public string Path { get; }

        /// <param name="path">组路径</param>
        public HorizontalGroupAttribute(string path)
        {
            Path = path;
        }
    }
}