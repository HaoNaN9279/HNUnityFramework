#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Core.Tests.Capability.UI
{
    [TestFixture]
    public class IUIManagerContractTests
    {
        /// <summary>
        /// Push 和 Pop 正常调用不抛出异常。
        /// </summary>
        [Test]
        public void PushPop_CanBeCalled_NoException()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.Push("TestPanel"));
            Assert.DoesNotThrow(() => manager.Pop());
        }

        /// <summary>
        /// Push(null) 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void Push_Null_ThrowsArgumentNullException()
        {
            MockUIManager manager = new MockUIManager();

            Assert.Throws<ArgumentNullException>(() => manager.Push(null!));
        }

        /// <summary>
        /// Push(空字符串) 视为正常参数，不应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void Push_EmptyString_DoesNotThrowArgNull()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.Push(string.Empty));
        }

        /// <summary>
        /// 空栈上调用 Pop 不应抛出异常。
        /// </summary>
        [Test]
        public void Pop_OnEmptyStack_NoException()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.Pop());
        }

        /// <summary>
        /// Show 和 Hide 正常调用不抛出异常。
        /// </summary>
        [Test]
        public void ShowHide_CanBeCalled_NoException()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.Show("TestPanel"));
            Assert.DoesNotThrow(() => manager.Hide("TestPanel"));
        }

        /// <summary>
        /// Show(null) 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void Show_Null_ThrowsArgumentNullException()
        {
            MockUIManager manager = new MockUIManager();

            Assert.Throws<ArgumentNullException>(() => manager.Show(null!));
        }

        /// <summary>
        /// Hide(null) 应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void Hide_Null_ThrowsArgumentNullException()
        {
            MockUIManager manager = new MockUIManager();

            Assert.Throws<ArgumentNullException>(() => manager.Hide(null!));
        }
    }

    /// <summary>
    /// IUIManager 的测试桩实现。
    /// </summary>
    internal class MockUIManager : IUIManager
    {
        /// <summary>
        /// 已 Push 的栈中面板数量。
        /// </summary>
        public int StackCount { get; private set; }

        /// <summary>
        /// 最后弹出的面板路径。
        /// </summary>
        public string? LastPoppedPanel { get; private set; }

        /// <summary>
        /// 推入一个面板到栈顶。
        /// </summary>
        public void Push(string panelPath)
        {
            if (panelPath == null)
                throw new ArgumentNullException(nameof(panelPath));
            StackCount++;
        }

        /// <summary>
        /// 弹出栈顶面板。
        /// </summary>
        public void Pop()
        {
            if (StackCount > 0)
            {
                LastPoppedPanel = "popped";
                StackCount--;
            }
        }

        /// <summary>
        /// 显示一个非栈面板（模态层）。
        /// </summary>
        public void Show(string panelPath)
        {
            if (panelPath == null)
                throw new ArgumentNullException(nameof(panelPath));
        }

        /// <summary>
        /// 隐藏一个非栈面板（模态层）。
        /// </summary>
        public void Hide(string panelPath)
        {
            if (panelPath == null)
                throw new ArgumentNullException(nameof(panelPath));
        }
    }
}
