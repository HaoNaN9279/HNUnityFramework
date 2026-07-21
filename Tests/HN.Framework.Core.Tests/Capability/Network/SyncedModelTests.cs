#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Core.Tests.Capability.Network
{
    /// <summary>
    /// SyncedModel 的单元测试。验证读/写、变更通知、脏标记等核心行为。
    /// </summary>
    [TestFixture]
    public class SyncedModelTests
    {
        #region Value

        [Test]
        public void Value_InitialDefault_ReturnsDefault()
        {
            var model = new SyncedModel<int>();
            Assert.That(model.Value, Is.EqualTo(0));
        }

        [Test]
        public void Value_InitialValue_ReturnsInitialValue()
        {
            var model = new SyncedModel<int>(42);
            Assert.That(model.Value, Is.EqualTo(42));
        }

        #endregion

        #region SetValue

        [Test]
        public void SetValue_DifferentValue_UpdatesValue()
        {
            var model = new SyncedModel<int>();
            model.SetValue(10);
            Assert.That(model.Value, Is.EqualTo(10));
        }

        [Test]
        public void SetValue_SameValue_DoesNotTriggerEvent()
        {
            var model = new SyncedModel<int>(5);
            int eventCount = 0;
            model.OnValueChanged += (_) => eventCount++;

            model.SetValue(5);

            Assert.That(eventCount, Is.EqualTo(0));
        }

        [Test]
        public void SetValue_DifferentValue_TriggersEvent()
        {
            var model = new SyncedModel<int>();
            int receivedValue = 0;
            model.OnValueChanged += (v) => receivedValue = v;

            model.SetValue(99);

            Assert.That(receivedValue, Is.EqualTo(99));
        }

        [Test]
        public void SetValue_StringType_WorksCorrectly()
        {
            var model = new SyncedModel<string>("hello");
            string? received = null;
            model.OnValueChanged += (v) => received = v;

            model.SetValue("world");

            Assert.That(received, Is.EqualTo("world"));
            Assert.That(model.Value, Is.EqualTo("world"));
        }

        [Test]
        public void SetValue_FloatType_WorksCorrectly()
        {
            var model = new SyncedModel<float>(1.5f);
            model.SetValue(3.14f);
            Assert.That(model.Value, Is.EqualTo(3.14f).Within(0.001f));
        }

        #endregion

        #region Custom Struct

        private struct TestData
        {
            public int X;
            public int Y;

            public TestData(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [Test]
        public void SetValue_CustomStruct_UpdatesValue()
        {
            var model = new SyncedModel<TestData>(new TestData(1, 2));
            model.SetValue(new TestData(3, 4));

            Assert.That(model.Value.X, Is.EqualTo(3));
            Assert.That(model.Value.Y, Is.EqualTo(4));
        }

        [Test]
        public void SetValue_CustomStructSameValue_DoesNotTriggerEvent()
        {
            var model = new SyncedModel<TestData>(new TestData(1, 2));
            int eventCount = 0;
            model.OnValueChanged += (_) => eventCount++;

            model.SetValue(new TestData(1, 2));

            Assert.That(eventCount, Is.EqualTo(0));
        }

        #endregion

        #region IsDirty / ClearDirty

        [Test]
        public void IsDirty_AfterConstruction_ReturnsFalse()
        {
            var model = new SyncedModel<int>();
            Assert.That(model.IsDirty, Is.False);
        }

        [Test]
        public void IsDirty_AfterSetValue_ReturnsTrue()
        {
            var model = new SyncedModel<int>();
            model.SetValue(1);
            Assert.That(model.IsDirty, Is.True);
        }

        [Test]
        public void ClearDirty_AfterSetValue_ResetsDirty()
        {
            var model = new SyncedModel<int>();
            model.SetValue(1);
            model.ClearDirty();
            Assert.That(model.IsDirty, Is.False);
        }

        [Test]
        public void IsDirty_AfterSetSameValue_ReturnsFalse()
        {
            var model = new SyncedModel<int>(5);
            model.SetValue(5);
            Assert.That(model.IsDirty, Is.False);
        }

        [Test]
        public void IsDirty_SetAgainAfterClear_ReturnsTrue()
        {
            var model = new SyncedModel<int>();
            model.SetValue(1);
            model.ClearDirty();
            model.SetValue(2);
            Assert.That(model.IsDirty, Is.True);
        }

        #endregion

        #region Reset

        [Test]
        public void Reset_ResetsValueToDefault()
        {
            var model = new SyncedModel<int>(42);
            model.Reset();
            Assert.That(model.Value, Is.EqualTo(0));
        }

        [Test]
        public void Reset_ResetsDirtyFlag()
        {
            var model = new SyncedModel<int>();
            model.SetValue(1);
            model.Reset();
            Assert.That(model.IsDirty, Is.False);
        }

        [Test]
        public void Reset_TriggersOnValueChanged()
        {
            var model = new SyncedModel<int>(10);
            int receivedValue = 0;
            model.OnValueChanged += (v) => receivedValue = v;

            model.Reset(0);

            Assert.That(receivedValue, Is.EqualTo(0));
        }

        #endregion

        #region OnValueChanged

        [Test]
        public void OnValueChanged_MultipleSubscribers_AllNotified()
        {
            var model = new SyncedModel<int>();
            int count1 = 0, count2 = 0;
            model.OnValueChanged += (_) => count1++;
            model.OnValueChanged += (_) => count2++;

            model.SetValue(1);

            Assert.That(count1, Is.EqualTo(1));
            Assert.That(count2, Is.EqualTo(1));
        }

        [Test]
        public void OnValueChanged_Unsubscribe_StopsNotification()
        {
            var model = new SyncedModel<int>();
            int count = 0;
            System.Action<int> handler = (_) => count++;
            model.OnValueChanged += handler;
            model.SetValue(1);
            model.OnValueChanged -= handler;

            model.SetValue(2);

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void OnValueChanged_NoSubscribers_DoesNotThrow()
        {
            var model = new SyncedModel<int>();
            Assert.DoesNotThrow(() => model.SetValue(42));
        }

        #endregion
    }
}
