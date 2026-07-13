#nullable enable

using System;

namespace HN.Framework.Core.Capability.Physics
{
    /// <summary>
    /// 物理刚体接口，表示物理世界中的一个独立刚体。
    /// 提供位置、旋转、速度等属性和力/扭矩操作，以及碰撞/触发事件。
    /// </summary>
    public interface IBody : IDisposable
    {
        /// <summary>
        /// 刚体的世界坐标位置
        /// </summary>
        PhysicsVector3 Position { get; set; }

        /// <summary>
        /// 刚体的世界坐标旋转
        /// </summary>
        PhysicsQuaternion Rotation { get; set; }

        /// <summary>
        /// 刚体的线速度
        /// </summary>
        PhysicsVector3 Velocity { get; set; }

        /// <summary>
        /// 刚体的角速度
        /// </summary>
        PhysicsVector3 AngularVelocity { get; set; }

        /// <summary>
        /// 刚体质量（单位：千克）
        /// </summary>
        float Mass { get; set; }

        /// <summary>
        /// 是否为运动学刚体（不受力影响，但可参与碰撞检测）
        /// </summary>
        bool IsKinematic { get; set; }

        /// <summary>
        /// 刚体类型
        /// </summary>
        BodyType BodyType { get; set; }

        /// <summary>
        /// 刚体实例的唯一标识 ID
        /// </summary>
        int InstanceId { get; }

        /// <summary>
        /// 对刚体施加力
        /// </summary>
        /// <param name="force">力向量（方向和大小）</param>
        void AddForce(PhysicsVector3 force);

        /// <summary>
        /// 对刚体施加扭矩
        /// </summary>
        /// <param name="torque">扭矩向量</param>
        void AddTorque(PhysicsVector3 torque);

        /// <summary>
        /// 碰撞进入事件
        /// </summary>
        event Action<CollisionEvent>? OnCollisionEnter;

        /// <summary>
        /// 碰撞持续事件
        /// </summary>
        event Action<CollisionEvent>? OnCollisionStay;

        /// <summary>
        /// 碰撞退出事件
        /// </summary>
        event Action<CollisionEvent>? OnCollisionExit;

        /// <summary>
        /// 触发进入事件
        /// </summary>
        event Action<CollisionEvent>? OnTriggerEnter;

        /// <summary>
        /// 触发持续事件
        /// </summary>
        event Action<CollisionEvent>? OnTriggerStay;

        /// <summary>
        /// 触发退出事件
        /// </summary>
        event Action<CollisionEvent>? OnTriggerExit;
    }
}
