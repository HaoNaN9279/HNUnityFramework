using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class TimelineEventClip : PlayableAsset, ITimelineClipAsset
    {
        public TimelineEventBehaviour template = new TimelineEventBehaviour();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<TimelineEventBehaviour>.Create(graph, template);
        }
    }
}