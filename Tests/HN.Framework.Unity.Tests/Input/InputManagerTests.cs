#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Unity.Capability.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Tests.Input
{
    /// <summary>
    /// InputManager EditMode tests. Uses InputTestFixture for hardware input simulation.
    /// </summary>
    [TestFixture]
    public class InputManagerTests : InputTestFixture
    {
        private InputActionAsset? _asset;
        private InputManager? _manager;
        private Keyboard? _keyboard;

        public override void Setup()
        {
            base.Setup();

            var json = CreateTestActionsJson();
            _asset = InputActionAsset.FromJson(json);
            _keyboard = InputSystem.AddDevice<Keyboard>("TestKeyboard");
            _asset.Enable();

            _manager = new InputManager(_asset);
        }

        public override void TearDown()
        {
            _manager?.Dispose();
            _manager = null;

            if (_asset != null)
            {
                _asset.Disable();
                _asset = null;
            }

            if (_keyboard != null && _keyboard.added)
            {
                InputSystem.RemoveDevice(_keyboard);
                _keyboard = null;
            }

            base.TearDown();
        }

        #region Helpers

        private static string CreateTestActionsJson()
        {
            return @"
            {
                ""maps"": [{
                    ""name"": ""Gameplay"",
                    ""id"": ""00000000-0000-0000-0000-000000000001"",
                    ""actions"": [
                        {
                            ""name"": ""Jump"",
                            ""type"": ""button"",
                            ""id"": ""00000000-0000-0000-0000-000000000010"",
                            ""expectedControlType"": ""Button""
                        },
                        {
                            ""name"": ""Move"",
                            ""type"": ""value"",
                            ""id"": ""00000000-0000-0000-0000-000000000011"",
                            ""expectedControlType"": ""Vector2""
                        }
                    ],
                    ""bindings"": [
                        {
                            ""name"": """",
                            ""id"": ""00000000-0000-0000-0000-000000000020"",
                            ""path"": ""<Keyboard>/space"",
                            ""action"": ""Jump""
                        }
                    ]
                }],
                ""controlSchemes"": []
            }";
        }

        private void SimulatePress(Key key)
        {
            Press(_keyboard![key]);
        }

        private void SimulateRelease(Key key)
        {
            Release(_keyboard![key]);
        }

        private void SimulateTap(Key key)
        {
            PressAndRelease(_keyboard![key]);
        }

        #endregion

        #region Constructor

        [Test]
        public void Constructor_WithNullAsset_ThrowsArgumentNullException()
        {
            Assert.That(() => new InputManager(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithValidAsset_CreatesInstance()
        {
            Assert.That(_manager, Is.Not.Null);
        }

        #endregion

        #region RegisterAction

        [Test]
        public void RegisterAction_ValidAction_CallbackFires()
        {
            InputContext? receivedContext = null;
            _manager!.RegisterAction("Jump", ctx => receivedContext = ctx);

            SimulateTap(Key.Space);

            Assert.That(receivedContext.HasValue, Is.True,
                "Expected callback to fire on key press.");
            Assert.That(receivedContext!.Value.ActionName, Is.EqualTo("Jump"));
        }

        [Test]
        public void RegisterAction_NullActionName_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _manager!.RegisterAction(null!, _ => { }),
                Throws.ArgumentNullException);
        }

        [Test]
        public void RegisterAction_EmptyActionName_ThrowsArgumentException()
        {
            Assert.That(
                () => _manager!.RegisterAction(string.Empty, _ => { }),
                Throws.ArgumentException);
        }

        [Test]
        public void RegisterAction_NullCallback_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _manager!.RegisterAction("Jump", null!),
                Throws.ArgumentNullException);
        }

        [Test]
        public void DoubleRegisterSameCallback_CallbackFiresOnce()
        {
            int fireCount = 0;
            void Handler(InputContext ctx) => fireCount++;

            _manager!.RegisterAction("Jump", Handler);

            // Measure single-registration count
            SimulateTap(Key.Space);
            int singleCount = fireCount;
            Assert.That(singleCount, Is.GreaterThan(0),
                "Sanity: single registration should fire.");
            fireCount = 0;

            // Register same callback again (should be deduplicated)
            _manager.RegisterAction("Jump", Handler);
            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.EqualTo(singleCount),
                $"Double registration should yield {singleCount} fires, got {fireCount}");
        }

        [Test]
        public void RegisterAction_ActionPhaseMapping_Correct()
        {
            var phases = new List<InputPhase>();
            _manager!.RegisterAction("Jump", ctx => phases.Add(ctx.Phase));

            SimulateTap(Key.Space);

            Assert.That(phases.Count, Is.GreaterThan(0),
                "Expected at least one phase event from key tap.");
        }

        #endregion

        #region UnregisterAction

        [Test]
        public void UnregisterAction_AfterRegister_CallbackStops()
        {
            int fireCount = 0;
            void Handler(InputContext ctx) => fireCount++;

            _manager!.RegisterAction("Jump", Handler);

            // Verify fires initially
            SimulateTap(Key.Space);
            Assert.That(fireCount, Is.GreaterThan(0),
                "Sanity: callback should fire before unregister.");
            fireCount = 0;

            // Unregister and verify it stops
            _manager.UnregisterAction("Jump", Handler);
            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.Zero,
                "After unregister, callback should not fire.");
        }

        [Test]
        public void UnregisterAction_NullActionName_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _manager!.UnregisterAction(null!, _ => { }),
                Throws.ArgumentNullException);
        }

        [Test]
        public void UnregisterAction_EmptyActionName_ThrowsArgumentException()
        {
            Assert.That(
                () => _manager!.UnregisterAction(string.Empty, _ => { }),
                Throws.ArgumentException);
        }

        [Test]
        public void UnregisterAction_NotRegistered_DoesNotThrow()
        {
            Assert.That(
                () => _manager!.UnregisterAction("Jump", _ => { }),
                Throws.Nothing);
        }

        #endregion

        #region EnableActionMap / DisableActionMap

        [Test]
        public void EnableActionMap_AfterDisable_ActionDoesNotFire()
        {
            int fireCount = 0;
            _manager!.RegisterAction("Jump", _ => fireCount++);

            _manager.DisableActionMap("Gameplay");

            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.Zero,
                "When ActionMap is disabled, callbacks should not fire.");
        }

        [Test]
        public void EnableActionMap_ReEnablesActions()
        {
            int fireCount = 0;
            _manager!.RegisterAction("Jump", _ => fireCount++);

            _manager.DisableActionMap("Gameplay");
            _manager.EnableActionMap("Gameplay");

            fireCount = 0;
            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.GreaterThan(0),
                "After re-enabling ActionMap, callbacks should fire.");
        }

        [Test]
        public void EnableActionMap_NonexistentMap_DoesNotThrow()
        {
            Assert.That(
                () => _manager!.EnableActionMap("NonExistent"),
                Throws.Nothing);
        }

        [Test]
        public void DisableActionMap_NonexistentMap_DoesNotThrow()
        {
            Assert.That(
                () => _manager!.DisableActionMap("NonExistent"),
                Throws.Nothing);
        }

        #endregion

        #region Blocker

        [Test]
        public void BlockerProperty_ReturnsNonNull()
        {
            Assert.That(_manager!.Blocker, Is.Not.Null);
        }

        [Test]
        public void Blocker_WhenBlocked_CallbackDoesNotFire()
        {
            int fireCount = 0;
            _manager!.RegisterAction("Jump", _ => fireCount++);

            _manager.Blocker.Push(this, 0);
            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.Zero,
                "When blocked, callbacks should not fire.");

            _manager.Blocker.Pop(this);
            fireCount = 0;
            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.GreaterThan(0),
                "After unblocking, callbacks should fire.");
        }

        #endregion

        #region Dispose

        [Test]
        public void Dispose_ClearsAllSubscriptions()
        {
            int fireCount = 0;
            _manager!.RegisterAction("Jump", _ => fireCount++);

            _manager.Dispose();

            SimulateTap(Key.Space);

            Assert.That(fireCount, Is.Zero,
                "After Dispose, callbacks should not fire.");
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            Assert.That(() => _manager!.Dispose(), Throws.Nothing);
            Assert.That(() => _manager.Dispose(), Throws.Nothing);
        }

        #endregion

        #region SetControlScheme

        [Test]
        public void SetControlScheme_DoesNotThrow()
        {
            Assert.That(
                () => _manager!.SetControlScheme("KeyboardMouse"),
                Throws.Nothing);
        }

        #endregion
    }
}
