#nullable enable

using System;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 四元数，用于表示三维空间中的旋转。
    /// 纯 C# 实现，不依赖 UnityEngine 类型。
    /// </summary>
    public readonly struct PhysicsQuaternion : IEquatable<PhysicsQuaternion>
    {
        /// <summary>X 分量</summary>
        public readonly float X;

        /// <summary>Y 分量</summary>
        public readonly float Y;

        /// <summary>Z 分量</summary>
        public readonly float Z;

        /// <summary>W 分量</summary>
        public readonly float W;

        /// <summary>单位四元数，表示无旋转 (0, 0, 0, 1)</summary>
        public static readonly PhysicsQuaternion Identity = new PhysicsQuaternion(0f, 0f, 0f, 1f);

        /// <summary>
        /// 初始化四元数
        /// </summary>
        /// <param name="x">X 分量</param>
        /// <param name="y">Y 分量</param>
        /// <param name="z">Z 分量</param>
        /// <param name="w">W 分量</param>
        public PhysicsQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// 将欧拉角（弧度）转换为四元数，旋转顺序为 ZYX
        /// </summary>
        /// <param name="x">绕 X 轴旋转角度（弧度）</param>
        /// <param name="y">绕 Y 轴旋转角度（弧度）</param>
        /// <param name="z">绕 Z 轴旋转角度（弧度）</param>
        /// <returns>对应的四元数</returns>
        public static PhysicsQuaternion Euler(float x, float y, float z)
        {
            float hx = x * 0.5f;
            float hy = y * 0.5f;
            float hz = z * 0.5f;

            float sx = MathF.Sin(hx);
            float cx = MathF.Cos(hx);
            float sy = MathF.Sin(hy);
            float cy = MathF.Cos(hy);
            float sz = MathF.Sin(hz);
            float cz = MathF.Cos(hz);

            // ZYX 顺序: q = qx * qy * qz
            return new PhysicsQuaternion(
                cx * sy * sz + sx * cy * cz,
                cx * sy * cz - sx * cy * sz,
                cx * cy * sz + sx * sy * cz,
                cx * cy * cz - sx * sy * sz
            );
        }

        /// <summary>
        /// 绕指定轴旋转指定角度（弧度）创建四元数
        /// </summary>
        /// <param name="angle">旋转角度（弧度）</param>
        /// <param name="axis">旋转轴（自动归一化）</param>
        /// <returns>对应的四元数</returns>
        public static PhysicsQuaternion AngleAxis(float angle, PhysicsVector3 axis)
        {
            float halfAngle = angle * 0.5f;
            float s = MathF.Sin(halfAngle);
            float c = MathF.Cos(halfAngle);

            PhysicsVector3 n = axis.Normalized;
            return new PhysicsQuaternion(n.X * s, n.Y * s, n.Z * s, c);
        }

        /// <summary>
        /// 创建一个朝向指定前方向的旋转，上方向默认使用 (0, 1, 0)
        /// </summary>
        /// <param name="forward">目标前方向</param>
        /// <returns>对应的四元数</returns>
        public static PhysicsQuaternion LookRotation(PhysicsVector3 forward)
        {
            float sqrMag = forward.SqrMagnitude;
            if (sqrMag < 1e-10f)
            {
                return Identity;
            }

            PhysicsVector3 f = forward.Normalized;

            PhysicsVector3 worldUp = PhysicsVector3.Up;
            float dot = PhysicsVector3.Dot(f, worldUp);

            // 前方向与世界上方向平行时的特殊处理
            if (MathF.Abs(dot) > 0.9999f)
            {
                // 使用 Right 作为临时的上方向
                PhysicsVector3 right = PhysicsVector3.Right;
                PhysicsVector3 up = PhysicsVector3.Cross(right, f).Normalized;
                if (up.SqrMagnitude < 1e-10f)
                {
                    up = PhysicsVector3.Forward;
                }

                right = PhysicsVector3.Cross(f, up).Normalized;
                return FromRotationMatrix(right, up, f);
            }

            PhysicsVector3 r = PhysicsVector3.Cross(worldUp, f).Normalized;
            PhysicsVector3 u = PhysicsVector3.Cross(f, r).Normalized;

            return FromRotationMatrix(r, u, f);
        }

        /// <summary>
        /// 四元数乘法，表示将两个旋转组合
        /// </summary>
        /// <param name="lhs">第一个四元数</param>
        /// <param name="rhs">第二个四元数</param>
        /// <returns>组合后的四元数</returns>
        public static PhysicsQuaternion operator *(PhysicsQuaternion lhs, PhysicsQuaternion rhs)
        {
            return new PhysicsQuaternion(
                lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.W * rhs.Y - lhs.X * rhs.Z + lhs.Y * rhs.W + lhs.Z * rhs.X,
                lhs.W * rhs.Z + lhs.X * rhs.Y - lhs.Y * rhs.X + lhs.Z * rhs.W,
                lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z
            );
        }

        /// <summary>
        /// 判断两个四元数是否相等
        /// </summary>
        public static bool operator ==(PhysicsQuaternion left, PhysicsQuaternion right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 判断两个四元数是否不相等
        /// </summary>
        public static bool operator !=(PhysicsQuaternion left, PhysicsQuaternion right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// 判断当前实例是否等于指定对象
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is PhysicsQuaternion other && Equals(other);
        }

        /// <summary>
        /// 判断两个四元数是否相等（逐分量比较）
        /// </summary>
        public bool Equals(PhysicsQuaternion other)
        {
            const float epsilon = 1e-6f;
            return MathF.Abs(X - other.X) < epsilon
                   && MathF.Abs(Y - other.Y) < epsilon
                   && MathF.Abs(Z - other.Z) < epsilon
                   && MathF.Abs(W - other.W) < epsilon;
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
                hash = hash * 23 + W.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// 返回四元数的调试字符串
        /// </summary>
        public override string ToString()
        {
            return $"({X:F3}, {Y:F3}, {Z:F3}, {W:F3})";
        }

        /// <summary>
        /// 从旋转矩阵（三个正交轴）构造四元数
        /// </summary>
        private static PhysicsQuaternion FromRotationMatrix(
            PhysicsVector3 right,
            PhysicsVector3 up,
            PhysicsVector3 forward)
        {
            float trace = right.X + up.Y + forward.Z;

            if (trace > 0f)
            {
                float s = 0.5f / MathF.Sqrt(trace + 1f);
                return new PhysicsQuaternion(
                    (up.Z - forward.Y) * s,
                    (forward.X - right.Z) * s,
                    (right.Y - up.X) * s,
                    0.25f / s
                );
            }

            if (right.X > up.Y && right.X > forward.Z)
            {
                float s = 2f * MathF.Sqrt(1f + right.X - up.Y - forward.Z);
                return new PhysicsQuaternion(
                    0.25f * s,
                    (up.X + right.Y) / s,
                    (forward.X + right.Z) / s,
                    (up.Z - forward.Y) / s
                );
            }

            if (up.Y > forward.Z)
            {
                float s = 2f * MathF.Sqrt(1f + up.Y - right.X - forward.Z);
                return new PhysicsQuaternion(
                    (up.X + right.Y) / s,
                    0.25f * s,
                    (forward.Y + up.Z) / s,
                    (forward.X - right.Z) / s
                );
            }

            float s2 = 2f * MathF.Sqrt(1f + forward.Z - right.X - up.Y);
            return new PhysicsQuaternion(
                (forward.X + right.Z) / s2,
                (forward.Y + up.Z) / s2,
                0.25f * s2,
                (right.Y - up.X) / s2
            );
        }
    }
}
