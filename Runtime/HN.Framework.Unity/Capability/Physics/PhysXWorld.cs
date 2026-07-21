#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common;
using UnityEngine;

using PhysicsHit = HN.Framework.Core.Capability.Physics.RaycastHit;

namespace HN.Framework.Unity.Capability.Physics
{
    public sealed class PhysXWorld : IPhysicsWorld, ITickable
    {
        private readonly PhysicsDimension _dimension;
        private readonly GameObject _container;
        private readonly List<UnityBody> _bodies = new();

        public PhysXWorld(PhysicsDimension dimension)
        {
            _dimension = dimension;
            _container = new GameObject("PhysXWorld");
            _container.hideFlags = HideFlags.HideAndDontSave;
#if !UNITY_EDITOR
            UnityEngine.Object.DontDestroyOnLoad(_container);
#endif
        }

        public PhysicsDimension Dimension => _dimension;
        public void Tick() { }
        public void LateTick() { }

        public IBody CreateBody(ShapeDefinition shape, PhysicsVector3 position, PhysicsQuaternion rotation, BodyType bodyType, string layer)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            var go = new GameObject("PhysicsBody");
            go.transform.SetParent(_container.transform);
            go.hideFlags = HideFlags.HideAndDontSave;
            int layerId = LayerMask.NameToLayer(layer);
            go.layer = layerId >= 0 ? layerId : LayerMask.NameToLayer("Default");

            UnityBody body;
            if (_dimension == PhysicsDimension.D2)
            {
                var rb = go.AddComponent<Rigidbody2D>();
                body = go.AddComponent<UnityBody>();
                body.Dimension = PhysicsDimension.D2;
                rb.position = UnityBody.ToUnityVector2(position);
                rb.rotation = UnityBody.ToUnityRotation2D(rotation);
                SetBodyType2D(rb, bodyType);
                AddCollider2D(go, shape);
            }
            else
            {
                var rb = go.AddComponent<Rigidbody>();
                body = go.AddComponent<UnityBody>();
                body.Dimension = PhysicsDimension.D3;
                rb.position = UnityBody.ToUnityVector3(position);
                rb.rotation = UnityBody.ToUnityQuaternion(rotation);
                SetBodyType3D(rb, bodyType);
                AddCollider3D(go, shape);
            }
            _bodies.Add(body);
            return body;
        }

        public void DestroyBody(IBody body)
        {
            if (body is UnityBody ub) _bodies.Remove(ub);
            body?.Dispose();
        }

        public bool Raycast(PhysicsVector3 origin, PhysicsVector3 direction, out PhysicsHit hitInfo, float maxDistance, string layerMask)
        {
            hitInfo = default;
            int mask = LayerMask.GetMask(layerMask);

            if (_dimension == PhysicsDimension.D2)
            {
                var hit2D = UnityEngine.Physics2D.Raycast(UnityBody.ToUnityVector2(origin), UnityBody.ToUnityVector2(direction), maxDistance, mask);
                if (hit2D.collider != null)
                {
                    hitInfo = new PhysicsHit(UnityBody.FromUnityVector2(hit2D.point), UnityBody.FromUnityVector2(hit2D.normal), hit2D.distance, hit2D.collider.GetInstanceID());
                    return true;
                }
            }
            else
            {
                if (UnityEngine.Physics.Raycast(UnityBody.ToUnityVector3(origin), UnityBody.ToUnityVector3(direction), out UnityEngine.RaycastHit hit3D, maxDistance, mask))
                {
                    hitInfo = new PhysicsHit(UnityBody.FromUnityVector3(hit3D.point), UnityBody.FromUnityVector3(hit3D.normal), hit3D.distance, hit3D.collider.GetInstanceID());
                    return true;
                }
            }
            return false;
        }

        public PhysicsHit[] RaycastAll(PhysicsVector3 origin, PhysicsVector3 direction, float maxDistance, string layerMask)
        {
            int mask = LayerMask.GetMask(layerMask);

            if (_dimension == PhysicsDimension.D2)
            {
                var hits = UnityEngine.Physics2D.RaycastAll(UnityBody.ToUnityVector2(origin), UnityBody.ToUnityVector2(direction), maxDistance, mask);
                var results = new PhysicsHit[hits.Length];
                for (int i = 0; i < hits.Length; i++)
                    results[i] = new PhysicsHit(UnityBody.FromUnityVector2(hits[i].point), UnityBody.FromUnityVector2(hits[i].normal), hits[i].distance, hits[i].collider.GetInstanceID());
                return results;
            }
            else
            {
                var hits = UnityEngine.Physics.RaycastAll(UnityBody.ToUnityVector3(origin), UnityBody.ToUnityVector3(direction), maxDistance, mask);
                var results = new PhysicsHit[hits.Length];
                for (int i = 0; i < hits.Length; i++)
                    results[i] = new PhysicsHit(UnityBody.FromUnityVector3(hits[i].point), UnityBody.FromUnityVector3(hits[i].normal), hits[i].distance, hits[i].collider.GetInstanceID());
                return results;
            }
        }

