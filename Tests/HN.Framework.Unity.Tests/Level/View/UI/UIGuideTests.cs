#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Level.View.UI;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Level.View.UI
{
    /// <summary>
    /// UIGuide 基类的 EditMode 单元测试，覆盖步骤导航、回调触发和生命周期。
    /// </summary>
    [TestFixture]
    public class UIGuideTests
    {
        private GameObject? _parentGo;
        private GameObject? _guideGo;
        private ConcreteGuide? _guide;
        private GuideStep[]? _steps;

        [SetUp]
        public void SetUp()
        {
            _parentGo = new GameObject("TestParent");
            _parentGo.hideFlags = HideFlags.HideAndDontSave;

            _guideGo = new GameObject("TestGuide", typeof(RectTransform));
            _guideGo.hideFlags = HideFlags.HideAndDontSave;

            _guide = _guideGo.AddComponent<ConcreteGuide>();

            _steps = new GuideStep[]
            {
                new GuideStep("step_1", "Button_1", "第一步"),
                new GuideStep("step_2", "Button_2", "第二步"),
                new GuideStep("step_3", "Button_3", "第三步"),
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_guideGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_guideGo);
                _guideGo = null;
            }

            if (_parentGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_parentGo);
                _parentGo = null;
            }

            _guide = null;
            _steps = null;
        }

        // ── 初始状态 ──

        /// <summary>
        /// Show() 调用后当前步骤索引应为 0。
        /// </summary>
        [Test]
        public void Show_GuideWithSteps_CurrentStepIsZero()
        {
            _guide!.Show(_parentGo!.transform, _steps!);

            Assert.That(_guide.CurrentStepIndex, Is.EqualTo(0));
        }

        /// <summary>
        /// Show() 调用后 TotalSteps 应匹配步骤数组长度。
        /// </summary>
        [Test]
        public void Show_TotalSteps_MatchesArrayLength()
        {
            _guide!.Show(_parentGo!.transform, _steps!);

            Assert.That(_guide.TotalSteps, Is.EqualTo(3));
        }

        // ── 步骤导航 ──

        /// <summary>
        /// NextStep() 应使当前步骤索引递增 1。
        /// </summary>
        [Test]
        public void NextStep_IncrementsIndex()
        {
            _guide!.Show(_parentGo!.transform, _steps!);
            _guide.NextStep();

            Assert.That(_guide.CurrentStepIndex, Is.EqualTo(1));
        }

        /// <summary>
        /// 最后一步调用 NextStep() 应触发 OnGuideCompleted 回调并关闭面板。
        /// </summary>
        [Test]
        public void NextStep_AtLastStep_TriggersCompleted()
        {
            bool completed = false;
            _guide!.OnGuideCompleted = () => completed = true;

            var twoSteps = new GuideStep[]
            {
                new GuideStep("step_1", "Button_1", "第一步"),
                new GuideStep("step_2", "Button_2", "第二步"),
            };

            _guide.Show(_parentGo!.transform, twoSteps);
            _guide.NextStep(); // 进入步骤 1
            _guide.NextStep(); // 超出范围，触发完成

            Assert.That(completed, Is.True, "OnGuideCompleted should be invoked.");
            Assert.That(_guide.CurrentState, Is.EqualTo(UIPanelState.Closed));
        }

        /// <summary>
        /// PrevStep() 应使当前步骤索引递减 1。
        /// </summary>
        [Test]
        public void PrevStep_DecrementsIndex()
        {
            _guide!.Show(_parentGo!.transform, _steps!);
            _guide.NextStep(); // 步骤 1
            _guide.NextStep(); // 步骤 2
            _guide.PrevStep(); // 回到步骤 1

            Assert.That(_guide.CurrentStepIndex, Is.EqualTo(1));
        }

        /// <summary>
        /// 第一步调用 PrevStep() 不应改变当前步骤索引。
        /// </summary>
        [Test]
        public void PrevStep_AtFirstStep_DoesNotChange()
        {
            _guide!.Show(_parentGo!.transform, _steps!);
            _guide.PrevStep();

            Assert.That(_guide.CurrentStepIndex, Is.EqualTo(0));
        }

        // ── 生命周期 ──

        /// <summary>
        /// Show() 后调用 Close()，面板状态应为 Closed。
        /// </summary>
        [Test]
        public void Close_AfterShow_CurrentStepReset()
        {
            _guide!.Show(_parentGo!.transform, _steps!);
            _guide.Close();

            Assert.That(_guide.CurrentState, Is.EqualTo(UIPanelState.Closed));
        }

        // ── 步骤回调 ──

        /// <summary>
        /// 导航过程中 OnStepEnter 和 OnStepExit 应按正确顺序和参数调用。
        /// </summary>
        [Test]
        public void OnStepEnterExit_CalledOnNavigation()
        {
            _guide!.Show(_parentGo!.transform, _steps!);
            _guide.NextStep(); // 0 → 1
            _guide.NextStep(); // 1 → 2
            _guide.PrevStep(); // 2 → 1

            // 预期入场: 0, 1, 2, 1
            Assert.That(_guide.EnteredSteps, Is.EqualTo(new[] { 0, 1, 2, 1 }),
                "OnStepEnter should be called for each step in order.");

            // 预期退场: 0, 1, 2
            Assert.That(_guide.ExitedSteps, Is.EqualTo(new[] { 0, 1, 2 }),
                "OnStepExit should be called when leaving each step.");
        }

        // ── 步骤变更回调 ──

        /// <summary>
        /// 导航过程中 OnStepChanged 应按正确顺序触发。
        /// </summary>
        [Test]
        public void OnStepChanged_CalledOnNavigation()
        {
            var changedSteps = new List<int>();
            _guide!.OnStepChanged = (index) => changedSteps.Add(index);

            _guide.Show(_parentGo!.transform, _steps!);
            _guide.NextStep(); // 触发 OnStepChanged(1)
            _guide.NextStep(); // 触发 OnStepChanged(2)

            Assert.That(changedSteps, Is.EqualTo(new[] { 0, 1, 2 }),
                "OnStepChanged should be called for every step change including the initial step.");
        }

        // ── CanvasGroup 射线检测 ──

        /// <summary>
        /// Show() 调用后 CanvasGroup.blocksRaycasts 应为 true，阻挡下层交互。
        /// </summary>
        [Test]
        public void Show_BlocksRaycasts_IsTrue()
        {
            _guide!.Show(_parentGo!.transform, _steps!);

            Assert.That(_guide.CanvasGroup.blocksRaycasts, Is.True);
        }

        // ── 测试用具象子类 ──

        /// <summary>
        /// UIGuide 的具象测试子类，追踪 OnStepEnter / OnStepExit 调用。
        /// </summary>
        public class ConcreteGuide : UIGuide
        {
            /// <summary>
            /// 记录所有调用过 OnStepEnter 的步骤索引。
            /// </summary>
            public List<int> EnteredSteps { get; } = new List<int>();

            /// <summary>
            /// 记录所有调用过 OnStepExit 的步骤索引。
            /// </summary>
            public List<int> ExitedSteps { get; } = new List<int>();

            /// <inheritdoc />
            protected override void OnStepEnter(int stepIndex)
            {
                base.OnStepEnter(stepIndex);
                EnteredSteps.Add(stepIndex);
            }

            /// <inheritdoc />
            protected override void OnStepExit(int stepIndex)
            {
                base.OnStepExit(stepIndex);
                ExitedSteps.Add(stepIndex);
            }
        }
    }
}
