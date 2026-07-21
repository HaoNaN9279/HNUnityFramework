#nullable enable

using System;
using HN.Framework.Core.Capability.Physics;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Physics
{
    public sealed class UnityBody : MonoBehaviour, IBody
    {
        private Rigidbody? _rigidbody;
        private Rigidbody2D? _rigidbody2D;
        private PhysicsDimension _dimension = PhysicsDimension.D3;

        private Action<CollisionEvent>? _collisionEnter;
        private Action<CollisionEvent>? _collisionStay;
        private Action<CollisionEvent>? _collisionExit;
        private Action<CollisionEvent>? _triggerEnter;
        private Action<CollisionEvent>? _triggerStay;
        private Action<CollisionEvent>? _triggerExit;

        event Action<CollisionEvent>? IBody.OnCollisionEnter { add => _collisionEnter += value; remove => _collisionEnter -= value; }
        event Action<CollisionEvent>? IBody.OnCollisionStay { add => _collisionStay += value; remove => _collisionStay -= value; }
        event Action<CollisionEvent>? IBody.OnCollisionExit { add => _collisionExit += value; remove => _collisionExit -= value; }
        event Action<CollisionEvent>? IBody.OnTriggerEnter { add => _triggerEnter += value; remove => _triggerEnter -= value; }
        event Action<CollisionEvent>? IBody.OnTriggerStay { add => _triggerStay += value; remove => _triggerStay -= value; }
        event Action<CollisionEvent>? IBody.OnTriggerExit { add => _triggerExit += value; remove => _triggerExit -= value; }

        internal PhysicsDimension Dimension { get => _dimension; set { _dimension = value; DetectRigidbody(); } }

        private void Awake() { DetectRigidbody(); }

        private void DetectRigidbody()
        {
            if (_dimension == PhysicsDimension.D2)
            {
                _rigidbody2D = GetComponent<Rigidbody2D>();
                _rigidbody = null;
            }
            else
            {
                _rigidbody = GetComponent<Rigidbody>();
                _rigidbody2D = null;
            }
        }

        private Rigidbody GetRigidbody()
        {
            if (_rigidbody == null) DetectRigidbody();
            return _rigidbody ?? throw new InvalidOperationException("UnityBody: Rigidbody not found");
        }

        private Rigidbody2D GetRigidbody2D()
        {
            if (_rigidbody2D == null) DetectRigidbody();
            return _rigidbody2D ?? throw new InvalidOperationException("UnityBody: Rigidbody2D not found");
        }

        public PhysicsVector3 Position
        {
            get
            {
                if (_dimension == PhysicsDimension.D2)
                    return FromUnityVector2(GetRigidbody2D().position);
                return FromUnityVector3(GetRigidbody().position);
            }
            set
            {
                if (_dimension == PhysicsDimension.D2)
                    GetRigidbody2D().position = ToUnityVector2(value);
                else
                    GetRigidbody().position = ToUnityVector3(value);
            }
        }

        public PhysicsQuaternion Rotation
        {
            get
            {
                if (_dimension == PhysicsDimension.D2)
                    return FromUnityRotation2D(GetRigidbody2D().rotation);
                return FromUnityQuaternion(GetRigidbody().rotation);
            }
            set
            {
                if (_dimension == PhysicsDimension.D2)
                    GetRigidbody2D().rotation = ToUnityRotation2D(value);
                else
                    GetRigidbody().rotation = ToUnityQuaternion(value);
            }
        }

        public PhysicsVector3 Velocity
        {
            get
            {
                if (_dimension == PhysicsDimension.D2)
                    return FromUnityVector2(GetRigidbody2D().velocity);
                return FromUnityVector3(GetRigidbody().velocity);
            }
            set
            {
                if (_dimension == PhysicsDimension.D2)
                    GetRigidbody2D().velocity = ToUnityVector2(value);
                else
                    GetRigidbody().velocity = ToUnityVector3(value);
            }
        }

        public PhysicsVector3 AngularVelocity
        {
            get
            {
                if (_dimension == PhysicsDimension.D2)
                    return FromUnityAngularVelocity2D(GetRigidbody2D().angularVelocity);
                return FromUnityVector3(GetRigidbody().angularVelocity);
            }
            set
            {
                if (_dimension == PhysicsDimension.D2)
                    GetRigidbody2D().angularVelocity = ToUnityAngularVelocity2D(value);
                else
                    GetRigidbody().angularVelocity = ToUnityVector3(value);
            }
        }

        public float Mass
        {
            get => _dimension == PhysicsDimension.D2 ? GetRigidbody2D().mass : GetRigidbody().mass;
            set { if (_dimension == PhysicsDimension.D2) GetRigidbody2D().mass = value; else GetRigidbody().mass = value; }
        }

        public bool IsKinematic
        {
            get => _dimension == PhysicsDimension.D2 ? GetRigidbody2D().isKinematic : GetRigidbody().isKinematic;
            set { if (_dimension == PhysicsDimension.D2) GetRigidbody2D().isKinematic = value; else GetRigidbody().isKinematic = value; }
        }

        public BodyType BodyType
        {
            get
            {
                if (_dimension == PhysicsDimension.D2)
                {
                    var rb = GetRigidbody2D();
                    if (rb.isKinematic) return BodyType.Kinematic;
                    if (rb.bodyType == RigidbodyType2D.Static) return BodyType.Static;
                    return BodyType.Dynamic;
                }
                else
                {
                    var rb = GetRigidbody();
                    if (rb.isKinematic) return BodyType.Kinematic;
                    return BodyType.Dynamic;
                }
            }
            set
            {
                if (_dimension == PhysicsDimension.D2)
                {
                    var rb = GetRigidbody2D();
                    switch (value)
                    {
                        case BodyType.Static: rb.bodyType = RigidbodyType2D.Static; break;
                        case BodyType.Kinematic: rb.bodyType = RigidbodyType2D.Kinematic; break;
                        case BodyType.Dynamic: rb.bodyType = RigidbodyType2D.Dynamic; break;
                    }
                }
                else
                {
                    var rb = GetRigidbody();
                    rb.isKinematic = value == BodyType.Kinematic;
                }
            }
        }

        public int InstanceId => gameObject.GetInstanceID();

        public void AddForce(PhysicsVector3 force)
        {
            if (_dimension == PhysicsDimension.D2)
                GetRigidbody2D().AddForce(ToUnityVector2(force));
            else
                GetRigidbody().AddForce(ToUnityVector3(force));
        }

        public void AddTorque(PhysicsVector3 torque)
        {
            if (_dimension == PhysicsDimension.D2)
                GetRigidbody2D().AddTorque(ToUnityAngularVelocity2D(torque));
            else
                GetRigidbody().AddTorque(ToUnityVector3(torque));
        }

        public void Dispose()
        {
            if (this != null && gameObject != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(gameObject);
                else
                    Destroy(gameObject);
#else
                Destroy(gameObject);
#endif
            }
        }

        private void OnCollisionEnter(Collision c) { _collisionEnter?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), c.gameObject.GetInstanceID(), FromUnityVector3(c.relativeVelocity), CollisionEventType.Enter)); }
        private void OnCollisionStay(Collision c) { _collisionStay?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), c.gameObject.GetInstanceID(), FromUnityVector3(c.relativeVelocity), CollisionEventType.Stay)); }
        private void OnCollisionExit(Collision c) { _collisionExit?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), c.gameObject.GetInstanceID(), FromUnityVector3(c.relativeVelocity), CollisionEventType.Exit)); }
        private void OnTriggerEnter(Collider o) { _triggerEnter?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), o.gameObject.GetInstanceID(), PhysicsVector3.Zero, CollisionEventType.Enter)); }
        private void OnTriggerStay(Collider o) { _triggerStay?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), o.gameObject.GetInstanceID(), PhysicsVector3.Zero, CollisionEventType.Stay)); }
        private void OnTriggerExit(Collider o) { _triggerExit?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), o.gameObject.GetInstanceID(), PhysicsVector3.Zero, CollisionEventType.Exit)); }
        private void OnCollisionEnter2D(Collision2D c) { _collisionEnter?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), c.gameObject.GetInstanceID(), FromUnityVector2(c.relativeVelocity), CollisionEventType.Enter)); }
        private void OnCollisionStay2D(Collision2D c) { _collisionStay?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), c.gameObject.GetInstanceID(), FromUnityVector2(c.relativeVelocity), CollisionEventType.Stay)); }
        private void OnCollisionExit2D(Collision2D c) { _collisionExit?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), c.gameObject.GetInstanceID(), FromUnityVector2(c.relativeVelocity), CollisionEventType.Exit)); }
        private void OnTriggerEnter2D(Collider2D o) { _triggerEnter?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), o.gameObject.GetInstanceID(), PhysicsVector3.Zero, CollisionEventType.Enter)); }
        private void OnTriggerStay2D(Collider2D o) { _triggerStay?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), o.gameObject.GetInstanceID(), PhysicsVector3.Zero, CollisionEventType.Stay)); }
        private void OnTriggerExit2D(Collider2D o) { _triggerExit?.Invoke(new CollisionEvent(gameObject.GetInstanceID(), o.gameObject.GetInstanceID(), PhysicsVector3.Zero, CollisionEventType.Exit)); }

        internal static Vector3 ToUnityVector3(PhysicsVector3 v) => new Vector3(v.X, v.Y, v.Z);
        internal static PhysicsVector3 FromUnityVector3(Vector3 v) => new PhysicsVector3(v.x, v.y, v.z);
        internal static Vector2 ToUnityVector2(PhysicsVector3 v) => new Vector2(v.X, v.Y);
        internal static PhysicsVector3 FromUnityVector2(Vector2 v) => new PhysicsVector3(v.x, v.y, 0f);
        internal static Quaternion ToUnityQuaternion(PhysicsQuaternion q) => new Quaternion(q.X, q.Y, q.Z, q.W);
        internal static PhysicsQuaternion FromUnityQuaternion(Quaternion q) => new PhysicsQuaternion(q.x, q.y, q.z, q.w);
        internal static PhysicsQuaternion FromUnityRotation2D(float d) { float r = d * MathF.PI / 180f * 0.5f; return new PhysicsQuaternion(0f, 0f, MathF.Sin(r), MathF.Cos(r)); }
        internal static float ToUnityRotation2D(PhysicsQuaternion q) => MathF.Atan2(q.Z, q.W) * 2f * 180f / MathF.PI;
        internal static PhysicsVector3 FromUnityAngularVelocity2D(float a) => new PhysicsVector3(0f, 0f, a);
        internal static float ToUnityAngularVelocity2D(PhysicsVector3 v) => v.Z;
    }
}
