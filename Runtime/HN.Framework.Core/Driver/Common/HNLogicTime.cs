using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Core.Driver.Common
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
        /// 由驱动者（如 <see cref="GameWorldDriver.Update"/>）每帧调用，注入本帧实际耗时。
        /// <para>
        /// LockstepManager 在推进逻辑帧后会把 DeltaTime 写为固定帧间隔，
        /// 供同一逻辑帧内后续模块（AISystem/QuestManager/PoolBase）读取。
        /// </para>
        /// </summary>
        /// <param name="deltaTime">本帧的实际耗时（秒），通常为 UnityEngine.Time.deltaTime</param>
        public static void SetFrameDelta(double deltaTime)
        {
            DeltaTime = deltaTime;
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
