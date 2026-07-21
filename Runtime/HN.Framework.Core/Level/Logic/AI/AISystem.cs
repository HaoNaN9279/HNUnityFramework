using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Level.Logic.AI
{
    /// <summary>
    /// AI 系统 — GameWorld 级别的 AI Agent 管理器。
    /// 负责统一驱动所有已注册 Agent 的 Tick 循环，替代 MonoBehaviour 自驱动的模式。
    /// 由 <see cref="Driver.GameWorld"/> 创建并驱动，确保 AI Tick 与 GameWorld 其他模块
    /// 保持一致的时序和暂停语义。
    /// </summary>
    /// <remarks>
    /// <para>
    /// AIAgentComponent 在 Awake 时通过 <c>world.AISystem.Register(agent)</c> 注册 Agent，
    /// 在 OnDestroy 时通过 <c>Unregister(agent)</c> 注销。
    /// </para>
    /// <para>
    /// Agent 的减速/暂停通过 HNLogicTime.DeltaTime 自动传播：当 GameWorld 暂停时，
    /// HNLogicTime.DeltaTime 归零，Agent.Tick(0) 将跳过决策执行。
    /// </para>
    /// </remarks>
    public class AISystem : ITickable
    {
        private readonly List<AIAgent> _agents = new List<AIAgent>();

        /// <summary>
        /// 已注册的 Agent 数量。
        /// </summary>
        public int AgentCount
        {
            get { return _agents.Count; }
        }

        /// <summary>
        /// 注册一个 AIAgent 到系统中。Agent 的 Tick 将由 GameWorld 统一驱动。
        /// </summary>
        /// <param name="agent">要注册的 Agent。为 null 或已注册时则忽略。</param>
        public void Register(AIAgent agent)
        {
            if (agent == null || _agents.Contains(agent))
            {
                return;
            }

            _agents.Add(agent);
        }

        /// <summary>
        /// 从系统中注销一个 AIAgent。
        /// </summary>
        /// <param name="agent">要注销的 Agent。</param>
        public void Unregister(AIAgent agent)
        {
            if (agent != null)
            {
                _agents.Remove(agent);
            }
        }

        /// <summary>
        /// 每帧驱动所有已注册 Agent。
        /// 使用 <see cref="HNLogicTime.DeltaTime"/> 确保与 GameWorld 其他模块使用相同的时间基准，
        /// 当 GameWorld 暂停时 DeltaTime 归零，Agent 自然停止决策。
        /// </summary>
        public void Tick()
        {
            var deltaTime = (float)HNLogicTime.DeltaTime;
            for (int i = _agents.Count - 1; i >= 0; i--)
            {
                _agents[i].Tick(deltaTime);
            }
        }

        /// <summary>
        /// LateTick 空实现。
        /// </summary>
        public void LateTick()
        {
        }

        /// <summary>
        /// 清空所有已注册的 Agent（不销毁 Agent 本身，仅解除注册）。
        /// </summary>
        public void Clear()
        {
            _agents.Clear();
        }
    }
}
