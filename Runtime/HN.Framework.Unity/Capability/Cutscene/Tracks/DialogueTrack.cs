using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [TrackColor(0.3f, 0.6f, 0.9f)]
    [TrackClipType(typeof(DialogueClip))]
    [TrackBindingType(typeof(HN.Framework.Core.Capability.Event.IEventBus))]
    public class DialogueTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<DialogueBehaviour>.Create(graph, inputCount);
        }
    }
}