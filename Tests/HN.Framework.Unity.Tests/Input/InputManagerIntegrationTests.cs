#nullable enable

using System;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Driver;
using HN.Framework.Unity.Capability.Input;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Tests.Input
{
    /// <summary>
    /// InputManager 与 GameWorld 的集成测试。
    /// 验证 InputManager 在 GameWorld 上的注入、替换和 Dispose 安全性。
    /// </summary>
    [TestFixture]
    public class InputManagerIntegrationTests
    {
        private GameWorld? _world;

        [SetUp]
        public void SetUp()
        {
            _world = new GameWorld();
        }

        [TearDown]
        public void TearDown()
        {
            (_world?.InputManager as IDisposable)?.Dispose();
            _world = null;
        }

        #region Integration: GameWorld.InputManager

        /// <summary>
        /// 验证 GameWorld 创建后 InputManager 属性默认为 null。
        /// </summary>
        [Test]
        public void InputManager_AfterGameWorldCreation_IsNull()
        {
            Assert.That(_world!.InputManager, Is.Null,
                "InputManager should be null before injection.");
        }

        /// <summary>
        /// 验证可以将 InputManager 设置到 GameWorld 上并能正确读取。
        /// </summary>
        [Test]
        public void InputManager_SetAndGet_Succeeds()
        {
            var emptyAsset = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            var manager = new InputManager(emptyAsset);

            _world!.InputManager = manager;

            Assert.That(_world.InputManager, Is.SameAs(manager),
                "InputManager should be the same instance that was set.");
        }

        /// <summary>
        /// 验证可以替换 GameWorld 上的 InputManager 实例。
        /// </summary>
        [Test]
        public void InputManager_SetAndReplace_Succeeds()
        {
            var emptyAsset1 = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            var emptyAsset2 = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            var manager1 = new InputManager(emptyAsset1);
            var manager2 = new InputManager(emptyAsset2);

            _world!.InputManager = manager1;
            Assert.That(_world.InputManager, Is.SameAs(manager1));

            _world.InputManager = manager2;
            Assert.That(_world.InputManager, Is.SameAs(manager2),
                "InputManager should be replaceable.");

            // Cleanup manager1 since it was replaced
            manager1.Dispose();
        }

        /// <summary>
        /// 验证当 InputManager 未设置时，Dispose 调用不会抛出异常。
        /// </summary>
        [Test]
        public void Dispose_WhenInputManagerIsNull_DoesNotThrow()
        {
            Assert.That(_world!.InputManager, Is.Null,
                "Sanity: InputManager should be null.");

            Assert.That(
                () => (_world.InputManager as IDisposable)?.Dispose(),
                Throws.Nothing,
                "Dispose on null InputManager should be safe.");
        }

        /// <summary>
        /// 验证 InputManager 设置后可以正确 Dispose。
        /// </summary>
        [Test]
        public void Dispose_WhenInputManagerIsSet_DoesNotThrow()
        {
            var emptyAsset = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            var manager = new InputManager(emptyAsset);
            _world!.InputManager = manager;

            Assert.That(
                () => (_world.InputManager as IDisposable)?.Dispose(),
                Throws.Nothing,
                "Dispose on set InputManager should be safe.");
        }

        /// <summary>
        /// 验证 IInputManager 接口通过 Blocker 属性可以正常访问。
        /// </summary>
        [Test]
        public void InputManager_Blocker_IsAccessible()
        {
            var emptyAsset = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            var manager = new InputManager(emptyAsset);
            _world!.InputManager = manager;

            Assert.That(_world.InputManager!.Blocker, Is.Not.Null,
                "Blocker should be accessible through the interface.");
        }

        #endregion
    }
}
