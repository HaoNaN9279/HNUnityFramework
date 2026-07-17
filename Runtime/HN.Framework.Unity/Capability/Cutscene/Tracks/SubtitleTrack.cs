using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [TrackColor(0.8f, 0.9f, 0.3f)]
    [TrackClipType(typeof(SubtitleClip))]
    [TrackBindingType(typeof(CutsceneSubtitleDisplay))]
    public class SubtitleTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<SubtitleBehaviour>.Create(graph, inputCount);
        }
    }
}