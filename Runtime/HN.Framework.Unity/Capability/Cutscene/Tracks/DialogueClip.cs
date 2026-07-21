using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class DialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public DialogueBehaviour template = new DialogueBehaviour();

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<DialogueBehaviour>.Create(graph, template);
        }
    }
}