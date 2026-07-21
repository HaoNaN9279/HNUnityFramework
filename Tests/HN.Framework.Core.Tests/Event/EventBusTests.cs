#nullable enable

using System;
using NUnit.Framework;
using HN.Framework.Core.Capability.Event;

namespace HN.Framework.Core.Tests.Event
{
    [TestFixture]
    public class EventBusTests
    {
        private EventBus _bus = null!;

        /// <summary>
        /// 测试用事件类型：包含一个整数值和一个可选标签
        /// </summary>
        private readonly struct TestEvent
        {
            public readonly int Value;
            public readonly string? Label;

            public TestEvent(int value, string? label = null)
            {
                Value = value;
                Label = label;
            }
        }

        /// <summary>
        /// 测试用事件类型：包含一个字符串数据
        /// </summary>
        private readonly struct OtherEvent
        {
            public readonly string Data;

            public OtherEvent(string data) => Data = data;
        }

        // Test 1, 2, 14, 15 共享的 handler 状态
        private int _receivedValue;

        // Test 3 三个独立 handler 状态
        private int _counterA;
        private int _counterB;
        private int _counterC;

        // Test 4 捕获完整 struct 数据
        private TestEvent _capturedEvent;

        // Test 5 隔离总线状态
        private int _bus1Value;
        private int _bus2Value;

        // Test 6 不同类型隔离
        private int _testEventCallCount;
        private int _otherEventCallCount;

        // Test 8 重复订阅计数
        private int _duplicateCount;

        // Test 11 default 值接收
        private TestEvent _defaultEvent;

        // Test 12 异常传播中正常 handler 计数
        private int _handler1Count;
        private int _handler3Count;

        // Test 16 后台线程发布
        private int _bgCount;

        // Test 17 并发订阅
        private int _concurrentCount1;
        private int _concurrentCount2;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _receivedValue = 0;
            _counterA = 0;
            _counterB = 0;
            _counterC = 0;
            _capturedEvent = default;
            _bus1Value = 0;
            _bus2Value = 0;
            _testEventCallCount = 0;
            _otherEventCallCount = 0;
            _duplicateCount = 0;
            _defaultEvent = default;
            _handler1Count = 0;
            _handler3Count = 0;
            _bgCount = 0;
            _concurrentCount1 = 0;
            _concurrentCount2 = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _bus = null!;
        }

        // ============ Handler Methods ============

        private void OnTestEvent(TestEvent e) => _receivedValue = e.Value;
        private void OnTestEventA(TestEvent e) => _counterA++;
        private void OnTestEventB(TestEvent e) => _counterB++;
        private void OnTestEventC(TestEvent e) => _counterC++;
        private void OnCaptureTestEvent(TestEvent e) => _capturedEvent = e;
        private void OnBus1Event(TestEvent e) => _bus1Value = e.Value;
        private void OnBus2Event(TestEvent e) => _bus2Value = e.Value;
        private void OnTestEventOnly(TestEvent e) => _testEventCallCount++;
        private void OnOtherEvent(OtherEvent e) => _otherEventCallCount++;
        private void OnDuplicateEvent(TestEvent e) => _duplicateCount++;
        private void OnDefaultEvent(TestEvent e) => _defaultEvent = e;
        private void OnHandler1(TestEvent e) => _handler1Count++;
        private void OnHandler2(TestEvent e) => throw new InvalidOperationException("Intentional failure");
        private void OnHandler3(TestEvent e) => _handler3Count++;
        private void OnBackgroundEvent(TestEvent e) => _bgCount = e.Value;
        private void OnConcurrentEvent1(TestEvent e) => _concurrentCount1++;
        private void OnConcurrentEvent2(TestEvent e) => _concurrentCount2++;

        // ============ Test Cases ============

        /// <summary>
        /// 订阅后发布事件，handler 应被正确调用并接收到数据
        /// </summary>
        [Test]
        public void Subscribe_Publish_HandlerCalled()
        {
            _bus.Subscribe<TestEvent>(OnTestEvent);
            _bus.Publish(new TestEvent(42));

            Assert.AreEqual(42, _receivedValue);
        }

        /// <summary>
        /// 取消订阅后发布事件，handler 不应被调用
        /// </summary>
        [Test]
        public void Subscribe_Unsubscribe_HandlerNotCalled()
        {
            _bus.Subscribe<TestEvent>(OnTestEvent);
            _bus.Unsubscribe<TestEvent>(OnTestEvent);
            _bus.Publish(new TestEvent(42));

            Assert.AreEqual(0, _receivedValue);
        }

        /// <summary>
        /// 多个订阅者订阅同一事件，发布后所有 handler 都应被调用
        /// </summary>
        [Test]
        public void MultipleSubscribers_AllCalled()
        {
            _bus.Subscribe<TestEvent>(OnTestEventA);
            _bus.Subscribe<TestEvent>(OnTestEventB);
            _bus.Subscribe<TestEvent>(OnTestEventC);
            _bus.Publish(new TestEvent(1));

            Assert.AreEqual(1, _counterA);
            Assert.AreEqual(1, _counterB);
            Assert.AreEqual(1, _counterC);
        }

        /// <summary>
        /// 发布包含多个字段的只读结构体，handler 应能接收到所有字段数据
        /// </summary>
        [Test]
        public void Publish_ReadonlyStruct_DataReceived()
        {
            _bus.Subscribe<TestEvent>(OnCaptureTestEvent);
            _bus.Publish(new TestEvent(42, "hello"));

            Assert.AreEqual(42, _capturedEvent.Value);
            Assert.AreEqual("hello", _capturedEvent.Label);
        }

