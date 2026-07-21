#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// Toast 浮动提示的配置参数。
    /// </summary>
    public readonly struct ToastConfig
    {
        /// <summary>
        /// 默认配置：持续 2 秒，显示在 Toast 层。
        /// </summary>
        public static readonly ToastConfig Default = new ToastConfig(2f, UILayer.Toast);

        /// <summary>
        /// 显示持续时间（秒）。
        /// </summary>
        public readonly float Duration;

        /// <summary>
        /// UI 渲染层级。
        /// </summary>
        public readonly UILayer Layer;

        /// <summary>
        /// 创建 Toast 配置。
        /// </summary>
        /// <param name="duration">显示持续时间（秒），默认 2 秒。</param>
        /// <param name="layer">UI 层级，默认 <see cref="UILayer.Toast"/>。</param>
        public ToastConfig(float duration = 2f, UILayer layer = UILayer.Toast)
        {
            Duration = duration;
            Layer = layer;
        }
    }
}
