using UnityEngine;
using UnityEngine.AI;

namespace HN.Framework.Unity.Level.Logic.AI
{
    /// <summary>
    /// 寻路适配器 — 封装 Unity NavMeshAgent，提供通用寻路控制方法。
    /// 不再绑定特定的 ActionCommand 类型，由项目在 <see cref="DefaultActionExecutor"/> 处理器中调用。
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshAgentAdapter : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 3.5f;
        [SerializeField] private float _stoppingDistance = 0.5f;

        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = _moveSpeed;
            _agent.stoppingDistance = _stoppingDistance;
        }

        /// <summary>
        /// 设置寻路目标并开始移动。可指定速度倍率。
        /// </summary>
        /// <param name="target">目标世界坐标。</param>
        /// <param name="speedMultiplier">速度倍率，默认 1.0。</param>
        public void SetDestination(Vector3 target, float speedMultiplier = 1f)
        {
            if (_agent == null)
            {
                return;
            }

            _agent.speed = _moveSpeed * speedMultiplier;
            _agent.SetDestination(target);
        }

        /// <summary>
        /// 取消当前寻路。
        /// </summary>
        public void CancelMove()
        {
            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.ResetPath();
            }
        }

        /// <summary>
        /// 是否已到达目的地。
        /// </summary>
        /// <returns>已到达则返回 true。</returns>
        public bool HasReachedDestination()
        {
            return _agent != null
                && !_agent.pathPending
                && _agent.remainingDistance <= _agent.stoppingDistance;
        }

        /// <summary>
        /// 当前移动速度。
        /// </summary>
        public float Speed
        {
            get => _agent != null ? _agent.speed : 0f;
            set
            {
                if (_agent != null)
                {
                    _agent.speed = value;
                }
            }
        }
    }
}
