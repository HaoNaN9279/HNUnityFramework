using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 在字段上方渲染一个标题（含可选副标题和水平分割线）。
    /// </summary>
    public class TitleAttribute : PropertyAttribute
    {
        public string Title { get; }
        public string Subtitle { get; set; }
        public bool Bold { get; set; } = true;
        public bool HorizontalLine { get; set; } = true;

        /// <param name="title">标题文本</param>
        public TitleAttribute(string title)
        {
            Title = title;
        }
    }
}