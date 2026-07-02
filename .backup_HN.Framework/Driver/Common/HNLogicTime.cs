using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Driver.Common
{
    /// <summary>
    /// 逻辑时间管理类，提供逻辑层的时间、帧增量与帧计数。
    /// </summary>
    public class HNLogicTime
    {
        public static void Initialize()
        {
            Time = 0.0d;
            DeltaTime = 0.0d;
            LogicFrameCount = 0;
        }


        /// <summary>
        /// 考虑在一个update中tick多次的情况，Time为每次Tick时的Time
        /// </summary>
        public static double Time { get; internal set; } = 0.0d;

        /// <summary>
        /// 考虑在一个update中tick多次的情况，DeltaTime为每次Tick时的DeltaTime
        /// </summary>
        public static double DeltaTime { get; internal set; } = 0.0d;

        /// <summary>
        /// 逻辑帧数
        /// </summary>
        public static ulong LogicFrameCount { get; internal set; } = 0;
    }
}
