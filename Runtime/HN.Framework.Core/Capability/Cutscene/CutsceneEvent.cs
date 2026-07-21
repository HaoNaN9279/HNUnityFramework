namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画事件数据，用于 TimelineEventTrack 向 EventBus 发布
    /// </summary>
    public readonly struct CutsceneEvent
    {
        /// <summary>事件类型标识（如 "DialogueStart"、"DoorOpen"）</summary>
        public string EventType { get; }

        /// <summary>所属过场资源标识</summary>
        public string CutsceneKey { get; }

        /// <summary>事件触发时间戳</summary>
        public double Timestamp { get; }

        /// <summary>事件附加参数（JSON 字符串或键值对）</summary>
        public string EventData { get; }

        public CutsceneEvent(string eventType, string cutsceneKey, double timestamp, string eventData = "")
        {
            EventType = eventType ?? string.Empty;
            CutsceneKey = cutsceneKey ?? string.Empty;
            Timestamp = timestamp;
            EventData = eventData ?? string.Empty;
        }
    }
}
