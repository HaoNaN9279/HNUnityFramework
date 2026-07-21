#nullable enable

using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 碰撞形状定义的抽象基类
    /// </summary>
    public abstract class ShapeDefinition : IReference
    {
        /// <summary>
        /// 重置形状到默认值
        /// </summary>
        public abstract void Clear();
    }

    /// <summary>
    /// 盒体碰撞形状定义
    /// </summary>
    public sealed class BoxShape : ShapeDefinition
    {
        /// <summary>
        /// 盒体的半尺寸（从中心到每个面的距离）
        /// </summary>
        public PhysicsVector3 HalfExtents { get; set; }

        /// <summary>
        /// 初始化盒体形状，默认半尺寸为 0.5
        /// </summary>
        public BoxShape()
        {
            HalfExtents = new PhysicsVector3(0.5f, 0.5f, 0.5f);
        }

        /// <summary>
        /// 初始化盒体形状
        /// </summary>
        /// <param name="halfExtents">半尺寸</param>
        public BoxShape(PhysicsVector3 halfExtents)
        {
            HalfExtents = halfExtents;
        }

        /// <summary>
        /// 重置盒体形状到默认值
        /// </summary>
        public override void Clear()
        {
            HalfExtents = new PhysicsVector3(0.5f, 0.5f, 0.5f);
        }
    }

    /// <summary>
    /// 球体碰撞形状定义
    /// </summary>
    public sealed class SphereShape : ShapeDefinition
    {
        /// <summary>
        /// 球体半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 初始化球体形状，默认半径为 0.5
        /// </summary>
        public SphereShape()
        {
            Radius = 0.5f;
        }

        /// <summary>
        /// 初始化球体形状
        /// </summary>
        /// <param name="radius">半径</param>
        public SphereShape(float radius)
        {
            Radius = radius;
        }

        /// <summary>
        /// 重置球体形状到默认值
        /// </summary>
        public override void Clear()
        {
            Radius = 0.5f;
        }
    }

    /// <summary>
    /// 胶囊体碰撞形状定义
    /// </summary>
    public sealed class CapsuleShape : ShapeDefinition
    {
        /// <summary>
        /// 胶囊体半径
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// 胶囊体高度（包含两个半球的总高度）
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// 初始化胶囊体形状，默认半径 0.5，高度 2
        /// </summary>
        public CapsuleShape()
        {
            Radius = 0.5f;
            Height = 2f;
        }

        /// <summary>
        /// 初始化胶囊体形状
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="height">高度（包含两个半球的总高度）</param>
        public CapsuleShape(float radius, float height)
        {
            Radius = radius;
            Height = height;
        }

        /// <summary>
        /// 重置胶囊体形状到默认值
        /// </summary>
        public override void Clear()
        {
            Radius = 0.5f;
            Height = 2f;
        }
    }
}
