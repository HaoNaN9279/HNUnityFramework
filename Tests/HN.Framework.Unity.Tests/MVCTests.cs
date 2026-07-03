using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;
using HN.Framework.Core.Level.Logic;
using HN.Framework.Unity.Level.View.Binding;
using NUnit.Framework;

namespace HN.Framework.Unity.Tests
{
    /// <summary>
    /// EditMode 单元测试，覆盖 MVC 生命周期、IReadOnlyModel、PropertyBinder 与 GameWorld 验证。
    /// </summary>
    [TestFixture]
    public class MVCTests
    {
        #region Helper Types

        /// <summary>
        /// 模拟 ControllerUnit，跟踪各生命周期方法调用状态。
        /// </summary>
        private class MockControllerUnit : ControllerUnit
        {
            public bool InitializeWasCalled { get; private set; }
            public bool OnFirstFrameWasCalled { get; private set; }
            public int TickCount { get; private set; }
            public bool ClearWasCalled { get; private set; }

            public override void Initialize()
            {
                InitializeWasCalled = true;
            }

            public override void OnFirstFrame()
            {
                OnFirstFrameWasCalled = true;
            }

            public override void Tick()
            {
                TickCount++;
            }

            public override void LateTick() { }

            public override void Clear()
            {
                ClearWasCalled = true;
            }
        }

        /// <summary>
        /// 模拟 ModelUnit，跟踪各生命周期方法调用状态。
        /// </summary>
        private class MockModelUnit : ModelUnit
        {
            public bool InitializeWasCalled { get; private set; }
            public bool OnFirstFrameWasCalled { get; private set; }
            public int TickCount { get; private set; }
            public bool ClearWasCalled { get; private set; }

            public override void Initialize()
            {
                InitializeWasCalled = true;
            }

            public override void OnFirstFrame()
            {
                OnFirstFrameWasCalled = true;
            }

            public override void Tick()
            {
                TickCount++;
            }

            public override void LateTick() { }

            public override void Clear()
            {
                ClearWasCalled = true;
            }
        }

        /// <summary>
        /// 模拟 Controller，重写 OnInitialize 与 OnClear 以跟踪虚拟钩子调用。
        /// </summary>
        private class MockController : Controller
        {
            public bool OnInitializeWasCalled { get; private set; }
            public bool OnClearWasCalled { get; private set; }

            protected override void OnInitialize()
            {
                base.OnInitialize();
                OnInitializeWasCalled = true;
            }

            protected override void OnClear()
            {
                base.OnClear();
                OnClearWasCalled = true;
            }
        }

        /// <summary>
        /// 模拟 Model，重写 OnInitialize 与 OnClear 以跟踪虚拟钩子调用。
        /// </summary>
        private class MockModel : Model
        {
            public bool OnInitializeWasCalled { get; private set; }
            public bool OnClearWasCalled { get; private set; }

            protected override void OnInitialize()
            {
                base.OnInitialize();
                OnInitializeWasCalled = true;
            }

            protected override void OnClear()
            {
                base.OnClear();
                OnClearWasCalled = true;
            }
        }

        /// <summary>
        /// 用于测试 Unit.Initialize 在 OnInitialize 之前被调用顺序的 Controller。
        /// </summary>
        private class OrderTrackingController : Controller
        {
            public bool UnitInitializedBeforeOnInitialize { get; private set; }

            protected override void OnInitialize()
            {
                // Controller.Initialize 先遍历 unit.Initialize()，再调用 OnInitialize
                // 此处验证 ControllerUnits[0] 已初始化
                var units = ControllerUnits;
                if (units.Count > 0 && ((MockControllerUnit)units[0]).InitializeWasCalled)
                    UnitInitializedBeforeOnInitialize = true;

                base.OnInitialize();
            }
        }

        /// <summary>
        /// 模拟 PropertyBinder，实现 Bind / UnbindAll 以验证绑定合约。
        /// 仅支持 int 类型，用于测试场景。
        /// </summary>
        private class MockPropertyBinder : PropertyBinder
        {
            private readonly List<Action> unbindActions = new();

            public override void Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged)
            {
                var intSource = (IReadOnlyModel<int>)(object)source;
                var typedHandler = (Action<int>)(object)onValueChanged;
                intSource.OnValueChanged += typedHandler;
                unbindActions.Add(() => intSource.OnValueChanged -= typedHandler);
            }

