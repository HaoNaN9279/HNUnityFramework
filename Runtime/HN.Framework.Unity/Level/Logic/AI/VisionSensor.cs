using UnityEngine;
using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

namespace HN.Framework.Unity.Level.Logic.AI
{
    public class VisionSensor : MonoBehaviour, IPerceptionSensor
    {
        public string SensorName => "Vision";
        public float DetectionRange { get => _viewDistance; set => _viewDistance = value; }
        public bool IsEnabled { get => enabled; set => enabled = value; }

        [SerializeField] private float _viewDistance = 10f;
        [SerializeField] private float _viewAngle = 90f;
        [SerializeField] private LayerMask _targetLayers;

        public void Sense(PerceptionState state)
        {
            var hits = Physics.OverlapSphere(transform.position, _viewDistance, _targetLayers);
            foreach (var hit in hits)
            {
                Vector3 dir = hit.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= _viewAngle * 0.5f)
                {
                    float dist = dir.magnitude;
                    state.AddVisibleTarget(hit.GetInstanceID(), 1f, dist,
                        new Vector3Data(hit.transform.position.x, hit.transform.position.y, hit.transform.position.z));
                }
            }
        }
    }
}
