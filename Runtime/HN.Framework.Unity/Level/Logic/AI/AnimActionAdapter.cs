using UnityEngine;

namespace HN.Framework.Unity.Level.Logic.AI
{
    /// <summary>
    /// 动画适配器 — 封装 Unity Animator，提供通用动画控制方法。
    /// 不再绑定特定的 ActionCommand 类型，由项目在 <see cref="DefaultActionExecutor"/> 处理器中调用。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimActionAdapter : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 播放指定动画。可指定播放速度。
        /// </summary>
        /// <param name="animationName">动画名称。</param>
        /// <param name="speed">播放速度，默认 1.0。</param>
        public void PlayAnimation(string animationName, float speed = 1f)
        {
            if (_animator == null)
            {
                return;
            }

            _animator.speed = speed;
            _animator.Play(Animator.StringToHash(animationName));
        }

        /// <summary>
        /// 取消动画播放，恢复默认速度。
        /// </summary>
        public void CancelAnimation()
        {
            if (_animator != null)
            {
                _animator.speed = 1f;
            }
        }

        /// <summary>
        /// 当前动画是否已播放完毕。
        /// </summary>
        /// <returns>播放完毕返回 true。</returns>
        public bool IsAnimationFinished()
        {
            if (_animator == null)
            {
                return true;
            }

            return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
                && !_animator.IsInTransition(0);
        }
    }
}
