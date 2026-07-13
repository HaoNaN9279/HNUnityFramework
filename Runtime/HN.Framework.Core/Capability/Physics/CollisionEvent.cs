#nullable enable

using System;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 碰撞事件类型
    /// </summary>
    public enum CollisionEventType
    {
        /// <summary>碰撞开始</summary>
        Enter,

        /// <summary>碰撞持续</summary>
        Stay,

        /// <summary>碰撞结束</summary>
        Exit,
    }

    /// <summary>
    /// 碰撞事件，描述两个碰撞体之间发生的碰撞或触发信息。
    /// </summary>
    public readonly struct CollisionEvent : IEquatable<CollisionEvent>
    {
        /// <summary>
        /// 自身的实例 ID
        /// </summary>
        public readonly int SelfInstanceId;

        /// <summary>
        /// 对方的实例 ID
        /// </summary>
        public readonly int OtherInstanceId;

        /// <summary>
        /// 相对速度
        /// </summary>
        public readonly PhysicsVector3 RelativeVelocity;

        /// <summary>
        /// 碰撞事件类型
        /// </summary>
        public readonly CollisionEventType Type;

        /// <summary>
        /// 初始化碰撞事件
        /// </summary>
        /// <param name="selfInstanceId">自身的实例 ID</param>
        /// <param name="otherInstanceId">对方的实例 ID</param>
        /// <param name="relativeVelocity">相对速度</param>
        /// <param name="type">碰撞事件类型</param>
        public CollisionEvent(int selfInstanceId, int otherInstanceId, PhysicsVector3 relativeVelocity, CollisionEventType type)
        {
            SelfInstanceId = selfInstanceId;
            OtherInstanceId = otherInstanceId;
            RelativeVelocity = relativeVelocity;
            Type = type;
        }

        /// <summary>
        /// 判断当前实例是否等于指定对象
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is CollisionEvent other && Equals(other);
        }

        /// <summary>
        /// 判断两个碰撞事件是否相等
        /// </summary>
        public bool Equals(CollisionEvent other)
        {
            return SelfInstanceId == other.SelfInstanceId
                   && OtherInstanceId == other.OtherInstanceId
                   && RelativeVelocity == other.RelativeVelocity
                   && Type == other.Type;
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + SelfInstanceId.GetHashCode();
                hash = hash * 23 + OtherInstanceId.GetHashCode();
                hash = hash * 23 + RelativeVelocity.GetHashCode();
                hash = hash * 23 + Type.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 判断两个碰撞事件是否相等
        /// </summary>
        public static bool operator ==(CollisionEvent left, CollisionEvent right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 判断两个碰撞事件是否不相等
        /// </summary>
        public static bool operator !=(CollisionEvent left, CollisionEvent right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 返回碰撞事件的调试字符串
        /// </summary>
        public override string ToString()
        {
            return $"[CollisionEvent] Self={SelfInstanceId}, Other={OtherInstanceId}, Type={Type}, RelVel={RelativeVelocity}";
        }
    }
}
