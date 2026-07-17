using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [TrackColor(0.4f, 0.8f, 0.4f)]
    [TrackClipType(typeof(GameStateClip))]
    [TrackBindingType(typeof(GameObject))]
    public class GameStateTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<GameStateBehaviour>.Create(graph, inputCount);
        }
    }
}