            public override void UnbindAll()
            {
                foreach (var action in unbindActions)
                    action();
                unbindActions.Clear();
            }
        }

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void SetUp()
        {
            ReferencePool.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            ReferencePool.ClearAll();
        }

        #endregion

        #region Test 1: ControllerManager lifecycle

        /// <summary>
        /// 验证 ControllerManager 的完整生命周期：Register → Initialize → Tick → Unregister。
        /// </summary>
        [Test]
        public void ControllerManager_Lifecycle_RegisterInitializeTickUnregister()
        {
            var manager = new ControllerManager();
            var mockCtrl = ReferencePool.Acquire<MockController>();
            var mockUnit = ReferencePool.Acquire<MockControllerUnit>();
            mockCtrl.AddUnit(mockUnit);

            // Register & Initialize
            manager.RegisterController(mockCtrl);
            manager.Initialize();

            Assert.IsTrue(mockUnit.InitializeWasCalled, "ControllerUnit.Initialize should be called");
            Assert.IsTrue(mockCtrl.OnInitializeWasCalled, "Controller.OnInitialize should be called after unit init");

            // Tick
            manager.Tick();
            Assert.Greater(mockUnit.TickCount, 0, "TickCount should increase after Tick");

            // OnFirstFrame
            manager.OnFirstFrame();
            Assert.IsTrue(mockUnit.OnFirstFrameWasCalled, "OnFirstFrame should be called");

            // Unregister (calls Clear + ReferencePool.Release)
            manager.UnregisterController(mockCtrl);
            Assert.IsTrue(mockUnit.ClearWasCalled, "ControllerUnit.Clear should be called on unregister");
            Assert.IsTrue(mockCtrl.OnClearWasCalled, "Controller.OnClear should be called on unregister");
        }

        #endregion

        #region Test 2: GameWorld Initialize

        /// <summary>
        /// 验证 GameWorld.Initialize 调用了 ControllerManager.Initialize（间接验证）。
        /// </summary>
        [Test]
        public void GameWorld_Initialize_CallsControllerManagerInitialize()
        {
            var world = new GameWorld();
            var mockCtrl = ReferencePool.Acquire<MockController>();
            var mockUnit = ReferencePool.Acquire<MockControllerUnit>();
            mockCtrl.AddUnit(mockUnit);
            world.ControllerManager.RegisterController(mockCtrl);

            world.Initialize();
            Assert.IsTrue(mockUnit.InitializeWasCalled,
                "GameWorld.Initialize should call ControllerManager.Initialize → Controller.Initialize → unit.Initialize");

            world.Tick();
            Assert.Greater(mockUnit.TickCount, 0,
                "GameWorld.Tick should propagate to ControllerManager.Tick → Controller.Tick → unit.Tick");
        }

        #endregion

        #region Test 3: IReadOnlyModel value and event

        /// <summary>
        /// 验证 ReadOnlyModel 的值读写与 OnValueChanged 事件触发规则。
        /// </summary>
        [Test]
        public void ReadOnlyModel_ValueAndEvent()
        {
            var model = new ReadOnlyModel<int>(42);
            Assert.AreEqual(42, model.Value);

            int firedValue = 0;
            model.OnValueChanged += v => firedValue = v;

            model.SetValue(100);
            Assert.AreEqual(100, model.Value);
            Assert.AreEqual(100, firedValue, "OnValueChanged should fire when value changes");

            // 设置相同值，不应触发事件
            firedValue = -1;
            model.SetValue(100);
            Assert.AreEqual(-1, firedValue, "OnValueChanged should NOT fire when value is unchanged");
            Assert.AreEqual(100, model.Value);
        }

        #endregion

        #region Test 4: Model lifecycle with Unit

        /// <summary>
        /// 验证 Model 生命周期方法正确传递给 ModelUnit 和虚拟钩子。
        /// </summary>
        [Test]
        public void Model_Lifecycle_WithUnit()
        {
            var model = new MockModel();
            var mockUnit = ReferencePool.Acquire<MockModelUnit>();
            model.AddUnit(mockUnit);

            // Initialize
            model.Initialize();
            Assert.IsTrue(mockUnit.InitializeWasCalled, "ModelUnit.Initialize should be called");
            Assert.IsTrue(model.OnInitializeWasCalled, "Model.OnInitialize should be called after unit init");

            // Tick
            model.Tick();
            Assert.AreEqual(1, mockUnit.TickCount, "ModelUnit.Tick should be called");

            // Clear
            model.Clear();
            Assert.IsTrue(mockUnit.ClearWasCalled, "ModelUnit.Clear should be called");
            Assert.IsTrue(model.OnClearWasCalled, "Model.OnClear should be called");
        }

