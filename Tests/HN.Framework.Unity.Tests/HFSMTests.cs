using System;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using HN.Framework.Core.Level.Logic;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests
{
    [TestFixture]
    public class HFSMTests
    {
        #region Helper Types

        private class TrackedState : HFSMState
        {
            public bool Entered { get; private set; }
            public bool Exited { get; private set; }
            public bool Created { get; private set; }
            public bool Destroyed { get; private set; }

            public override void OnCreate()
            {
                base.OnCreate();
                Created = true;
                isAllowedTrans = true;
            }

            public override void OnEnter() { Entered = true; }
            public override void OnExit() { Exited = true; }
            public override void OnDestroy() { Destroyed = true; }
        }

        private class InnerState : HFSMState
        {
            public override void OnCreate()
            {
                base.OnCreate();
                isAllowedTrans = true;
            }
        }

        private class TestCompoundState : HFSMCompoundState<HFSM>
        {
            public override void Initialize(string name)
            {
                base.Initialize(name);
                var innerState = SubStateMachine.AddState<InnerState>("Inner");
                SubStateMachine.AddTransition<HFSMTransition>(
                    SubStateMachine.EntryState, innerState, null);
                SubStateMachine.AddTransition<HFSMTransition>(
                    innerState, SubStateMachine.ExitState, null);
            }
        }

        #endregion

        #region Simple FSM

        [Test]
        public void SimpleFSM_TwoStates_TransitionDrivesStateChange()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var stateA = fsm.AddState<TrackedState>("A");
            var stateB = fsm.AddState<TrackedState>("B");
            bool condition = false;
            fsm.AddTransition<HFSMTransition>(stateA, stateB, () => condition);

            fsm.Start("A");
            Assert.AreEqual(stateA, fsm.CurrentState);
            Assert.IsTrue(stateA.Entered);

            fsm.Update();
            Assert.AreEqual(stateA, fsm.CurrentState);

            condition = true;
            fsm.Update();
            Assert.AreEqual(stateB, fsm.CurrentState);
            Assert.IsTrue(stateA.Exited);
            Assert.IsTrue(stateB.Entered);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void SimpleFSM_StartWithStateName_EntersCorrectly()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var stateB = fsm.AddState<TrackedState>("B");
            fsm.Start("B");

            Assert.AreEqual(stateB, fsm.CurrentState);
            Assert.IsTrue(stateB.Entered);
            Assert.IsTrue(fsm.IsAlive);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void StartWithoutArgs_GoesThroughEntryState()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var targetA = fsm.AddState<TrackedState>("A");
            fsm.AddTransition<HFSMTransition>(fsm.EntryState, targetA, null);
            fsm.Start();

            Assert.AreEqual(fsm.EntryState, fsm.CurrentState);
            fsm.Update();
            Assert.AreEqual(targetA, fsm.CurrentState);

            ReferencePool.Release(fsm);
        }

        #endregion

        #region Compound State

        [Test]
        public void CompoundState_InnerSubFSM_RunsAndCompletes()
        {
            var compound = ReferencePool.Acquire<TestCompoundState>();
            compound.Initialize("Combat");

            Assert.IsNotNull(compound.SubStateMachine);
            Assert.AreEqual(3, compound.SubStateMachine.StateCount); // Entry + Inner + Exit

            // Enter the compound state
            compound.OnEnter();
            Assert.IsTrue(compound.SubStateMachine.IsAlive);

            // Sub-FSM: Entry → Inner → Exit (cascades in one Update)
            compound.Update();
            Assert.IsFalse(compound.SubStateMachine.IsAlive);
            // After sub-FSM completes, isAllowedTrans should be true
            // (but we can't easily test protected field)

            ReferencePool.Release(compound);
        }

        /// <summary>
        /// 验证复合状态在完整 FSM 中正确级联：
        /// Idle → Combat（子状态机运行完成）→ End → Shutdown，全在一个 Update 帧。
        /// </summary>
        [Test]
        public void CompoundState_InFSM_CascadesToEndAndShutsDown()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();

            var idleState = fsm.AddState<TrackedState>("Idle");
            var combatCompound = fsm.AddState<TestCompoundState>("Combat");
            var endState = fsm.AddState<TrackedState>("End");

            fsm.AddTransition<HFSMTransition>(idleState, combatCompound, () => true);
            fsm.AddTransition<HFSMTransition>(combatCompound, endState, null);
            fsm.Start("Idle");

            // 一帧内：Idle → Combat(子FSM完成) → End
            fsm.Update();

            // 所有转换级联完成，FSM 停留在 End
            Assert.AreEqual(endState, fsm.CurrentState);
            Assert.IsTrue(fsm.IsAlive); // End 没有到 ExitState，所以还活着

            ReferencePool.Release(fsm);
        }

        #endregion

        #region RemoveState / RemoveTransition

        [Test]
        public void RemoveState_AlsoRemovesAssociatedTransitions()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var a = fsm.AddState<TrackedState>("A");
            var b = fsm.AddState<TrackedState>("B");
            fsm.AddTransition<HFSMTransition>(a, b, null);

            Assert.AreEqual(1, a.OutputTransitions.Count);
            Assert.AreEqual(1, b.InputTransitions.Count);

            fsm.RemoveState(a);

            Assert.IsTrue(a.Destroyed);
            Assert.AreEqual(0, fsm.Transitions.Count);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void RemoveTransition_ByStates_CleansUpBothSides()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var a = fsm.AddState<TrackedState>("A");
            var b = fsm.AddState<TrackedState>("B");
            fsm.AddTransition<HFSMTransition>(a, b, null);

            fsm.RemoveTransition(a, b);

            Assert.AreEqual(0, a.OutputTransitions.Count);
            Assert.AreEqual(0, b.InputTransitions.Count);
            Assert.AreEqual(0, fsm.Transitions.Count);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void RemoveTransition_ByTransitionObject_CleansUpBothSides()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var a = fsm.AddState<TrackedState>("A");
            var b = fsm.AddState<TrackedState>("B");
            var trans = fsm.AddTransition<HFSMTransition>(a, b, null);

            fsm.RemoveTransition(trans);

            Assert.AreEqual(0, a.OutputTransitions.Count);
            Assert.AreEqual(0, b.InputTransitions.Count);
            Assert.AreEqual(0, fsm.Transitions.Count);

            ReferencePool.Release(fsm);
        }

        #endregion

        #region Regression — transitions init

        [Test]
        public void Initialize_InitializesTransitionsDictionary_NoNPE()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();

            Assert.DoesNotThrow(() =>
            {
                var a = fsm.AddState<TrackedState>("A");
                var b = fsm.AddState<TrackedState>("B");
                fsm.AddTransition<HFSMTransition>(a, b, null);
            });

            Assert.AreEqual(1, fsm.Transitions.Count);
            Assert.IsNotNull(fsm.Transitions);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void TransitionsProperty_BeforeInitialize_ReturnsEmptyNotNull()
        {
            var fsm = ReferencePool.Acquire<HFSM>();

            Assert.IsNotNull(fsm.Transitions);
            Assert.AreEqual(0, fsm.Transitions.Count);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void StatesProperty_BeforeInitialize_ReturnsEmptyNotNull()
        {
            var fsm = ReferencePool.Acquire<HFSM>();

            Assert.IsNotNull(fsm.States);
            Assert.AreEqual(0, fsm.States.Count);

            ReferencePool.Release(fsm);
        }

        #endregion

        #region Regression — Clear

        [Test]
        public void StateClear_ReleasesPooledDictionaries_NoLeak()
        {
            var state = ReferencePool.Acquire<TrackedState>();
            state.Initialize("TestState");

            Assert.DoesNotThrow(() =>
            {
                ReferencePool.Release(state);
            });

            var state2 = ReferencePool.Acquire<TrackedState>();
            state2.Initialize("TestState2");
            Assert.IsNotNull(state2.InputTransitions);
            Assert.IsNotNull(state2.OutputTransitions);
            ReferencePool.Release(state2);
        }

        [Test]
        public void CompoundStateClear_ReleasesSubStateMachine()
        {
            var compound = ReferencePool.Acquire<TestCompoundState>();
            compound.Initialize("Combat");

            Assert.IsNotNull(compound.SubStateMachine);
            Assert.IsTrue(compound.SubStateMachine.StateCount > 0);

            ReferencePool.Release(compound);

            var compound2 = ReferencePool.Acquire<TestCompoundState>();
            compound2.Initialize("Combat2");
            Assert.IsNotNull(compound2.SubStateMachine);
            Assert.IsTrue(compound2.SubStateMachine.StateCount > 0);
            ReferencePool.Release(compound2);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void StateWithNoOutputTransitions_DoesNotThrowDeathLoop()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var stuckState = fsm.AddState<TrackedState>("Stuck");
            fsm.AddState<TrackedState>("End");
            fsm.Start("Stuck");

            Assert.DoesNotThrow(() =>
            {
                fsm.Update();
            });

            Assert.IsTrue(fsm.IsAlive);
            Assert.AreEqual(stuckState, fsm.CurrentState);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void PausedFSM_DoesNotProcessTransitions()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var a = fsm.AddState<TrackedState>("A");
            var b = fsm.AddState<TrackedState>("B");
            fsm.AddTransition<HFSMTransition>(a, b, () => true);

            fsm.Start("A");
            fsm.Paused = true;
            fsm.Update();
            Assert.AreEqual(a, fsm.CurrentState);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void ReachingExitState_AutomaticallyShutsDown()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var a = fsm.AddState<TrackedState>("A");
            fsm.AddTransition<HFSMTransition>(a, fsm.ExitState, () => true);
            fsm.Start("A");

            fsm.Update();

            Assert.IsFalse(fsm.IsAlive);
            Assert.IsNull(fsm.CurrentState);

            ReferencePool.Release(fsm);
        }

        [Test]
        public void NullConditionFunc_AlwaysFires()
        {
            var fsm = ReferencePool.Acquire<HFSM>();
            fsm.Initialize();
            var a = fsm.AddState<TrackedState>("A");
            var b = fsm.AddState<TrackedState>("B");
            fsm.AddTransition<HFSMTransition>(a, b, null);
            fsm.Start("A");

            fsm.Update();

            Assert.AreEqual(b, fsm.CurrentState);
            ReferencePool.Release(fsm);
        }

        #endregion
    }
}