        public PhysicsHit[] OverlapSphere(PhysicsVector3 center, float radius, string layerMask)
        {
            int mask = LayerMask.GetMask(layerMask);

            if (_dimension == PhysicsDimension.D2)
            {
                var cols = UnityEngine.Physics2D.OverlapCircleAll(UnityBody.ToUnityVector2(center), radius, mask);
                var results = new PhysicsHit[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    var cp = cols[i].ClosestPoint(UnityBody.ToUnityVector2(center));
                    float dist = Vector2.Distance(UnityBody.ToUnityVector2(center), cp);
                    results[i] = new PhysicsHit(UnityBody.FromUnityVector2(cp), PhysicsVector3.Zero, dist, cols[i].GetInstanceID());
                }
                return results;
            }
            else
            {
                var cols = UnityEngine.Physics.OverlapSphere(UnityBody.ToUnityVector3(center), radius, mask);
                var results = new PhysicsHit[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    var cp = cols[i].ClosestPoint(UnityBody.ToUnityVector3(center));
                    float dist = Vector3.Distance(UnityBody.ToUnityVector3(center), cp);
                    results[i] = new PhysicsHit(UnityBody.FromUnityVector3(cp), PhysicsVector3.Zero, dist, cols[i].GetInstanceID());
                }
                return results;
            }
        }

        public PhysicsHit[] OverlapBox(PhysicsVector3 center, PhysicsVector3 halfExtents, PhysicsQuaternion rotation, string layerMask)
        {
            int mask = LayerMask.GetMask(layerMask);

            if (_dimension == PhysicsDimension.D2)
            {
                float angle = UnityBody.ToUnityRotation2D(rotation);
                var cols = UnityEngine.Physics2D.OverlapBoxAll(UnityBody.ToUnityVector2(center), UnityBody.ToUnityVector2(halfExtents) * 2f, angle, mask);
                var results = new PhysicsHit[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    float d = Vector2.Distance(UnityBody.ToUnityVector2(center), cols[i].bounds.center);
                    results[i] = new PhysicsHit(UnityBody.FromUnityVector3(cols[i].bounds.center), PhysicsVector3.Zero, d, cols[i].GetInstanceID());
                }
                return results;
            }
            else
            {
                var cols = UnityEngine.Physics.OverlapBox(UnityBody.ToUnityVector3(center), UnityBody.ToUnityVector3(halfExtents), UnityBody.ToUnityQuaternion(rotation), mask);
                var results = new PhysicsHit[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    var cp = cols[i].ClosestPoint(UnityBody.ToUnityVector3(center));
                    float dist = Vector3.Distance(UnityBody.ToUnityVector3(center), cp);
                    results[i] = new PhysicsHit(UnityBody.FromUnityVector3(cp), PhysicsVector3.Zero, dist, cols[i].GetInstanceID());
                }
                return results;
            }
        }

        public void Step(float deltaTime)
        {
            if (_dimension == PhysicsDimension.D2) UnityEngine.Physics2D.Simulate(deltaTime);
            else UnityEngine.Physics.Simulate(deltaTime);
        }

        public void Dispose()
        {
            for (int i = _bodies.Count - 1; i >= 0; i--) _bodies[i].Dispose();
            _bodies.Clear();
            if (_container != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEngine.Object.DestroyImmediate(_container);
                else
                    UnityEngine.Object.Destroy(_container);
#else
                UnityEngine.Object.Destroy(_container);
#endif
            }
        }

        private static void SetBodyType3D(Rigidbody rb, BodyType t)
        {
            switch (t)
            {
                case BodyType.Static: rb.isKinematic = true; rb.useGravity = false; break;
                case BodyType.Dynamic: rb.isKinematic = false; rb.useGravity = true; break;
                case BodyType.Kinematic: rb.isKinematic = true; rb.useGravity = false; break;
            }
        }

        private static void SetBodyType2D(Rigidbody2D rb, BodyType t)
        {
            switch (t)
            {
                case BodyType.Static: rb.bodyType = RigidbodyType2D.Static; rb.gravityScale = 0f; break;
                case BodyType.Dynamic: rb.bodyType = RigidbodyType2D.Dynamic; rb.gravityScale = 1f; break;
                case BodyType.Kinematic: rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0f; break;
            }
        }

        private static void AddCollider3D(GameObject go, ShapeDefinition s)
        {
            switch (s)
            {
                case BoxShape b: var bc = go.AddComponent<BoxCollider>(); bc.size = UnityBody.ToUnityVector3(b.HalfExtents) * 2f; break;
                case SphereShape sp: var sc = go.AddComponent<SphereCollider>(); sc.radius = sp.Radius; break;
                case CapsuleShape ca: var cc = go.AddComponent<CapsuleCollider>(); cc.radius = ca.Radius; cc.height = ca.Height; break;
                default: throw new ArgumentException($"Unsupported shape: {s.GetType().Name}");
            }
        }

        private static void AddCollider2D(GameObject go, ShapeDefinition s)
        {
            switch (s)
            {
                case BoxShape b: var bc = go.AddComponent<BoxCollider2D>(); bc.size = UnityBody.ToUnityVector2(b.HalfExtents) * 2f; break;
                case SphereShape sp: var sc = go.AddComponent<CircleCollider2D>(); sc.radius = sp.Radius; break;
                case CapsuleShape ca: var cc = go.AddComponent<CapsuleCollider2D>(); cc.size = new Vector2(ca.Radius * 2f, ca.Height); break;
                default: throw new ArgumentException($"Unsupported shape: {s.GetType().Name}");
            }
        }
    }
}
