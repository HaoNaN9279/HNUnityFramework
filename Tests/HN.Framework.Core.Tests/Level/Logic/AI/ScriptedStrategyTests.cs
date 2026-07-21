#nullable enable

namespace HN.Framework.Core.Tests.Level.Logic.AI
{
    using System;
    using System.Collections.Generic;
    using HN.Framework.Core.Capability.Scripting;
    using HN.Framework.Core.Level.Logic.AI;
    using HN.Framework.Core.Level.Logic.AI.KnowledgePool;
    using HN.Framework.Core.Level.Logic.AI.Strategies;
    using NUnit.Framework;

    /// <summary>
    /// ScriptedStrategy 单元测试 — 验证脚本驱动策略的脚本引擎集成、异常安全和生命周期管理。
    /// </summary>
    [TestFixture]
    public class ScriptedStrategyTests
    {
        private MockScriptEngine _mockEngine = null!;
        private StrategyContext _context = null!;
        private Blackboard _blackboard = null!;
        private WorldStateCache _worldState = null!;
        private PerceptionState _perception = null!;

        /// <summary>
        /// 每个测试前初始化 MockScriptEngine 和评估上下文。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _mockEngine = new MockScriptEngine();

            _blackboard = new Blackboard();
            _blackboard.Initialize();

            _worldState = new WorldStateCache();
            _worldState.Initialize();

            _perception = new PerceptionState();
            _perception.Initialize();

            _context = new StrategyContext(_blackboard, _worldState, _perception);
        }

