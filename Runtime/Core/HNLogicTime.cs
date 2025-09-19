using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
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
        public static double Time = 0.0d;

        /// <summary>
        /// 考虑在一个update中tick多次的情况，DeltaTime为每次Tick时的DeltaTime
        /// </summary>
        public static double DeltaTime = 0.0d;

        /// <summary>
        /// 逻辑帧数
        /// </summary>
        public static ulong LogicFrameCount = 0;
    }
}
