namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场字幕数据接口，定义单条字幕的时间轴信息和内容引用
    /// </summary>
    public interface ICutsceneSubtitle
    {
        /// <summary>字幕开始时间（秒）</summary>
        double StartTime { get; }

        /// <summary>字幕结束时间（秒）</summary>
        double EndTime { get; }

        /// <summary>本地化键（通过 C3 Localization ILocaleProvider.GetString 解析）</summary>
        string LocalizationKey { get; }

        /// <summary>说话者角色名（对应 CutsceneRole.RoleName）</summary>
        string SpeakerRole { get; }

        /// <summary>是否已显示</summary>
        bool IsDisplayed { get; }
    }
}
