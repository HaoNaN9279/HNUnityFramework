using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 将字段组织到标签页中。标签页名称为路径的第一段，如 "Stats/Base" 归入 "Stats" 标签页。
    /// </summary>
    public class TabGroupAttribute : PropertyAttribute
    {
        public string Path { get; }

        /// <param name="path">组路径，首段为标签页名，如 "Stats/Base"</param>
        public TabGroupAttribute(string path)
        {
            Path = path;
        }
    }
}