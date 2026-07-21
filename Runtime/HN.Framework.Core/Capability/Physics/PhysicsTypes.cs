#nullable enable

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 物理维度
    /// </summary>
    public enum PhysicsDimension
    {
        /// <summary>二维物理</summary>
        D2,

        /// <summary>三维物理</summary>
        D3,
    }

    /// <summary>
    /// 刚体类型
    /// </summary>
    public enum BodyType
    {
        /// <summary>静态刚体，不受力影响，位置固定</summary>
        Static,

        /// <summary>动态刚体，受力和重力影响</summary>
        Dynamic,

        /// <summary>运动学刚体，可通过代码控制位置，不受力影响</summary>
        Kinematic,
    }
}
