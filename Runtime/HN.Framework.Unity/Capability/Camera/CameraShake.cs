#nullable enable

using System;
using System.Collections;
using Cinemachine;
using HN.Framework.Core.Capability.Camera;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Camera
{
    /// <summary>
    /// 摄像机振动控制器。
    /// 通过 CinemachineBasicMultiChannelPerlin 组件控制 Perlin 噪声参数，
    /// 实现振幅/频率/时长可控的摄像机振动效果。
    /// </summary>
    public sealed class CameraShake
    {
        private readonly CameraHandle _cameraHandle;
        private readonly MonoBehaviour _coroutineRunner;
        private Coroutine? _shakeCoroutine;
        private CinemachineBasicMultiChannelPerlin? _perlin;

        /// <summary>
        /// 创建振动控制器。
        /// </summary>
        /// <param name="cameraHandle">要控制的摄像机句柄。</param>
        /// <param name="coroutineRunner">用于运行协程的 MonoBehaviour（通常为 CameraManager 持有的 CinemachineBrain）。</param>
        /// <exception cref="ArgumentNullException">任一参数为 null 时抛出。</exception>
        public CameraShake(CameraHandle cameraHandle, MonoBehaviour coroutineRunner)
        {
            _cameraHandle = cameraHandle ?? throw new ArgumentNullException(nameof(cameraHandle));
            _coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner));
            _perlin = _cameraHandle.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        /// <summary>是否正在振动中。</summary>
        public bool IsShaking => _shakeCoroutine != null;

        /// <summary>
        /// 开始振动。
        /// </summary>
        /// <param name="profile">振动配置文件。</param>
        public void StartShake(CameraShakeProfile profile)
        {
            StopShake();

            if (_perlin == null)
            {
                // 如果 VCam 没有 Noise 组件，尝试添加
                if (_cameraHandle.VirtualCamera is CinemachineVirtualCamera vcam)
                {
                    _perlin = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                }
            }

            if (_perlin != null)
            {
                _perlin.m_AmplitudeGain = profile.AmplitudeGain;
                _perlin.m_FrequencyGain = profile.FrequencyGain;
            }

            if (profile.Duration > 0f)
            {
                _shakeCoroutine = _coroutineRunner.StartCoroutine(ShakeDurationRoutine(profile));
            }
        }

        /// <summary>
        /// 停止振动，重置噪声参数为 0。
        /// </summary>
        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                _coroutineRunner.StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            if (_perlin != null)
            {
                _perlin.m_AmplitudeGain = 0f;
                _perlin.m_FrequencyGain = 0f;
            }
        }

        private IEnumerator ShakeDurationRoutine(CameraShakeProfile profile)
        {
            float elapsed = 0f;
            float startAmplitude = profile.AmplitudeGain;
            float startFrequency = profile.FrequencyGain;

            while (elapsed < profile.Duration)
            {
                float t = elapsed / profile.Duration;
                float decay = EvaluateDecay(t, profile.DecayType);

                if (_perlin != null)
                {
                    _perlin.m_AmplitudeGain = startAmplitude * decay;
                    _perlin.m_FrequencyGain = startFrequency * decay;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            StopShake();
        }

        private static float EvaluateDecay(float t, CameraShakeDecayType decayType)
        {
            // t: 0->1, return: 1->0
            return decayType switch
            {
                CameraShakeDecayType.Linear => 1f - t,
                CameraShakeDecayType.Exponential => Mathf.Exp(-4f * t),
                CameraShakeDecayType.EaseOut => 1f - (t * t),
                CameraShakeDecayType.EaseIn => (1f - t) * (1f - t),
                CameraShakeDecayType.EaseInOut => t < 0.5f
                    ? 1f - 2f * t * t
                    : 2f * (1f - t) * (1f - t),
                _ => 1f - t
            };
        }
    }
}
