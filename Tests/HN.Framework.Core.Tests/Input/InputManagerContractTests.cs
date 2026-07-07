#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using HN.Framework.Core.Capability.Input;

namespace HN.Framework.Core.Tests.Input
{
    /// <summary>
    /// IInputManager 接口的模拟实现，用于合约测试。
    /// </summary>
    internal sealed class MockInputManager : IInputManager
    {
        private readonly Dictionary<string, List<Action<InputContext>>> _callbacks = new();
        private readonly Dictionary<string, bool> _actionMaps = new();
        private string _controlScheme = string.Empty;

        /// <summary>
        /// 获取当前控制方案。
        /// </summary>
        public string ControlScheme => _controlScheme;

        /// <summary>
        /// 注册输入动作回调。
        /// </summary>
        public void RegisterAction(string actionName, Action<InputContext> callback)
        {
            if (!_callbacks.ContainsKey(actionName))
                _callbacks[actionName] = new List<Action<InputContext>>();
            _callbacks[actionName].Add(callback);
        }

        /// <summary>
        /// 注销输入动作回调。
        /// </summary>
        public void UnregisterAction(string actionName, Action<InputContext> callback)
        {
            if (_callbacks.TryGetValue(actionName, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0)
                    _callbacks.Remove(actionName);
            }
        }

        /// <summary>
        /// 启用指定的 ActionMap。
        /// </summary>
        public void EnableActionMap(string mapName)
        {
            _actionMaps[mapName] = true;
        }

        /// <summary>
        /// 禁用指定的 ActionMap。
        /// </summary>
        public void DisableActionMap(string mapName)
        {
            _actionMaps[mapName] = false;
        }

        /// <summary>
        /// 切换控制方案。
        /// </summary>
        public void SetControlScheme(string scheme)
        {
            _controlScheme = scheme;
        }

        /// <summary>
        /// 获取指定 ActionMap 是否已启用。
        /// </summary>
        public bool IsActionMapEnabled(string mapName)
        {
            return _actionMaps.TryGetValue(mapName, out bool enabled) && enabled;
        }

        /// <summary>
        /// 获取指定动作已注册的回调数量。
        /// </summary>
        public int GetRegisteredCallbackCount(string actionName)
        {
            return _callbacks.TryGetValue(actionName, out var list) ? list.Count : 0;
        }

        /// <summary>
        /// 获取输入屏蔽器实例。
        /// </summary>
        public IInputBlocker Blocker { get; } = new MockInputBlocker();
    }

    /// <summary>
    /// IInputManager 接口合约测试。
    /// 验证接口契约而非具体实现。
    /// </summary>
    [TestFixture]
    public class InputManagerContractTests
    {
        private MockInputManager _manager = null!;

        /// <summary>
        /// 每个测试前创建新的 MockInputManager 实例。
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _manager = new MockInputManager();
        }

        /// <summary>
        /// 每个测试后清理资源。
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _manager = null!;
        }

        /// <summary>
        /// RegisterAction 应添加回调，增加注册计数。
        /// </summary>
        [Test]
        public void RegisterAction_AddsCallback_IncreasesCount()
        {
            Action<InputContext> callback = ctx => { };
            _manager.RegisterAction("Jump", callback);
            Assert.That(_manager.GetRegisteredCallbackCount("Jump"), Is.EqualTo(1));
        }

        /// <summary>
        /// UnregisterAction 应移除回调，减少注册计数。
        /// </summary>
        [Test]
        public void UnregisterAction_RemovesCallback_DecreasesCount()
        {
            Action<InputContext> callback = ctx => { };
            _manager.RegisterAction("Jump", callback);
            _manager.UnregisterAction("Jump", callback);
            Assert.That(_manager.GetRegisteredCallbackCount("Jump"), Is.EqualTo(0));
        }

        /// <summary>
        /// EnableActionMap 和 DisableActionMap 应切换 ActionMap 状态。
        /// </summary>
        [Test]
        public void EnableActionMap_DisableActionMap_TogglesState()
        {
            _manager.EnableActionMap("Gameplay");
            Assert.That(_manager.IsActionMapEnabled("Gameplay"), Is.True);
            _manager.DisableActionMap("Gameplay");
            Assert.That(_manager.IsActionMapEnabled("Gameplay"), Is.False);
            _manager.EnableActionMap("Gameplay");
            Assert.That(_manager.IsActionMapEnabled("Gameplay"), Is.True);
        }

        /// <summary>
        /// SetControlScheme 应存储控制方案值。
        /// </summary>
        [Test]
        public void SetControlScheme_StoresScheme()
        {
            _manager.SetControlScheme("Gamepad");
            Assert.That(_manager.ControlScheme, Is.EqualTo("Gamepad"));
        }

        /// <summary>
        /// Blocker 属性应返回非空 IInputBlocker 实例。
        /// </summary>
        [Test]
        public void Blocker_Property_ReturnsNonNull()
        {
            Assert.That(_manager.Blocker, Is.Not.Null);
        }
    }
}
