#nullable enable

using System;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 射线检测命中结果，描述射线与碰撞体相交的位置、法线等信息。
    /// </summary>
    public readonly struct RaycastHit : IEquatable<RaycastHit>
    {
        /// <summary>
        /// 命中点的世界坐标
        /// </summary>
        public readonly PhysicsVector3 Point;

        /// <summary>
        /// 命中面的法线方向
        /// </summary>
        public readonly PhysicsVector3 Normal;

        /// <summary>
        /// 从射线原点沿射线方向到命中点的距离
        /// </summary>
        public readonly float Distance;

        /// <summary>
        /// 被命中的碰撞体实例 ID
        /// </summary>
        public readonly int ColliderInstanceId;

        /// <summary>
        /// 初始化射线检测命中结果
        /// </summary>
        /// <param name="point">命中点的世界坐标</param>
        /// <param name="normal">命中面的法线方向</param>
        /// <param name="distance">射线起点到命中点的距离</param>
        /// <param name="colliderInstanceId">被命中的碰撞体实例 ID</param>
        public RaycastHit(PhysicsVector3 point, PhysicsVector3 normal, float distance, int colliderInstanceId)
        {
            Point = point;
            Normal = normal;
            Distance = distance;
            ColliderInstanceId = colliderInstanceId;
        }

        /// <summary>
        /// 判断当前实例是否等于指定对象
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is RaycastHit other && Equals(other);
        }

        /// <summary>
        /// 判断两个命中结果是否相等
        /// </summary>
        public bool Equals(RaycastHit other)
        {
            return Point == other.Point
                   && Normal == other.Normal
                   && MathF.Abs(Distance - other.Distance) < 1e-6f
                   && ColliderInstanceId == other.ColliderInstanceId;
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Point.GetHashCode();
                hash = hash * 23 + Normal.GetHashCode();
                hash = hash * 23 + Distance.GetHashCode();
                hash = hash * 23 + ColliderInstanceId.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 判断两个命中结果是否相等
        /// </summary>
        public static bool operator ==(RaycastHit left, RaycastHit right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 判断两个命中结果是否不相等
        /// </summary>
        public static bool operator !=(RaycastHit left, RaycastHit right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 返回命中结果的调试字符串
        /// </summary>
        public override string ToString()
        {
            return $"[RaycastHit] Point={Point}, Normal={Normal}, Distance={Distance:F3}, ColliderId={ColliderInstanceId}";
        }
    }
}
