using System.Collections.Generic;
using FixedMathSharp;
using MemoryPack;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 帧输入数据结构，包含帧号和该帧的输入动作快照。
    /// 使用 MemoryPack 序列化，通过网络传输。
    /// </summary>
    [MemoryPackable]
    public partial struct FrameInput
    {
        /// <summary>
        /// 帧号，标识此输入所属的逻辑帧。
        /// </summary>
        [MemoryPackOrder(0)]
        public ulong FrameNumber;

        /// <summary>
        /// 输入动作键值对。键为动作名称（如 "Horizontal", "Vertical", "Jump"），
        /// 值为使用 <see cref="Fixed64"/> 表示的定点数参数。
        /// </summary>
        [MemoryPackOrder(1)]
        public Dictionary<string, Fixed64> Actions;

        /// <summary>
        /// 创建指定帧号的帧输入数据。
        /// </summary>
        /// <param name="frameNumber">帧号</param>
        public FrameInput(ulong frameNumber)
        {
            FrameNumber = frameNumber;
            Actions = new Dictionary<string, Fixed64>();
        }

        /// <summary>
        /// 添加输入动作的便捷方法。
        /// </summary>
        /// <param name="name">动作名称</param>
        /// <param name="value">动作参数值</param>
        public void AddAction(string name, Fixed64 value)
        {
            Actions[name] = value;
        }
    }
}