        /// <summary>
        /// 每个测试后清理评估上下文资源。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _blackboard.Clear();
            _worldState.Clear();
            _perception.Clear();
        }

        /// <summary>
        /// 脚本引擎返回 IActionCommand 时，Evaluate 返回包含该指令的列表。
        /// </summary>
        [Test]
        public void Evaluate_WithScriptEngine_ReturnsActionCommand()
        {
            TestActionCommand expectedCmd = new TestActionCommand();
            expectedCmd.Initialize();
            _mockEngine.CallFunctionResult = expectedCmd;

            ScriptedStrategy strategy = new ScriptedStrategy(_mockEngine);
            strategy.Initialize();

            IReadOnlyList<IActionCommand> result = strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.SameAs(expectedCmd));
            Assert.That(_mockEngine.LastCalledFunction, Is.EqualTo("Evaluate"));
        }

        /// <summary>
        /// 策略禁用后，Evaluate 直接返回空列表，不调用脚本引擎。
        /// </summary>
        [Test]
        public void Evaluate_Disabled_ReturnsEmpty()
        {
            ScriptedStrategy strategy = new ScriptedStrategy(_mockEngine);
            strategy.Initialize();
            strategy.IsEnabled = false;

            IReadOnlyList<IActionCommand> result = strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 未初始化时 Evaluate 返回空列表，不调用脚本引擎。
        /// </summary>
        [Test]
        public void Evaluate_NotInitialized_ReturnsEmpty()
        {
            ScriptedStrategy strategy = new ScriptedStrategy(_mockEngine);

            // 未调用 Initialize，_initialized 为 false
            IReadOnlyList<IActionCommand> result = strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
            // 验证脚本引擎未被调用
            Assert.That(_mockEngine.LastCalledFunction, Is.Null);
        }

        /// <summary>
        /// 脚本引擎 CallFunction 抛出异常时，Evaluate 静默处理并返回空列表。
        /// </summary>
        [Test]
        public void Evaluate_ScriptEngineThrows_ReturnsEmptyGracefully()
        {
            _mockEngine.ThrowOnCallFunction = true;

            ScriptedStrategy strategy = new ScriptedStrategy(_mockEngine);
            strategy.Initialize();

            IReadOnlyList<IActionCommand> result = strategy.Evaluate(_context);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// 提供初始化脚本时，Initialize 执行该脚本；Evaluate 调用指定的决策函数并传入上下文。
        /// </summary>
        [Test]
        public void Initialize_AndEvaluate_CallsScriptFunction()
        {
            string initCode = "function setup() end";
            string decisionFuncName = "Decide";
            _mockEngine.CallFunctionResult = new TestActionCommand();

            ScriptedStrategy strategy = new ScriptedStrategy(
                _mockEngine, string.Empty, decisionFuncName, initCode, "ai_ctx");
            strategy.Initialize();

            // 验证初始化脚本已执行
            Assert.That(_mockEngine.ExecutedScripts.Count, Is.EqualTo(1));
            Assert.That(_mockEngine.ExecutedScripts[0], Is.EqualTo(initCode));

            strategy.Evaluate(_context);

            // 验证上下文已注册为全局变量
            Assert.That(_mockEngine.RegisteredGlobals.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(_mockEngine.RegisteredGlobals[0].Item1, Is.EqualTo("ai_ctx"));

            // 验证正确调用了指定的决策函数
            Assert.That(_mockEngine.LastCalledFunction, Is.EqualTo(decisionFuncName));

            // 验证上下文作为参数传入
            Assert.That(_mockEngine.LastCalledArgs, Is.Not.Null);
            Assert.That(_mockEngine.LastCalledArgs!.Length, Is.EqualTo(1));
            Assert.That(_mockEngine.LastCalledArgs[0], Is.SameAs(_context));
        }

        /// <summary>
        /// IScriptEngine 的 Mock 实现，用于单元测试中模拟脚本引擎行为。
        /// 记录所有调用信息并支持配置返回值和异常行为。
        /// </summary>
        private class MockScriptEngine : IScriptEngine
        {
            /// <summary>
            /// 已执行的脚本代码列表。
            /// </summary>
            public List<string> ExecutedScripts { get; } = new List<string>();

            /// <summary>
            /// 已注册的全局变量列表（名称, 对象）。
            /// </summary>
            public List<(string, object)> RegisteredGlobals { get; } = new List<(string, object)>();

            /// <summary>
            /// CallFunction 的返回值。
            /// </summary>
            public object? CallFunctionResult { get; set; }

            /// <summary>
            /// 是否在 Execute 调用时抛出异常。
            /// </summary>
            public bool ThrowOnExecute { get; set; }

            /// <summary>
            /// 是否在 CallFunction 调用时抛出异常。
            /// </summary>
            public bool ThrowOnCallFunction { get; set; }

            /// <summary>
            /// 最后一次调用 CallFunction 的模块名。
            /// </summary>
            public string? LastCalledModule { get; private set; }

            /// <summary>
            /// 最后一次调用 CallFunction 的函数名。
            /// </summary>
            public string? LastCalledFunction { get; private set; }

            /// <summary>
            /// 最后一次调用 CallFunction 的参数列表。
            /// </summary>
            public object[]? LastCalledArgs { get; private set; }

            /// <inheritdoc/>
            public void Execute(string code, string chunkName = "inline")
            {
                ExecutedScripts.Add(code);

                if (ThrowOnExecute)
                {
                    throw new InvalidOperationException("Mock script engine execute error.");
                }
            }

            /// <inheritdoc/>
            public void RegisterGlobal(string name, object obj)
            {
                RegisteredGlobals.Add((name, obj));
            }

            /// <inheritdoc/>
            public object CallFunction(string moduleName, string funcName, params object[] args)
            {
                LastCalledModule = moduleName;
                LastCalledFunction = funcName;
                LastCalledArgs = args;

                if (ThrowOnCallFunction)
                {
                    throw new InvalidOperationException("Mock script engine call error.");
                }

                return CallFunctionResult!;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }
        }

        /// <summary>
        /// 测试用 ActionCommand — 实现 IActionCommand 的最小 mock。
        /// 注意：不实现 IReference（不使用 ReferencePool），直接 new 使用。
        /// </summary>
        private sealed class TestActionCommand : IActionCommand
        {
            public string TypeName => "TestAction";
            public int Priority { get; set; }
            public ActionCommandStatus Status { get; set; }

            public void Initialize()
            {
                Status = ActionCommandStatus.Pending;
            }

            public void Execute()
            {
                Status = ActionCommandStatus.Running;
            }

            public void Cancel()
            {
                Status = ActionCommandStatus.Cancelled;
            }

            public void Clear()
            {
                Priority = 0;
                Status = ActionCommandStatus.Pending;
            }
        }
    }
}
