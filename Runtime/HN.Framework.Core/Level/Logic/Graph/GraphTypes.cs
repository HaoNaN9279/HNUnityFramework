#nullable enable

using MemoryPack;
using System;

namespace HN.Framework.Core.Level.Logic.Graph
{
    /// <summary>
    /// 图端口方向。
    /// </summary>
    public enum GraphPortDirection : byte
    {
        /// <summary>输入端口，接收来自其他节点输出的连接。</summary>
        Input = 0,

        /// <summary>输出端口，向其他节点输入发起连接。</summary>
        Output = 1,
    }

    /// <summary>
    /// 图数据模型使用的二维向量（位置 / 尺寸）。
    /// Core 层不依赖 UnityEngine，因此使用独立的轻量值类型存储编辑器布局坐标。
    /// </summary>
    [MemoryPackable]
    public partial struct GraphVector2 : IEquatable<GraphVector2>
    {
        [MemoryPackOrder(0)]
        public float X { get; set; }

        [MemoryPackOrder(1)]
        public float Y { get; set; }

        /// <summary>零向量。</summary>
        public static GraphVector2 Zero => default;

        /// <summary>
        /// 初始化二维向量。
        /// </summary>
        /// <param name="x">X 分量。</param>
        /// <param name="y">Y 分量。</param>
        public GraphVector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc />
        public bool Equals(GraphVector2 other) => X == other.X && Y == other.Y;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is GraphVector2 other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>相等运算符。</summary>
        public static bool operator ==(GraphVector2 left, GraphVector2 right) => left.Equals(right);

        /// <summary>不等运算符。</summary>
        public static bool operator !=(GraphVector2 left, GraphVector2 right) => !left.Equals(right);

        /// <inheritdoc />
        public override string ToString() => $"({X}, {Y})";
    }
}
