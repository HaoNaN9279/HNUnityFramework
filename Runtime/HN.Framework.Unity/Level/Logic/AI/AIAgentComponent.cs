using UnityEngine;
using HN.Framework.Core.Level.Logic.AI;
using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
using HN.Framework.Unity.Driver.Platform;

namespace HN.Framework.Unity.Level.Logic.AI
{
    /// <summary>
    /// AI Agent 的 MonoBehaviour 桥接组件。
    /// 负责创建 AIAgent、注册到 GameWorld.AISystem、以及向外部暴露知识池访问接口。
    /// AI 决策 Tick 由 GameWorld.AISystem 统一驱动（而非自身的 Update），
    /// 以确保与 GameWorld 其他模块保持一致的时序、时间基准和暂停语义。
    /// </summary>
    public class AIAgentComponent : MonoBehaviour
    {
        [SerializeField] private string _agentName = "AI Agent";
        private AIAgent _agent;
        private StrategyContext _context;
        private Blackboard _blackboard;
        private WorldStateCache _worldState;
        private PerceptionState _perception;
        private GameWorldDriver _driver;

        public AIAgent Agent => _agent;
        public StrategyContext Context => _context;
        public bool AutoActivate { get; set; } = true;

        private void Awake()
        {
            _agent = new AIAgent();
            _agent.Initialize(_agentName);

            // 使用 Agent 内部的 KnowledgePool 组件，避免重复创建
            _blackboard = _agent.Blackboard;
            _worldState = _agent.WorldState;
            _perception = _agent.Perception;
            _context = new StrategyContext(_blackboard, _worldState, _perception);

            // 注册到 GameWorld 的 AISystem，由 GameWorld 统一驱动 AI Tick
            // 替代原来的 MonoBehaviour.Update() 自驱动模式
            _driver = FindFirstObjectByType<GameWorldDriver>();
            _driver?.World?.AISystem?.Register(_agent);

            if (AutoActivate) Activate();
        }

        public void Activate() { _agent?.Activate(); }
        public void Deactivate() { _agent?.Deactivate(); }
        public void Pause() { _agent?.Pause(); }
        public void Resume() { _agent?.Resume(); }
        public Blackboard GetBlackboard() => _blackboard;
        public WorldStateCache GetWorldState() => _worldState;
        public PerceptionState GetPerception() => _perception;

        // NOTE: 不再使用 Update() 自驱动 AI Tick。
        // AIAgent.Tick() 现由 GameWorld.AISystem 统一调用，
        // 使用 HNLogicTime.DeltaTime 确保与 GameWorld 其他模块保持相同的时间基准和暂停语义。

        private void OnDestroy()
        {
            // 从 AISystem 注销，停止 GameWorld 对其的 Tick 驱动
            _driver?.World?.AISystem?.Unregister(_agent);
            _agent?.Clear();
        }
    }
}
