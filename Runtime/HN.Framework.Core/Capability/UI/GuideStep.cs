#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// 新手引导中的单个步骤。
    /// 使用独立的 float 字段表示高亮偏移和尺寸，
    /// 因为 Core 程序集禁用了 UnityEngine 引用（noEngineReferences: true）。
    /// </summary>
    public class GuideStep
    {
        /// <summary>
        /// 步骤唯一标识。
        /// </summary>
        public string StepId { get; }

        /// <summary>
        /// 高亮目标节点名称。
        /// </summary>
        public string TargetName { get; }

        /// <summary>
        /// 步骤描述文本。
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 高亮框 X 轴偏移（世界/屏幕坐标）。
        /// </summary>
        public float HighlightOffsetX { get; }

        /// <summary>
        /// 高亮框 Y 轴偏移（世界/屏幕坐标）。
        /// </summary>
        public float HighlightOffsetY { get; }

        /// <summary>
        /// 高亮框宽度。
        /// </summary>
        public float HighlightSizeWidth { get; }

        /// <summary>
        /// 高亮框高度。
        /// </summary>
        public float HighlightSizeHeight { get; }

        /// <summary>
        /// 创建一个引导步骤。
        /// </summary>
        /// <param name="stepId">步骤唯一标识。</param>
        /// <param name="targetName">高亮目标节点名称。</param>
        /// <param name="description">步骤描述文本。</param>
        /// <param name="highlightOffsetX">高亮框 X 轴偏移，默认 0。</param>
        /// <param name="highlightOffsetY">高亮框 Y 轴偏移，默认 0。</param>
        /// <param name="highlightSizeWidth">高亮框宽度，默认 200。</param>
        /// <param name="highlightSizeHeight">高亮框高度，默认 200。</param>
        public GuideStep(
            string stepId,
            string targetName,
            string description,
            float highlightOffsetX = 0f,
            float highlightOffsetY = 0f,
            float highlightSizeWidth = 200f,
            float highlightSizeHeight = 200f)
        {
            StepId = stepId;
            TargetName = targetName;
            Description = description;
            HighlightOffsetX = highlightOffsetX;
            HighlightOffsetY = highlightOffsetY;
            HighlightSizeWidth = highlightSizeWidth;
            HighlightSizeHeight = highlightSizeHeight;
        }
    }
}
