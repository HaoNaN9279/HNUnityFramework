#nullable enable

using System;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 物理世界接口，负责管理所有物理刚体的生命周期以及执行物理查询。
    /// </summary>
    public interface IPhysicsWorld : IDisposable
    {
        /// <summary>
        /// 在物理世界中创建一个刚体
        /// </summary>
        /// <param name="shape">碰撞形状定义</param>
        /// <param name="position">初始位置</param>
        /// <param name="rotation">初始旋转</param>
        /// <param name="bodyType">刚体类型</param>
        /// <param name="layer">物理层名称</param>
        /// <returns>创建的刚体实例</returns>
        IBody CreateBody(ShapeDefinition shape, PhysicsVector3 position, PhysicsQuaternion rotation, BodyType bodyType, string layer);

        /// <summary>
        /// 从物理世界中销毁一个刚体
        /// </summary>
        /// <param name="body">要销毁的刚体实例</param>
        void DestroyBody(IBody body);

        /// <summary>
        /// 执行单次射线检测，返回第一个命中结果
        /// </summary>
        /// <param name="origin">射线起点（世界坐标）</param>
        /// <param name="direction">射线方向（应归一化）</param>
        /// <param name="hitInfo">命中结果（输出参数）</param>
        /// <param name="maxDistance">最大检测距离</param>
        /// <param name="layerMask">物理层掩码名称</param>
        /// <returns>是否有命中</returns>
        bool Raycast(PhysicsVector3 origin, PhysicsVector3 direction, out RaycastHit hitInfo, float maxDistance, string layerMask);

        /// <summary>
        /// 执行射线检测，返回所有命中结果
        /// </summary>
        /// <param name="origin">射线起点（世界坐标）</param>
        /// <param name="direction">射线方向（应归一化）</param>
        /// <param name="maxDistance">最大检测距离</param>
        /// <param name="layerMask">物理层掩码名称</param>
        /// <returns>所有命中结果数组（按距离排序，从近到远）</returns>
        RaycastHit[] RaycastAll(PhysicsVector3 origin, PhysicsVector3 direction, float maxDistance, string layerMask);

        /// <summary>
        /// 球体范围检测，返回球体内所有碰撞体的命中结果
        /// </summary>
        /// <param name="center">球心世界坐标</param>
        /// <param name="radius">球体半径</param>
        /// <param name="layerMask">物理层掩码名称</param>
        /// <returns>所有命中结果数组</returns>
        RaycastHit[] OverlapSphere(PhysicsVector3 center, float radius, string layerMask);

        /// <summary>
        /// 盒体范围检测，返回盒体内所有碰撞体的命中结果
        /// </summary>
        /// <param name="center">盒体中心世界坐标</param>
        /// <param name="halfExtents">盒体半尺寸</param>
        /// <param name="rotation">盒体旋转</param>
        /// <param name="layerMask">物理层掩码名称</param>
        /// <returns>所有命中结果数组</returns>
        RaycastHit[] OverlapBox(PhysicsVector3 center, PhysicsVector3 halfExtents, PhysicsQuaternion rotation, string layerMask);

        /// <summary>
        /// 推进物理模拟一步
        /// </summary>
        /// <param name="deltaTime">时间步长（秒）</param>
        void Step(float deltaTime);

        /// <summary>
        /// 物理世界的维度（二维或三维）
        /// </summary>
        PhysicsDimension Dimension { get; }
    }
}
