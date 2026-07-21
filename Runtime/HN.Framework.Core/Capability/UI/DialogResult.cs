#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// 对话框用户操作结果。
    /// </summary>
    public enum DialogResult
    {
        /// <summary>
        /// 无操作（对话框未关闭或未做出选择）。
        /// </summary>
        None = 0,

        /// <summary>
        /// 用户点击了确认按钮。
        /// </summary>
        Confirm = 1,

        /// <summary>
        /// 用户点击了取消按钮。
        /// </summary>
        Cancel = 2
    }
}
