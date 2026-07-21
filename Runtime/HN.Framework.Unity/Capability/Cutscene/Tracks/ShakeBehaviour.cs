using System;
using HN.Framework.Core.Capability.Camera;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class ShakeBehaviour : PlayableBehaviour
    {
        public float Amplitude = 1f;
        public float Frequency = 1f;

        private bool _started;
        private ICameraManager _cameraManager;

        public override void OnGraphStart(Playable playable)
        {
            _started = false;
            _cameraManager = null;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_started) return;

            _cameraManager = playerData as ICameraManager;
            if (_cameraManager != null)
            {
                var profile = new CameraShakeProfile
                {
                    AmplitudeGain = Amplitude,
                    FrequencyGain = Frequency,
                    Duration = (float)playable.GetDuration(),
                };
                _cameraManager.Shake(string.Empty, profile);
                _started = true;
            }
        }
    }
}