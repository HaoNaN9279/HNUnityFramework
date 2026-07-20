using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 将同组的字段放入一个可折叠的面板中。组路径支持层级，如 "Stats/Base/Damage"。
    /// 同一路径的字段自动归入同一折叠组。
    /// </summary>
    public class FoldoutGroupAttribute : PropertyAttribute
    {
        public string Path { get; }

        /// <param name="path">组路径，支持层级格式 "Parent/Child"</param>
        public FoldoutGroupAttribute(string path)
        {
            Path = path;
        }
    }
}