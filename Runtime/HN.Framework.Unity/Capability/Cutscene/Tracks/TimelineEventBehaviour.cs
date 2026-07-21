using System;
using HN.Framework.Core.Capability.Cutscene;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class TimelineEventBehaviour : PlayableBehaviour
    {
        public string EventType;
        public string EventData;

        private bool _triggered;

        public override void OnGraphStart(Playable playable)
        {
            _triggered = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_triggered) return;

            var eventBus = playerData as HN.Framework.Core.Capability.Event.IEventBus;
            if (eventBus != null)
            {
                var cutsceneEvent = new CutsceneEvent(EventType, string.Empty, playable.GetTime(), EventData);
                eventBus.Publish(cutsceneEvent);
                _triggered = true;
            }
        }
    }
}