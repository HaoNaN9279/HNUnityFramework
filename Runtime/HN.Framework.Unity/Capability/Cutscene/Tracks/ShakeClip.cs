using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class ShakeClip : PlayableAsset, ITimelineClipAsset
    {
        public ShakeBehaviour template = new ShakeBehaviour();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<ShakeBehaviour>.Create(graph, template);
        }
    }
}