#nullable enable

using System;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 三维向量，表示物理世界中的位置、方向或速度。
    /// 纯 C# 实现，不依赖 UnityEngine 类型。
    /// </summary>
    public readonly struct PhysicsVector3 : IEquatable<PhysicsVector3>
    {
        /// <summary>X 分量</summary>
        public readonly float X;

        /// <summary>Y 分量</summary>
        public readonly float Y;

        /// <summary>Z 分量</summary>
        public readonly float Z;

        /// <summary>零向量 (0, 0, 0)</summary>
        public static readonly PhysicsVector3 Zero = new PhysicsVector3(0f, 0f, 0f);

        /// <summary>全 1 向量 (1, 1, 1)</summary>
        public static readonly PhysicsVector3 One = new PhysicsVector3(1f, 1f, 1f);

        /// <summary>上方向 (0, 1, 0)</summary>
        public static readonly PhysicsVector3 Up = new PhysicsVector3(0f, 1f, 0f);

        /// <summary>前方向 (0, 0, 1)</summary>
        public static readonly PhysicsVector3 Forward = new PhysicsVector3(0f, 0f, 1f);

        /// <summary>右方向 (1, 0, 0)</summary>
        public static readonly PhysicsVector3 Right = new PhysicsVector3(1f, 0f, 0f);

        /// <summary>
        /// 初始化三维向量
        /// </summary>
        /// <param name="x">X 分量</param>
        /// <param name="y">Y 分量</param>
        /// <param name="z">Z 分量</param>
        public PhysicsVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// 向量长度
        /// </summary>
        public float Magnitude => MathF.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// 向量长度的平方（避免开方运算，性能更优）
        /// </summary>
        public float SqrMagnitude => X * X + Y * Y + Z * Z;

        /// <summary>
        /// 归一化后的向量（方向不变，长度为 1）
        /// 若当前向量为零向量，返回 Zero。
        /// </summary>
        public PhysicsVector3 Normalized
        {
            get
            {
                float sqrMag = SqrMagnitude;
                if (sqrMag < 1e-10f)
                {
                    return Zero;
                }

                float invMag = 1f / MathF.Sqrt(sqrMag);
                return new PhysicsVector3(X * invMag, Y * invMag, Z * invMag);
            }
        }

        /// <summary>
        /// 返回归一化后的新向量（方向不变，长度为 1）
        /// 若当前向量为零向量，返回 Zero。
        /// </summary>
        /// <returns>归一化后的向量</returns>
        public PhysicsVector3 Normalize()
        {
            return Normalized;
        }

        /// <summary>
        /// 计算两个向量的点积
        /// </summary>
        /// <param name="a">向量 a</param>
        /// <param name="b">向量 b</param>
        /// <returns>点积值</returns>
        public static float Dot(PhysicsVector3 a, PhysicsVector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        /// <summary>
        /// 计算两个点之间的欧几里得距离
        /// </summary>
        /// <param name="a">点 a</param>
        /// <param name="b">点 b</param>
        /// <returns>两点间距离</returns>
        public static float Distance(PhysicsVector3 a, PhysicsVector3 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            float dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>
        /// 计算两个向量的叉积
        /// </summary>
        /// <param name="a">向量 a</param>
        /// <param name="b">向量 b</param>
        /// <returns>叉积结果（垂直于 a 和 b 的向量）</returns>
        public static PhysicsVector3 Cross(PhysicsVector3 a, PhysicsVector3 b)
        {
            return new PhysicsVector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        /// <summary>
        /// 线性插值（a + (b - a) * t），t 不进行钳制
        /// </summary>
        /// <param name="a">起始向量</param>
        /// <param name="b">目标向量</param>
        /// <param name="t">插值因子</param>
        /// <returns>插值结果</returns>
        public static PhysicsVector3 Lerp(PhysicsVector3 a, PhysicsVector3 b, float t)
        {
            return new PhysicsVector3(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        /// <summary>
        /// 向量加法
        /// </summary>
        public static PhysicsVector3 operator +(PhysicsVector3 a, PhysicsVector3 b)
        {
            return new PhysicsVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// 向量减法
        /// </summary>
        public static PhysicsVector3 operator -(PhysicsVector3 a, PhysicsVector3 b)
        {
            return new PhysicsVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// 标量乘法（向量 × 标量）
        /// </summary>
        public static PhysicsVector3 operator *(PhysicsVector3 v, float scalar)
        {
            return new PhysicsVector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        /// <summary>
        /// 标量乘法（标量 × 向量）
        /// </summary>
        public static PhysicsVector3 operator *(float scalar, PhysicsVector3 v)
        {
            return new PhysicsVector3(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        /// <summary>
        /// 标量除法
        /// </summary>
        public static PhysicsVector3 operator /(PhysicsVector3 v, float scalar)
        {
            return new PhysicsVector3(v.X / scalar, v.Y / scalar, v.Z / scalar);
        }

        /// <summary>
        /// 判断两个向量是否相等
        /// </summary>
        public static bool operator ==(PhysicsVector3 left, PhysicsVector3 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 判断两个向量是否不相等
        /// </summary>
        public static bool operator !=(PhysicsVector3 left, PhysicsVector3 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 判断当前实例是否等于指定对象
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is PhysicsVector3 other && Equals(other);
        }

        /// <summary>
        /// 判断两个向量是否相等（逐分量比较）
        /// </summary>
        public bool Equals(PhysicsVector3 other)
        {
            const float epsilon = 1e-6f;
            return MathF.Abs(X - other.X) < epsilon
                   && MathF.Abs(Y - other.Y) < epsilon
                   && MathF.Abs(Z - other.Z) < epsilon;
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                hash = hash * 23 + Z.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 返回向量的调试字符串
        /// </summary>
        public override string ToString()
        {
            return $"({X:F3}, {Y:F3}, {Z:F3})";
        }
    }
}
