#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Core.Tests.Capability.Network
{
    /// <summary>
    /// SyncCollection 数据结构单元测试。验证 SyncCollectionChange 和 SyncDictChange 结构体的字段正确性。
    /// </summary>
    [TestFixture]
    public class SyncCollectionTests
    {
        #region SyncCollectionOperation

        [Test]
        public void SyncCollectionOperation_Add_IsDefined()
        {
            Assert.That((byte)SyncCollectionOperation.Add, Is.EqualTo(0));
        }

        [Test]
        public void SyncCollectionOperation_Remove_IsDefined()
        {
            Assert.That((byte)SyncCollectionOperation.Remove, Is.EqualTo(1));
        }

        [Test]
        public void SyncCollectionOperation_Insert_IsDefined()
        {
            Assert.That((byte)SyncCollectionOperation.Insert, Is.EqualTo(2));
        }

        [Test]
        public void SyncCollectionOperation_Set_IsDefined()
        {
            Assert.That((byte)SyncCollectionOperation.Set, Is.EqualTo(3));
        }

        [Test]
        public void SyncCollectionOperation_Clear_IsDefined()
        {
            Assert.That((byte)SyncCollectionOperation.Clear, Is.EqualTo(4));
        }

        #endregion

        #region SyncCollectionChange<T>

        [Test]
        public void SyncCollectionChange_AddOperation_HasCorrectFields()
        {
            var change = new SyncCollectionChange<int>(SyncCollectionOperation.Add, 0, 42);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(change.Index, Is.EqualTo(0));
            Assert.That(change.Item, Is.EqualTo(42));
            Assert.That(change.OldItem, Is.EqualTo(0));
        }

        [Test]
        public void SyncCollectionChange_RemoveOperation_HasCorrectFields()
        {
            var change = new SyncCollectionChange<string>(SyncCollectionOperation.Remove, 2, "removed");

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Remove));
            Assert.That(change.Index, Is.EqualTo(2));
            Assert.That(change.Item, Is.EqualTo("removed"));
        }

        [Test]
        public void SyncCollectionChange_InsertOperation_HasCorrectFields()
        {
            var change = new SyncCollectionChange<float>(SyncCollectionOperation.Insert, 1, 3.14f);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Insert));
            Assert.That(change.Index, Is.EqualTo(1));
            Assert.That(change.Item, Is.EqualTo(3.14f).Within(0.001f));
        }

        [Test]
        public void SyncCollectionChange_SetOperation_HasOldItem()
        {
            var change = new SyncCollectionChange<int>(SyncCollectionOperation.Set, 3, 100, 50);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Set));
            Assert.That(change.Index, Is.EqualTo(3));
            Assert.That(change.Item, Is.EqualTo(100));
            Assert.That(change.OldItem, Is.EqualTo(50));
        }

        [Test]
        public void SyncCollectionChange_ClearOperation_IndexIsNegative()
        {
            var change = new SyncCollectionChange<int>(SyncCollectionOperation.Clear, -1, 0);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Clear));
            Assert.That(change.Index, Is.EqualTo(-1));
            Assert.That(change.Item, Is.EqualTo(0));
        }

        [Test]
        public void SyncCollectionChange_IsReadOnlyStruct()
        {
            var change = new SyncCollectionChange<int>(SyncCollectionOperation.Add, 0, 1);

            // 验证无法修改（只读结构体属性无 setter）
            var op = change.Operation;
            var idx = change.Index;
            var item = change.Item;
            var oldItem = change.OldItem;

            Assert.That(op, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(idx, Is.EqualTo(0));
            Assert.That(item, Is.EqualTo(1));
            Assert.That(oldItem, Is.EqualTo(0));
        }

        #endregion

        #region SyncDictChange<TKey, TValue>

        [Test]
        public void SyncDictChange_AddOperation_HasCorrectFields()
        {
            var change = new SyncDictChange<string, int>(SyncCollectionOperation.Add, "hp", 100);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(change.Key, Is.EqualTo("hp"));
            Assert.That(change.Value, Is.EqualTo(100));
        }

        [Test]
        public void SyncDictChange_SetOperation_HasCorrectFields()
        {
            var change = new SyncDictChange<int, string>(SyncCollectionOperation.Set, 1, "updated");

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Set));
            Assert.That(change.Key, Is.EqualTo(1));
            Assert.That(change.Value, Is.EqualTo("updated"));
        }

        [Test]
        public void SyncDictChange_RemoveOperation_HasCorrectFields()
        {
            var change = new SyncDictChange<int, float>(SyncCollectionOperation.Remove, 5, 3.14f);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Remove));
            Assert.That(change.Key, Is.EqualTo(5));
            Assert.That(change.Value, Is.EqualTo(3.14f).Within(0.001f));
        }

        [Test]
        public void SyncDictChange_ClearOperation_HasCorrectFields()
        {
            var change = new SyncDictChange<int, string>(SyncCollectionOperation.Clear, 0, string.Empty);

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Clear));
            Assert.That(change.Key, Is.EqualTo(0));
            Assert.That(change.Value, Is.EqualTo(string.Empty));
        }

        [Test]
        public void SyncDictChange_IsReadOnlyStruct()
        {
            var change = new SyncDictChange<string, int>(SyncCollectionOperation.Add, "key", 42);

            var op = change.Operation;
            var key = change.Key;
            var value = change.Value;

            Assert.That(op, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(key, Is.EqualTo("key"));
            Assert.That(value, Is.EqualTo(42));
        }

        #endregion

        #region Edge Cases

        [Test]
        public void SyncCollectionChange_DefaultValue()
        {
            SyncCollectionChange<int> change = default;

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(change.Index, Is.EqualTo(0));
            Assert.That(change.Item, Is.EqualTo(0));
            Assert.That(change.OldItem, Is.EqualTo(0));
        }

        [Test]
        public void SyncDictChange_DefaultValue()
        {
            SyncDictChange<string, string> change = default;

            Assert.That(change.Operation, Is.EqualTo(SyncCollectionOperation.Add));
            Assert.That(change.Key, Is.EqualTo(null));
            Assert.That(change.Value, Is.EqualTo(null));
        }

        #endregion
    }
}