        #endregion

        #region Test 5: ControllerUnit pool integration

        /// <summary>
        /// 验证 Controller.RemoveUnit 将 ControllerUnit 释放到 ReferencePool 中。
        /// </summary>
        [Test]
        public void ControllerUnit_PoolIntegration_AddRemoveUnit()
        {
            var controller = ReferencePool.Acquire<MockController>();
            var unit = ReferencePool.Acquire<MockControllerUnit>();
            controller.AddUnit(unit);

            // RemoveUnit → ReferencePool.Release(unit)
            controller.RemoveUnit(unit);
            Assert.IsTrue(unit.ClearWasCalled, "RemoveUnit should call Clear on the unit");

            // 从池中重新获取，应为同一实例
            var reacquired = ReferencePool.Acquire<MockControllerUnit>();
            Assert.AreSame(unit, reacquired,
                "Re-acquired ControllerUnit should be the same instance (pooled)");

            ReferencePool.Release(controller);
            ReferencePool.Release(reacquired);
        }

        #endregion

        #region Test 6: ControllerManager PooledList lifecycle

        /// <summary>
        /// 验证 Uninitialize 后 PooledList 被释放并重新获取，控制器列表干净。
        /// </summary>
        [Test]
        public void ControllerManager_PooledList_UninitializeReacquire()
        {
            var manager = new ControllerManager();
            var c1 = ReferencePool.Acquire<MockController>();
            var c2 = ReferencePool.Acquire<MockController>();
            var c3 = ReferencePool.Acquire<MockController>();
            manager.RegisterController(c1);
            manager.RegisterController(c2);
            manager.RegisterController(c3);

            Assert.AreEqual(3, manager.Controllers.Count);

            // Uninitialize 遍历调用 Clear()，释放 PooledList，重新获取空列表
            manager.Uninitialize();

            Assert.IsTrue(c1.OnClearWasCalled, "c1.OnClear should be called");
            Assert.IsTrue(c2.OnClearWasCalled, "c2.OnClear should be called");
            Assert.IsTrue(c3.OnClearWasCalled, "c3.OnClear should be called");

            // 重新注册后列表应为全新
            var c4 = ReferencePool.Acquire<MockController>();
            manager.RegisterController(c4);
            Assert.AreEqual(1, manager.Controllers.Count,
                "After Uninitialize + re-register, count should be 1 (fresh list)");

            manager.Uninitialize();
        }

        #endregion

        #region Test 7: PropertyBinder contract

        /// <summary>
        /// 验证 PropertyBinder.Bind 与 UnbindAll 的基本合约。
        /// </summary>
        [Test]
        public void PropertyBinder_Contract_BindAndUnbind()
        {
            var binder = new MockPropertyBinder();
            var model = new ReadOnlyModel<int>(42);
            int captured = 0;

            binder.Bind(model, v => captured = v);
            model.SetValue(100);
            Assert.AreEqual(100, captured, "Bind callback should fire on value change");

            // UnbindAll 不应抛异常
            Assert.DoesNotThrow(() => binder.UnbindAll(),
                "UnbindAll should not throw after valid Bind");

            // 解绑后事件不应再触发
            captured = -1;
            model.SetValue(200);
            Assert.AreEqual(-1, captured, "Callback should not fire after UnbindAll");
        }

        #endregion

        #region Test 8: Controller virtual hooks order

        /// <summary>
        /// 验证 Controller.Initialize 中 Unit.Initialize 在 OnInitialize 之前调用。
        /// </summary>
        [Test]
        public void Controller_VirtualHooksOrder_UnitBeforeOnInitialize()
        {
            var controller = ReferencePool.Acquire<OrderTrackingController>();
            var unit = ReferencePool.Acquire<MockControllerUnit>();
            controller.AddUnit(unit);

            controller.Initialize();

            Assert.IsTrue(controller.UnitInitializedBeforeOnInitialize,
                "Unit.Initialize should be called BEFORE OnInitialize in Controller.Initialize");
        }

        #endregion
    }
}
