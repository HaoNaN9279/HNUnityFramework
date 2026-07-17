using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class SubtitleClip : PlayableAsset, ITimelineClipAsset
    {
        public SubtitleBehaviour template = new SubtitleBehaviour();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<SubtitleBehaviour>.Create(graph, template);
        }
    }
}