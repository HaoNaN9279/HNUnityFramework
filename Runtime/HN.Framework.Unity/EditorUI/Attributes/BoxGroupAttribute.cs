using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 将同组的字段放入一个带框线的区域中。组路径支持层级。
    /// </summary>
    public class BoxGroupAttribute : PropertyAttribute
    {
        public string Path { get; }

        /// <param name="path">组路径，支持层级格式 "Parent/Child"</param>
        public BoxGroupAttribute(string path)
        {
            Path = path;
        }
    }
}