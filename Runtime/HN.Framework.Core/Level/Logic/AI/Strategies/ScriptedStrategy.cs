namespace HN.Framework.Core.Level.Logic.AI.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HN.Framework.Core.Capability.Scripting;
    using HN.Framework.Core.Level.Logic.AI;

    /// <summary>
    /// 脚本驱动策略 — 通过 <see cref="IScriptEngine"/> 运行决策脚本，
    /// 由脚本返回动作指令列表。桥接 C15 IScriptEngine 能力，
    /// 使 AI 决策逻辑可以通过 Lua 等脚本语言实现和热更新。
    /// </summary>
    /// <remarks>
    /// 典型使用流程：
    /// <list type="number">
    ///   <item>创建 <see cref="ScriptedStrategy"/> 并注入 <see cref="IScriptEngine"/> 实例</item>
    ///   <item>调用 <see cref="Initialize"/> 执行初始化脚本（注册全局变量、加载模块等）</item>
    ///   <item>每帧调用 <see cref="Evaluate"/>，内部将上下文注册为全局变量后调用脚本决策函数</item>
    ///   <item>脚本返回动作指令列表，由策略转换为 <see cref="IActionCommand"/> 数组</item>
    /// </list>
    /// </remarks>
    public class ScriptedStrategy : IDecisionStrategy
    {
        /// <summary>
        /// 默认的上下文全局变量名，脚本通过此名称访问 <see cref="IEvaluationContext"/>。
        /// </summary>
        public const string DefaultContextGlobalName = "ai_context";

        /// <summary>
        /// 默认的决策函数名称。
        /// </summary>
        public const string DefaultDecisionFuncName = "Evaluate";

        private readonly IScriptEngine _scriptEngine;
        private readonly string _scriptModule;
        private readonly string _decisionFuncName;
        private readonly string _contextGlobalName;
        private readonly string _initScript;

        private bool _initialized;

        /// <summary>
        /// 使用指定的脚本引擎和默认配置初始化脚本驱动策略。
        /// 决策函数名默认为 "Evaluate"，上下文全局变量名默认为 "ai_context"。
        /// </summary>
        /// <param name="scriptEngine">脚本引擎实例，由外部注入。不可为 null。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="scriptEngine"/> 为 null 时抛出。</exception>
        public ScriptedStrategy(IScriptEngine scriptEngine)
            : this(scriptEngine, string.Empty, DefaultDecisionFuncName, null, DefaultContextGlobalName)
        {
        }

        /// <summary>
        /// 使用指定的脚本引擎和模块名初始化脚本驱动策略。
        /// </summary>
        /// <param name="scriptEngine">脚本引擎实例，由外部注入。不可为 null。</param>
        /// <param name="scriptModule">决策函数所在的脚本模块名称。空字符串表示全局命名空间。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="scriptEngine"/> 为 null 时抛出。</exception>
        public ScriptedStrategy(IScriptEngine scriptEngine, string scriptModule)
            : this(scriptEngine, scriptModule, DefaultDecisionFuncName, null, DefaultContextGlobalName)
        {
        }

        /// <summary>
        /// 使用完整配置初始化脚本驱动策略。
        /// </summary>
        /// <param name="scriptEngine">脚本引擎实例，由外部注入。不可为 null。</param>
        /// <param name="scriptModule">决策函数所在的脚本模块名称。空字符串表示全局命名空间。</param>
        /// <param name="decisionFuncName">
        /// 决策函数名称，脚本中必须存在此名称的函数。
        /// 默认为 "Evaluate"。不可为 null。
        /// </param>
        /// <param name="initScript">
        /// 初始化脚本代码，在 <see cref="Initialize"/> 时执行。
        /// 通常用于注册全局变量或加载依赖模块。可为 null。
        /// </param>
        /// <param name="contextGlobalName">
        /// 上下文在脚本环境中的全局变量名。每次 <see cref="Evaluate"/> 调用时，
        /// 当前 <see cref="IEvaluationContext"/> 会以此名称注册到脚本环境。
        /// 默认为 "ai_context"。不可为 null。
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// 当 <paramref name="scriptEngine"/>、<paramref name="decisionFuncName"/>
        /// 或 <paramref name="contextGlobalName"/> 为 null 时抛出。
        /// </exception>
        public ScriptedStrategy(
            IScriptEngine scriptEngine,
            string scriptModule,
            string decisionFuncName,
            string initScript,
            string contextGlobalName)
        {
            _scriptEngine = scriptEngine ?? throw new ArgumentNullException(nameof(scriptEngine));
            _scriptModule = scriptModule ?? string.Empty;
            _decisionFuncName = decisionFuncName ?? throw new ArgumentNullException(nameof(decisionFuncName));
            _initScript = initScript;
            _contextGlobalName = contextGlobalName ?? throw new ArgumentNullException(nameof(contextGlobalName));
        }

        /// <inheritdoc/>
        public string Name => "Scripted";

        /// <inheritdoc/>
        public int Priority { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 获取当前决策函数名称。用于调试和日志追踪。
        /// </summary>
        public string DecisionFuncName => _decisionFuncName;

        /// <summary>
        /// 获取当前脚本模块名称。
        /// </summary>
        public string ScriptModule => _scriptModule;

        /// <summary>
        /// 获取上下文在脚本环境中的全局变量名。
        /// </summary>
        public string ContextGlobalName => _contextGlobalName;

        /// <summary>
        /// 获取策略是否已完成初始化。
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <inheritdoc/>
        /// <remarks>
        /// 初始化过程：
        /// <list type="number">
        ///   <item>若提供了初始化脚本，则通过 <see cref="IScriptEngine.Execute"/> 执行</item>
        ///   <item>标记为已初始化。重复调用不会重复执行初始化脚本</item>
        /// </list>
        /// 初始化脚本执行失败不会阻止策略注册，但 Evaluate 时可能因函数未定义而返回空列表。
        /// </remarks>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_initScript))
            {
                try
                {
                    _scriptEngine.Execute(_initScript, $"{Name}_init");
                }
                catch (Exception)
                {
                    // NOTE: 初始化脚本执行失败不阻止策略注册。
                    // Evaluate 时若决策函数未定义将返回空列表。
                }
            }

            _initialized = true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 评估流程：
        /// <list type="number">
        ///   <li>若 context 为 null 或未初始化，直接返回空列表</li>
        ///   <li>将当前 context 注册为脚本全局变量（变量名由 <see cref="ContextGlobalName"/> 指定）</li>
        ///   <li>调用 <see cref="IScriptEngine.CallFunction"/> 执行脚本决策函数，
        ///       将 context 作为参数传入</li>
        ///   <li>将脚本返回值转换为 <see cref="IActionCommand"/> 列表</li>
        ///   <li>脚本执行异常时返回空列表</li>
        /// </list>
        /// </remarks>
        /// <param name="context">评估上下文，提供 Blackboard、WorldState、Perception 访问。</param>
        /// <returns>脚本返回的动作指令列表。异常或未初始化时返回空列表。</returns>
        public IReadOnlyList<IActionCommand> Evaluate(IEvaluationContext context)
        {
            if (context == null)
            {
                return Array.Empty<IActionCommand>();
            }

            if (!_initialized)
            {
                return Array.Empty<IActionCommand>();
            }

            try
            {
                // 将当前上下文注册为脚本全局变量，脚本可通过全局变量名直接访问
                _scriptEngine.RegisterGlobal(_contextGlobalName, context);

                // 调用脚本决策函数，传入 context 作为参数
                object rawResult = _scriptEngine.CallFunction(
                    _scriptModule,
                    _decisionFuncName,
                    context);

                return ConvertToActionCommands(rawResult);
            }
            catch (Exception)
            {
                // 脚本异常时静默处理，返回空列表避免中断决策管线
                return Array.Empty<IActionCommand>();
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 重置初始化标记，允许在关卡重开或 Agent 重生后重新执行初始化脚本。
        /// </remarks>
        public void Reset()
        {
            _initialized = false;
        }

        /// <summary>
        /// 将 <see cref="IScriptEngine.CallFunction"/> 的原始返回值转换为动作指令列表。
        /// 支持脚本返回多种格式：
        /// <list type="bullet">
        ///   <item><c>null</c> → 空列表</item>
        ///   <item><see cref="IReadOnlyList{IActionCommand}"/> → 直接返回</item>
        ///   <item><see cref="List{IActionCommand}"/> 或数组 → 直接返回（隐式转换）</item>
        ///   <item><see cref="IEnumerable{IActionCommand}"/> → 转换为列表</item>
        ///   <item>单个 <see cref="IActionCommand"/> → 包装为单元素列表</item>
        ///   <item>其他类型 → 空列表（静默忽略）</item>
        /// </list>
        /// </summary>
        /// <param name="rawResult"><see cref="IScriptEngine.CallFunction"/> 的原始返回值。</param>
        /// <returns>解析后的动作指令列表，保证非 null。</returns>
        private static IReadOnlyList<IActionCommand> ConvertToActionCommands(object rawResult)
        {
            switch (rawResult)
            {
                case null:
                    return Array.Empty<IActionCommand>();

                case IReadOnlyList<IActionCommand> readonlyList:
                    return readonlyList;

                case IEnumerable<IActionCommand> enumerable:
                    return enumerable.ToList();

                case IActionCommand single:
                    return new IActionCommand[] { single };

                default:
                    return Array.Empty<IActionCommand>();
            }
        }
    }
}
