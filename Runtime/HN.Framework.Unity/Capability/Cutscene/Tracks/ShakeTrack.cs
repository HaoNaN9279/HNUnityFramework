using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using HN.Framework.Core.Capability.Camera;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [TrackColor(0.9f, 0.6f, 0.1f)]
    [TrackClipType(typeof(ShakeClip))]
    [TrackBindingType(typeof(ICameraManager))]
    public class ShakeTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<ShakeBehaviour>.Create(graph, inputCount);
        }
    }
}