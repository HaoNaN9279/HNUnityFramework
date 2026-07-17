using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [TrackColor(0.9f, 0.4f, 0.4f)]
    [TrackClipType(typeof(TimelineEventClip))]
    [TrackBindingType(typeof(HN.Framework.Core.Capability.Event.IEventBus))]
    public class TimelineEventTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TimelineEventBehaviour>.Create(graph, inputCount);
        }
    }
}