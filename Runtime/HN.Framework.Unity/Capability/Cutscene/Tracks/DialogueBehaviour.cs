using System;
using HN.Framework.Core.Capability.Event;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class DialogueBehaviour : PlayableBehaviour
    {
        public string DialogueId;

        private bool _triggered;

        public override void OnGraphStart(Playable playable)
        {
            _triggered = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_triggered) return;

            var eventBus = playerData as IEventBus;
            if (eventBus != null)
            {
                eventBus.Publish(new DialogueTriggerEvent
                {
                    DialogueId = DialogueId,
                    Timestamp = playable.GetTime()
                });
                _triggered = true;
            }
        }
    }
}