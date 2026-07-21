using UnityEngine;
using HN.Framework.Core.Level.Logic.AI.KnowledgePool;

namespace HN.Framework.Unity.Level.Logic.AI
{
    public class AudioSensor : MonoBehaviour, IPerceptionSensor
    {
        public string SensorName => "Audio";
        public float DetectionRange { get => _hearingRange; set => _hearingRange = value; }
        public bool IsEnabled { get => enabled; set => enabled = value; }

        [SerializeField] private float _hearingRange = 15f;

        public void Sense(PerceptionState state)
        {
            // Audio perception handled externally via event system
        }
    }
}
