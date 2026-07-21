using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 在字段上方渲染一个信息/警告/错误框，支持条件显示。
    /// </summary>
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public InfoMessageType Type { get; set; } = InfoMessageType.Info;
        public string VisibleIf { get; set; }

        /// <param name="message">显示的消息文本</param>
        public InfoBoxAttribute(string message)
        {
            Message = message;
        }
    }
}
