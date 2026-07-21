namespace HN.Framework.Core.Level.Logic.AI
{
    using System.Collections.Generic;
    using HN.Framework.Core.Driver.Common;

    /// <summary>
    /// 管线层级枚举 — 四层标准管线
    /// </summary>
    public enum PipelineLayer
    {
        /// <summary>
        /// 战略层（如 Utility）
        /// </summary>
        Strategic = 0,

        /// <summary>
        /// 任务分解层（如 HTN）
        /// </summary>
        TaskPlanning = 1,

        /// <summary>
        /// 行为执行层（如 BehaviorTree）
        /// </summary>
        BehaviorExecution = 2,

        /// <summary>
        /// 子状态层（如 FSM）
        /// </summary>
        SubState = 3,
    }

    /// <summary>
    /// 决策管线编排器 — 按层级组织策略链，顺序评估输出动作指令
    /// </summary>
    public class DecisionPipeline : IReference
    {
        private IDecisionStrategy[] m_Layers;
        private bool[] m_LayerEnabled;

        /// <summary>
        /// 管线层级数。可通过 <see cref="SetLayerCount"/> 在初始化时从配置 SO 注入。
        /// </summary>
        internal static int s_LayerCount = 4;

        /// <summary>
        /// 设置决策管线层级数（由 Unity 层桥接从 AISettings 注入）。
        /// </summary>
        public static void SetLayerCount(int count)
        {
            if (count > 0)
                s_LayerCount = count;
        }

        /// <summary>
        /// 初始化管线，所有层默认跳过
        /// </summary>
        public void Initialize()
        {
            m_Layers = new IDecisionStrategy[s_LayerCount];
            m_LayerEnabled = new bool[s_LayerCount];
        }

        /// <summary>
        /// 设置指定层级的策略
        /// </summary>
        /// <param name="layer">管线层级</param>
        /// <param name="strategy">要设置的策略实例，传 null 等同于跳过该层</param>
        public void SetLayer(PipelineLayer layer, IDecisionStrategy strategy)
        {
            int index = (int)layer;
            m_Layers[index] = strategy;
            m_LayerEnabled[index] = strategy != null;
        }

        /// <summary>
        /// 跳过指定层级（不执行）
        /// </summary>
        /// <param name="layer">要跳过的管线层级</param>
        public void SkipLayer(PipelineLayer layer)
        {
            m_LayerEnabled[(int)layer] = false;
        }

        /// <summary>
        /// 获取指定层级的策略
        /// </summary>
        /// <param name="layer">管线层级</param>
        /// <returns>该层配置的策略实例，未配置则返回 null</returns>
        public IDecisionStrategy GetLayer(PipelineLayer layer)
        {
            return m_Layers[(int)layer];
        }

        /// <summary>
        /// 指定层级是否启用
        /// </summary>
        /// <param name="layer">管线层级</param>
        /// <returns>已启用且策略非空则返回 true</returns>
        public bool IsLayerEnabled(PipelineLayer layer)
        {
            return m_LayerEnabled[(int)layer];
        }

        /// <summary>
        /// 按层级顺序执行管线
        /// 每层的策略 Evaluate 结果累加到最终输出列表
        /// </summary>
        /// <param name="context">评估上下文</param>
        /// <returns>所有启用层策略产生的动作指令只读列表</returns>
        public IReadOnlyList<IActionCommand> Execute(IEvaluationContext context)
        {
            if (context == null)
            {
                return System.Array.Empty<IActionCommand>();
            }

            var results = new List<IActionCommand>();
            for (int i = 0; i < s_LayerCount; i++)
            {
                if (m_LayerEnabled[i] && m_Layers[i] != null && m_Layers[i].IsEnabled)
                {
                    var layerResults = m_Layers[i].Evaluate(context);
                    if (layerResults != null)
                    {
                        results.AddRange(layerResults);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 获取当前管线中的策略快照
        /// </summary>
        /// <returns>所有已启用层配置的策略只读列表</returns>
        public IReadOnlyList<IDecisionStrategy> GetActiveStrategies()
        {
            var active = new List<IDecisionStrategy>();
            for (int i = 0; i < s_LayerCount; i++)
            {
                if (m_LayerEnabled[i] && m_Layers[i] != null)
                {
                    active.Add(m_Layers[i]);
                }
            }

            return active;
        }

        /// <summary>
        /// 检查管线是否有任何启用层
        /// </summary>
        /// <returns>存在启用层则返回 true</returns>
        public bool HasAnyEnabled()
        {
            for (int i = 0; i < s_LayerCount; i++)
            {
                if (m_LayerEnabled[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 重置所有层策略 — 依次调用各策略的 Reset 并清空管线配置
        /// </summary>
        public void ResetAll()
        {
            for (int i = 0; i < s_LayerCount; i++)
            {
                if (m_Layers[i] != null)
                {
                    m_Layers[i].Reset();
                    m_Layers[i] = null;
                }

                m_LayerEnabled[i] = false;
            }
        }

        /// <summary>
        /// 实现 IReference.Clear — 重置所有策略并释放数组引用
        /// </summary>
        public void Clear()
        {
            ResetAll();
            m_Layers = null;
            m_LayerEnabled = null;
        }
    }
}
