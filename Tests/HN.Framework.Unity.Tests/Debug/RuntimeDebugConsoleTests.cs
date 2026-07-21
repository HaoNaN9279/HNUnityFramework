#nullable enable

using System;
using System.Reflection;
using CoreDebugHub = HN.Framework.Core.Capability.Debug.DebugHub;
using HN.Framework.Core.Driver.Common.Debug;
using HN.Framework.Unity.Capability.Debug;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Debug
{
    /// <summary>
    /// RuntimeDebugConsole 的 EditMode 单元测试，验证组件创建、DebugHub 注入和命令执行委托。
    /// </summary>
    [TestFixture]
    public class RuntimeDebugConsoleTests
    {
        #region Helper Types

        /// <summary>
        /// 用于测试命令执行跟踪的模拟命令。
        /// </summary>
        private class MockCommand : IDebugCommand
        {
            public string Name { get; }
            public string Description => "Mock command for testing";
            public bool WasExecuted { get; private set; }
            public string[]? ReceivedArgs { get; private set; }
            public int ExecuteCount { get; private set; }

            public MockCommand(string name)
            {
                Name = name;
            }

            public void Execute(string[] args)
            {
                WasExecuted = true;
                ReceivedArgs = args;
                ExecuteCount++;
            }

            public void Reset()
            {
                WasExecuted = false;
                ReceivedArgs = null;
                ExecuteCount = 0;
            }
        }

        #endregion

        private GameObject? _testRoot;
        private RuntimeDebugConsole? _console;

        [SetUp]
        public void SetUp()
        {
            _testRoot = new GameObject("TestRoot", typeof(RuntimeDebugConsole));
            _testRoot.hideFlags = HideFlags.HideAndDontSave;
            _console = _testRoot.GetComponent<RuntimeDebugConsole>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_testRoot != null)
                UnityEngine.Object.DestroyImmediate(_testRoot);
            _testRoot = null;
            _console = null;
        }

        [Test]
        public void Constructor_DoesNotThrow()
        {
            Assert.That(_console, Is.Not.Null);
            Assert.That(_console.IsVisible, Is.False);
        }

        [Test]
        public void DebugHub_CanBeSetAndGet()
        {
            var hub = new CoreDebugHub();
            _console!.DebugHub = hub;

            Assert.That(_console.DebugHub, Is.SameAs(hub));
        }

        [Test]
        public void DebugHub_DefaultsToNull()
        {
            Assert.That(_console!.DebugHub, Is.Null);
        }

        [Test]
        public void Start_CreatesUI_DoesNotThrow()
        {
            // 注入 DebugHub 以避免 FindObjectOfType 依赖
            _console!.DebugHub = new CoreDebugHub();

            Assert.DoesNotThrow(() =>
            {
                InvokeStart(_console);
            });

            // 验证 UI 已创建（Canvas 应该存在但初始不可见）
            var canvasTransform = _testRoot!.transform.Find("RuntimeDebugCanvas");
            Assert.That(canvasTransform, Is.Not.Null);
            Assert.That(canvasTransform.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void ExecuteCommand_DelegatesToDebugHub()
        {
            // 准备 DebugHub 和模拟命令
            var hub = new CoreDebugHub();
            var mockCmd = new MockCommand("test.hello");
            hub.RegisterCommand(mockCmd);

            _console!.DebugHub = hub;
            InvokeStart(_console);

            // 通过反射设置 InputField 文本并调用 ExecuteInput
            SetInputFieldText(_console, "test.hello arg1 arg2");
            InvokeExecuteInput(_console);

            Assert.That(mockCmd.WasExecuted, Is.True, "Expected mock command to be executed");
            Assert.That(mockCmd.ReceivedArgs, Is.Not.Null);
            Assert.That(mockCmd.ReceivedArgs, Is.EqualTo(new[] { "arg1", "arg2" }));
        }

        [Test]
        public void ExecuteCommand_UnknownCommand_DoesNotThrow()
        {
            var hub = new CoreDebugHub();
            _console!.DebugHub = hub;
            InvokeStart(_console);

            SetInputFieldText(_console, "nonexistent.command");
            Assert.DoesNotThrow(() => InvokeExecuteInput(_console));
        }

        [Test]
        public void ExecuteCommand_WithNoArgs_DelegatesCorrectly()
        {
            var hub = new CoreDebugHub();
            var mockCmd = new MockCommand("ping");
            hub.RegisterCommand(mockCmd);

            _console!.DebugHub = hub;
            InvokeStart(_console);

            SetInputFieldText(_console, "ping");
            InvokeExecuteInput(_console);

            Assert.That(mockCmd.WasExecuted, Is.True);
            Assert.That(mockCmd.ReceivedArgs, Is.Not.Null);
            Assert.That(mockCmd.ReceivedArgs.Length, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteCommand_EmptyInput_DoesNothing()
        {
            var hub = new CoreDebugHub();
            _console!.DebugHub = hub;
            InvokeStart(_console);

            // 空字符串和纯空格都不应触发执行
            SetInputFieldText(_console, "");
            Assert.DoesNotThrow(() => InvokeExecuteInput(_console));

            SetInputFieldText(_console, "   ");
            Assert.DoesNotThrow(() => InvokeExecuteInput(_console));
        }

        [Test]
        public void ExecuteCommand_WithoutDebugHub_DoesNotThrow()
        {
            InvokeStart(_console!);

            // 未设置 DebugHub 时不应崩溃
            SetInputFieldText(_console, "any.command");
            Assert.DoesNotThrow(() => InvokeExecuteInput(_console));
        }

        [Test]
        public void AppendText_AppendsWithColorTag()
        {
            var hub = new CoreDebugHub();
            _console!.DebugHub = hub;
            InvokeStart(_console);

            // 通过 AppendText 添加彩色文本
            var appendMethod = typeof(RuntimeDebugConsole).GetMethod("AppendText",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(appendMethod, Is.Not.Null);

            appendMethod!.Invoke(_console, new object[] { "Hello World\n", Color.cyan });
            appendMethod.Invoke(_console, new object[] { "Error\n", Color.red });

            // 验证输出文本包含颜色标签
            var outputTextField = typeof(RuntimeDebugConsole)
                .GetField("_outputText", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(outputTextField, Is.Not.Null);

            var outputText = outputTextField!.GetValue(_console) as TMPro.TextMeshProUGUI;
            Assert.That(outputText, Is.Not.Null);
            Assert.That(outputText!.text, Does.Contain("<color=#"));
            Assert.That(outputText.text, Does.Contain("Hello World"));
            Assert.That(outputText.text, Does.Contain("Error"));
        }

        #region Reflection Helpers

        private static void InvokeStart(RuntimeDebugConsole console)
        {
            var method = typeof(RuntimeDebugConsole).GetMethod("Start",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Start method not found");
            method!.Invoke(console, null);
        }

        private static void InvokeExecuteInput(RuntimeDebugConsole console)
        {
            var method = typeof(RuntimeDebugConsole).GetMethod("ExecuteInput",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "ExecuteInput method not found");
            method!.Invoke(console, null);
        }

        private static void SetInputFieldText(RuntimeDebugConsole console, string text)
        {
            var field = typeof(RuntimeDebugConsole).GetField("_inputField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, "_inputField field not found");

            var inputField = field!.GetValue(console) as TMPro.TMP_InputField;
            Assert.That(inputField, Is.Not.Null, "InputField not created. Did Start() run?");

            inputField!.text = text;
        }

        #endregion
    }
}
