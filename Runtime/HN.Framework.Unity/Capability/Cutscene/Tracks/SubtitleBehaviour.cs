using System;
using UnityEngine;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class SubtitleBehaviour : PlayableBehaviour
    {
        public string LocalizationKey;
        public string SpeakerRole;

        private bool _displayed;
        private CutsceneSubtitleDisplay _boundDisplay;

        public override void OnGraphStart(Playable playable)
        {
            _displayed = false;
            _boundDisplay = null;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _boundDisplay = playerData as CutsceneSubtitleDisplay;

            if (_displayed || _boundDisplay == null) return;

            _boundDisplay.ShowSubtitle(LocalizationKey, SpeakerRole);
            _displayed = true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (_boundDisplay != null)
            {
                _boundDisplay.HideSubtitle();
            }
            _displayed = false;
        }
    }
}