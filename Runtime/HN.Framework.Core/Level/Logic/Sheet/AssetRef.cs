using MemoryPack;
using System;

namespace HN.Framework.Core.Level.Logic.Sheet
{
    /// <summary>
    /// 配置表资产引用，存储 Addressables Label 字符串。
    /// Core 层只存数据（MemoryPack 序列化），Unity 侧通过扩展方法提供 LoadAssetAsync/ReleaseAsset。
    /// </summary>
    [MemoryPackable]
    public partial struct AssetRef<T> : IEquatable<AssetRef<T>>
    {
        [MemoryPackOrder(0)]
        public string Label;

        /// <summary>是否为有效引用（Label 非空）</summary>
        public bool IsValid => !string.IsNullOrEmpty(Label);

        /// <summary>空引用</summary>
        public static AssetRef<T> Empty => default;

        public bool Equals(AssetRef<T> other) => Label == other.Label;
        public override bool Equals(object obj) => obj is AssetRef<T> other && Equals(other);
        public override int GetHashCode() => Label?.GetHashCode() ?? 0;
        public static bool operator ==(AssetRef<T> left, AssetRef<T> right) => left.Equals(right);
        public static bool operator !=(AssetRef<T> left, AssetRef<T> right) => !left.Equals(right);
    }
}
