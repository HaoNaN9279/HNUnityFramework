using System;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests
{
    [TestFixture]
    public class ProcedureTests
    {
        #region Helper Types

        /// <summary>
        /// 用于测试的简单 ProcedureState 子类
        /// </summary>
        private class TestProcedureState : ProcedureState
        {
        }

        /// <summary>
        /// 特定类型的状态，用于 ChangeState&lt;T&gt; 泛型查找测试
        /// </summary>
        private class SpecificState : TestProcedureState
        {
        }

        /// <summary>
        /// 模拟子流程，跟踪 Cancel 和 Clear 的调用状态
        /// </summary>
        private class MockSubProcess : IProcedureSubProcess
        {
            public bool Canceled { get; private set; }
            public bool Cleared { get; private set; }

            public void Cancel()
            {
                Canceled = true;
            }

            public void Clear()
            {
                Cleared = true;
            }
        }

        #endregion

        #region Test 1: Clear nulls LateTickEvent after Release (P0)

        /// <summary>
        /// 验证 Release → Clear 后 LateTickEvent 被置空，
        /// 重新从对象池获取同一实例后，旧的 handler 不会被触发。
        /// </summary>
        [Test]
        public void Clear_NullsLateTickEvent_AfterRelease()
        {
            var manager = new ProcedureManager();
            int callCount = 0;

            var state = manager.AddState<TestProcedureState>("StateA");
            state.LateTickEvent += () => callCount++;

            manager.Start("StateA");
            manager.LateTick();
            Assert.AreEqual(1, callCount, "Handler should fire while state is active");

            // Shutdown + RemoveState 触发 Release → Clear，事件被置空
            manager.Shutdown();
            manager.RemoveState("StateA");

            // 从对象池重新获取同一类型实例（Clear 后的实例）
            var reacquired = manager.AddState<TestProcedureState>("StateA");
            manager.Start("StateA");
            manager.LateTick();

            // 旧的 handler 已被 Clear 清除，不应被调用
            Assert.AreEqual(1, callCount, "LateTickEvent subscription should have been cleared on Release");

            manager.Shutdown();
            manager.RemoveState("StateA");
        }

        #endregion

        #region Test 2: AddState with duplicate name throws (P1)

        /// <summary>
        /// 验证向 ProcedureManager 添加同名状态时抛出 InvalidOperationException
        /// </summary>
        [Test]
        public void AddState_WithDuplicateName_ThrowsException()
        {
            var manager = new ProcedureManager();
            manager.AddState<TestProcedureState>("TestState");

            var duplicate = ReferencePool.Acquire<TestProcedureState>();
            duplicate.Initialize("TestState");

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                manager.AddState(duplicate);
            });
            Assert.That(ex.Message, Does.Contain("TestState"));

            // 清理：释放未成功添加的状态
            ReferencePool.Release(duplicate);
            manager.ClearAll();
        }

        #endregion

        #region Test 3: SubProcess is canceled on ChangeState (P1)

        /// <summary>
        /// 验证 ChangeState 时，当前状态的子流程被 Cancel 并释放回对象池
        /// </summary>
        [Test]
        public void SubProcess_IsCanceled_OnStateChange()
        {
            var manager = new ProcedureManager();
            manager.AddState<TestProcedureState>("A");
            manager.AddState<TestProcedureState>("B");

            manager.Start("A");

            var mock = ReferencePool.Acquire<MockSubProcess>();
            manager.CurrentState.RegisterSubProcess(mock);

            manager.ChangeState("B");

            Assert.IsTrue(mock.Canceled, "SubProcess should be canceled on state change");
            Assert.IsTrue(mock.Cleared, "SubProcess should be released back to pool on state change");

            manager.Shutdown();
            manager.ClearAll();
        }

        #endregion

        #region Test 4: SubProcess is canceled on Shutdown (P1)

        /// <summary>
        /// 验证 Shutdown 时，当前状态的子流程被 Cancel 并释放回对象池
        /// </summary>
        [Test]
        public void SubProcess_IsCanceled_OnShutdown()
        {
            var manager = new ProcedureManager();
            manager.AddState<TestProcedureState>("A");

            manager.Start("A");

            var mock = ReferencePool.Acquire<MockSubProcess>();
            manager.CurrentState.RegisterSubProcess(mock);

            manager.Shutdown();

            Assert.IsTrue(mock.Canceled, "SubProcess should be canceled on shutdown");
            Assert.IsTrue(mock.Cleared, "SubProcess should be released back to pool on shutdown");

            manager.ClearAll();
        }

        #endregion

        #region Test 5: Reentrant ChangeState does not stack overflow (P1)

        /// <summary>
        /// 验证 ChangeState 期间的重入调用被 m_isChanging 守卫静默忽略，
        /// 不会造成栈溢出，最终状态保持为原始目标状态。
        /// </summary>
        [Test]
        public void ChangeState_ReentrantCall_DoesNotStackOverflow()
        {
            var manager = new ProcedureManager();
            var stateA = manager.AddState<TestProcedureState>("A");
            var stateB = manager.AddState<TestProcedureState>("B");
            manager.AddState<TestProcedureState>("C");

            // A 的 ExitEvent 在退出时尝试重入 ChangeState("C")
            stateA.ExitEvent += () => manager.ChangeState("C");

            manager.Start("A");

            Assert.DoesNotThrow(() =>
            {
                manager.ChangeState("B");
            });

            // 重入调用被忽略，最终状态应为 B 而不是 C
            Assert.AreEqual(stateB, manager.CurrentState, "Reentrant ChangeState should be ignored by m_isChanging guard");

            manager.Shutdown();
            manager.ClearAll();
        }

        #endregion

        #region Test 6: ChangeState to same state is no-op (P1)

        /// <summary>
        /// 验证 ChangeState 到当前状态时不会重复触发 EnterEvent
        /// </summary>
        [Test]
        public void ChangeState_SameState_Noop()
        {
            var manager = new ProcedureManager();
            var stateA = manager.AddState<TestProcedureState>("A");

            int enterCount = 0;
            stateA.EnterEvent += () => enterCount++;

            manager.Start("A");
            Assert.AreEqual(1, enterCount, "EnterEvent fires on Start");

            manager.ChangeState("A");
            Assert.AreEqual(1, enterCount, "EnterEvent should NOT fire again on same-state ChangeState");
            Assert.AreEqual(stateA, manager.CurrentState);

            manager.Shutdown();
            manager.ClearAll();
        }

        #endregion

        #region Test 7: ExitEvent exception does not block transition (P2)

        /// <summary>
        /// 验证 ExitEvent 抛出异常不会阻断状态转换，
        /// 目标状态的 EnterEvent 仍然被调用
        /// </summary>
        [Test]
        public void ExitEvent_Throws_DoesNotBlockTransition()
        {
            var manager = new ProcedureManager();
            var stateA = manager.AddState<TestProcedureState>("A");
            var stateB = manager.AddState<TestProcedureState>("B");

            bool bEntered = false;
            stateA.ExitEvent += () => throw new Exception("Simulated exit failure");
            stateB.EnterEvent += () => bEntered = true;

            manager.Start("A");

            Assert.DoesNotThrow(() =>
            {
                manager.ChangeState("B");
            });

            Assert.IsTrue(bEntered, "State B EnterEvent should fire despite A's ExitEvent throwing");
            Assert.AreEqual(stateB, manager.CurrentState);

            manager.Shutdown();
            manager.ClearAll();
        }

        #endregion

        #region Test 8: ChangeState<T> finds by type (P2)

        /// <summary>
        /// 验证 ChangeState&lt;T&gt; 通过类型查找并切换到目标状态
        /// </summary>
        [Test]
        public void ChangeState_Generic_FindsByType()
        {
            var manager = new ProcedureManager();
            manager.AddState<TestProcedureState>("PlainState");
            manager.AddState<SpecificState>("MyState");

            manager.Start("PlainState");
            manager.ChangeState<SpecificState>();

            Assert.AreEqual("MyState", manager.CurrentStateName,
                "ChangeState<SpecificState>() should find the state of that type");

            manager.Shutdown();
            manager.ClearAll();
        }

        #endregion

        #region Test 9: CurrentStateName returns null before Start / after Shutdown (P2)

        /// <summary>
        /// 验证 CurrentStateName 在未启动和 Shutdown 后返回 null
        /// </summary>
        [Test]
        public void CurrentStateName_ReturnsNull_BeforeStart()
        {
            var manager = new ProcedureManager();
            manager.AddState<TestProcedureState>("SomeState");

            // 启动前应为 null
            Assert.IsNull(manager.CurrentStateName, "CurrentStateName should be null before Start");

            manager.Start("SomeState");
            Assert.AreEqual("SomeState", manager.CurrentStateName,
                "CurrentStateName should return the active state name after Start");

            manager.Shutdown();
            Assert.IsNull(manager.CurrentStateName,
                "CurrentStateName should be null after Shutdown");

            manager.ClearAll();
        }

        #endregion
    }
}
