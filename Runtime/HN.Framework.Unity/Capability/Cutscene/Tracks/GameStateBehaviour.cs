using System;
using UnityEngine;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Capability.Cutscene.Tracks
{
    [Serializable]
    public class GameStateBehaviour : PlayableBehaviour
    {
        public string TargetObjectPath;
        public string ComponentType;
        public bool SetActive;
        public string AnimatorParameter;
        public float AnimatorValue;

        private bool _applied;

        public override void OnGraphStart(Playable playable)
        {
            _applied = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (_applied) return;

            var target = playerData as GameObject;
            if (target == null && !string.IsNullOrEmpty(TargetObjectPath))
            {
                target = GameObject.Find(TargetObjectPath);
            }

            if (target != null)
            {
                if (!string.IsNullOrEmpty(ComponentType))
                {
                    var component = target.GetComponent(ComponentType);
                    if (component != null)
                    {
                        if (component is Behaviour behaviour)
                            behaviour.enabled = SetActive;
                        else if (component is Renderer renderer)
                            renderer.enabled = SetActive;
                        else if (component is Collider collider)
                            collider.enabled = SetActive;
                    }
                }

                if (!string.IsNullOrEmpty(AnimatorParameter))
                {
                    var animator = target.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetFloat(AnimatorParameter, AnimatorValue);
                    }
                }
            }

            _applied = true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            _applied = false;
        }
    }
}