        /// <summary>
        /// 不同 EventBus 实例之间状态完全隔离
        /// </summary>
        [Test]
        public void MultipleInstances_IsolatedState()
        {
            var bus1 = new EventBus();
            var bus2 = new EventBus();

            bus1.Subscribe<TestEvent>(OnBus1Event);
            bus2.Subscribe<TestEvent>(OnBus2Event);

            bus1.Publish(new TestEvent(100));

            Assert.AreEqual(100, _bus1Value);
            Assert.AreEqual(0, _bus2Value);
        }

        /// <summary>
        /// 不同类型的事件订阅互相隔离
        /// </summary>
        [Test]
        public void Subscribe_DifferentTypes_TypeIsolation()
        {
            _bus.Subscribe<TestEvent>(OnTestEventOnly);
            _bus.Subscribe<OtherEvent>(OnOtherEvent);
            _bus.Publish(new TestEvent(1));

            Assert.AreEqual(1, _testEventCallCount);
            Assert.AreEqual(0, _otherEventCallCount);
        }

        /// <summary>
        /// 发布无订阅者的事件不应抛出异常
        /// </summary>
        [Test]
        public void Publish_ZeroSubscribers_NoError()
        {
            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent(1)));
        }

        /// <summary>
        /// 同一个 handler 被订阅两次，发布事件后应被调用两次
        /// </summary>
        [Test]
        public void Subscribe_DuplicateHandler_CalledMultipleTimes()
        {
            _bus.Subscribe<TestEvent>(OnDuplicateEvent);
            _bus.Subscribe<TestEvent>(OnDuplicateEvent);
            _bus.Publish(new TestEvent(1));

            Assert.AreEqual(2, _duplicateCount);
        }

        /// <summary>
        /// 订阅 null handler 应抛出 ArgumentNullException
        /// </summary>
        [Test]
        public void Subscribe_NullHandler_ThrowsArgNull()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Subscribe<TestEvent>(null!));
        }

        /// <summary>
        /// 取消订阅 null handler 应抛出 ArgumentNullException
        /// </summary>
        [Test]
        public void Unsubscribe_NullHandler_ThrowsArgNull()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Unsubscribe<TestEvent>(null!));
        }

        /// <summary>
        /// 发布默认值结构体，handler 应接收到各字段为默认值的数据
        /// </summary>
        [Test]
        public void Publish_ValueTypeDefault_Allowed()
        {
            _bus.Subscribe<TestEvent>(OnDefaultEvent);
            _bus.Publish(default(TestEvent));

            Assert.AreEqual(0, _defaultEvent.Value);
            Assert.IsNull(_defaultEvent.Label);
        }

        /// <summary>
        /// 某个 handler 抛出异常不应阻止其他 handler 的执行
        /// </summary>
        [Test]
        public void Publish_HandlerThrows_ContinuesToOtherHandlers()
        {
            _bus.Subscribe<TestEvent>(OnHandler1);
            _bus.Subscribe<TestEvent>(OnHandler2);
            _bus.Subscribe<TestEvent>(OnHandler3);

            Assert.DoesNotThrow(() => _bus.Publish(new TestEvent(1)));

            Assert.AreEqual(1, _handler1Count);
            Assert.AreEqual(1, _handler3Count);
        }

        /// <summary>
        /// 取消订阅从未订阅过的 handler 不应抛出异常
        /// </summary>
        [Test]
        public void Unsubscribe_NotSubscribed_NoError()
        {
            Assert.DoesNotThrow(() => _bus.Unsubscribe<TestEvent>(OnTestEvent));
        }

        /// <summary>
        /// 订阅后 HasSubscribers 应返回 true
        /// </summary>
        [Test]
        public void HasSubscribers_WhenSubscribed_ReturnsTrue()
        {
            _bus.Subscribe<TestEvent>(OnTestEvent);

            Assert.IsTrue(_bus.HasSubscribers<TestEvent>());
        }

        /// <summary>
        /// 取消订阅最后一个 handler 后 HasSubscribers 应返回 false
        /// </summary>
        [Test]
        public void HasSubscribers_WhenUnsubscribed_ReturnsFalse()
        {
            _bus.Subscribe<TestEvent>(OnTestEvent);
            _bus.Unsubscribe<TestEvent>(OnTestEvent);

            Assert.IsFalse(_bus.HasSubscribers<TestEvent>());
        }

        /// <summary>
        /// 从后台线程发布事件，handler 应能正确接收数据
        /// </summary>
        [Test]
        public void ThreadSafe_PublishFromBackgroundTask()
        {
            _bus.Subscribe<TestEvent>(OnBackgroundEvent);

            var task = System.Threading.Tasks.Task.Run(() => _bus.Publish(new TestEvent(99)));
            task.Wait();

            Assert.AreEqual(99, _bgCount);
        }

        /// <summary>
        /// 后台线程订阅与主线程发布并发执行，不应抛出异常
        /// </summary>
        [Test]
        public void Publish_ConcurrentSubscribe_NoException()
        {
            _bus.Subscribe<TestEvent>(OnConcurrentEvent1);

            Assert.DoesNotThrow(() =>
            {
                var task = System.Threading.Tasks.Task.Run(() => _bus.Subscribe<TestEvent>(OnConcurrentEvent2));
                _bus.Publish(new TestEvent(1));
                task.Wait();
            });
        }
    }
}
