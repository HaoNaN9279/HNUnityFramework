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

        // ───────── 新增方法测试 ─────────

        /// <summary>
        /// ShowDialog 确认回调可被触发。
        /// </summary>
        [Test]
        public void ShowDialog_Callback_CanBeInvoked()
        {
            MockUIManager manager = new MockUIManager();
            bool confirmCalled = false;
            bool cancelCalled = false;
            Action onConfirm = () => confirmCalled = true;
            Action onCancel = () => cancelCalled = true;

            manager.ShowDialog("TestDialog", onConfirm, onCancel);
            Assert.That(manager.LastDialogPath, Is.EqualTo("TestDialog"));
            Assert.That(manager.LastOnConfirm, Is.SameAs(onConfirm));
            Assert.That(manager.LastOnCancel, Is.SameAs(onCancel));

            // 模拟触发确认回调
            manager.LastOnConfirm?.Invoke();
            Assert.That(confirmCalled, Is.True);
        }

        /// <summary>
        /// ShowToast 存储参数。
        /// </summary>
        [Test]
        public void ShowToast_StoresParameters()
        {
            MockUIManager manager = new MockUIManager();

            manager.ShowToast("Hello World", 3f);

            Assert.That(manager.LastToastMessage, Is.EqualTo("Hello World"));
            Assert.That(manager.LastToastDuration, Is.EqualTo(3f));
        }

        /// <summary>
        /// StartGuide 正常调用不抛出异常。
        /// </summary>
        [Test]
        public void StartGuide_DoesNotThrow()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.StartGuide("guide_intro"));
            Assert.That(manager.LastGuideId, Is.EqualTo("guide_intro"));
        }

        /// <summary>
        /// StopGuide 正常调用不抛出异常。
        /// </summary>
        [Test]
        public void StopGuide_DoesNotThrow()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.StopGuide());
        }

        /// <summary>
        /// PushAsync 正常调用不抛出异常。
        /// </summary>
        [Test]
        public void PushAsync_DoesNotThrow()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.PushAsync("AsyncPanel"));
        }

        /// <summary>
        /// ShowAsync 正常调用不抛出异常。
        /// </summary>
        [Test]
        public void ShowAsync_DoesNotThrow()
        {
            MockUIManager manager = new MockUIManager();

            Assert.DoesNotThrow(() => manager.ShowAsync("AsyncModal"));
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

        // ───────── 原有方法 ─────────

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

        // ───────── 新增方法 ─────────

        /// <summary>
        /// 最后显示的对话框路径。
        /// </summary>
        public string? LastDialogPath { get; private set; }

        /// <summary>
        /// 最后注册的确认回调。
        /// </summary>
        public Action? LastOnConfirm { get; private set; }

        /// <summary>
        /// 最后注册的取消回调。
        /// </summary>
        public Action? LastOnCancel { get; private set; }

        /// <summary>
        /// 显示对话框。
        /// </summary>
        public void ShowDialog(string panelPath, Action? onConfirm = null, Action? onCancel = null)
        {
            LastDialogPath = panelPath;
            LastOnConfirm = onConfirm;
            LastOnCancel = onCancel;
        }

        /// <summary>
        /// 最后显示的 Toast 消息。
        /// </summary>
        public string? LastToastMessage { get; private set; }

        /// <summary>
        /// 最后显示的 Toast 持续时间。
        /// </summary>
        public float LastToastDuration { get; private set; }

        /// <summary>
        /// 显示 Toast。
        /// </summary>
        public void ShowToast(string message, float duration = 2f)
        {
            LastToastMessage = message;
            LastToastDuration = duration;
        }

        /// <summary>
        /// 最后启动的引导 ID。
        /// </summary>
        public string? LastGuideId { get; private set; }

        /// <summary>
        /// 启动引导。
        /// </summary>
        public void StartGuide(string guideId, Action? onCompleted = null)
        {
            LastGuideId = guideId;
        }

        /// <summary>
        /// 停止引导。
        /// </summary>
        public void StopGuide()
        {
        }

        /// <summary>
        /// 异步推入面板。
        /// </summary>
        public void PushAsync(string panelPath)
        {
        }

        /// <summary>
        /// 异步显示面板。
        /// </summary>
        public void ShowAsync(string panelPath)
        {
        }
    }
}
