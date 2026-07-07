#nullable enable

using NUnit.Framework;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Tests.Input
{
    [TestFixture]
    public class CoreInputModelsTests
    {
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

        // ============ Enum Tests ============

        /// <summary>
        /// InputActionType 枚举应包含 Button、Value、Vector2 三个值
        /// </summary>
        [Test]
        public void InputActionType_HasExpectedMembers()
        {
            var names = System.Enum.GetNames(typeof(InputActionType));
            Assert.AreEqual(3, names.Length);
            Assert.Contains(nameof(InputActionType.Button), names);
            Assert.Contains(nameof(InputActionType.Value), names);
            Assert.Contains(nameof(InputActionType.Vector2), names);
        }

        /// <summary>
        /// InputPhase 枚举应包含 Started、Performed、Canceled 三个值
        /// </summary>
        [Test]
        public void InputPhase_HasExpectedMembers()
        {
            var names = System.Enum.GetNames(typeof(InputPhase));
            Assert.AreEqual(3, names.Length);
            Assert.Contains(nameof(InputPhase.Started), names);
            Assert.Contains(nameof(InputPhase.Performed), names);
            Assert.Contains(nameof(InputPhase.Canceled), names);
        }

        // ============ InputAction (IReference) Tests ============

        /// <summary>
        /// 通过 ReferencePool 获取的 InputAction 初始状态为空
        /// </summary>
        [Test]
        public void InputAction_Acquire_InitialStateIsDefault()
        {
            var action = ReferencePool.Acquire<InputAction>();

            Assert.IsNull(action.ActionName);
            Assert.AreEqual(default(InputActionType), action.ActionType);
            Assert.IsNull(action.DefaultBinding);
        }

        /// <summary>
        /// 设置属性后值可正确读取
        /// </summary>
        [Test]
        public void InputAction_SetProperties_ValuesAreStored()
        {
            var action = ReferencePool.Acquire<InputAction>();
            action.ActionName = "Jump";
            action.ActionType = InputActionType.Button;
            action.DefaultBinding = "<Keyboard>/space";

            Assert.AreEqual("Jump", action.ActionName);
            Assert.AreEqual(InputActionType.Button, action.ActionType);
            Assert.AreEqual("<Keyboard>/space", action.DefaultBinding);
        }

        /// <summary>
        /// Clear 方法应将所有属性重置为默认值
        /// </summary>
        [Test]
        public void InputAction_Clear_NullsAllProperties()
        {
            var action = ReferencePool.Acquire<InputAction>();
            action.ActionName = "Jump";
            action.ActionType = InputActionType.Button;
            action.DefaultBinding = "<Keyboard>/space";

            action.Clear();

            Assert.IsNull(action.ActionName);
            Assert.AreEqual(default(InputActionType), action.ActionType);
            Assert.IsNull(action.DefaultBinding);
        }

        /// <summary>
        /// ReferencePool Acquire→Release→Acquire 周期后，复用的 InputAction 状态已被清除
        /// </summary>
        [Test]
        public void InputAction_ReferencePoolCycle_ReturnsCleanState()
        {
            var action = ReferencePool.Acquire<InputAction>();
            action.ActionName = "Jump";
            action.ActionType = InputActionType.Button;
            action.DefaultBinding = "<Keyboard>/space";

            ReferencePool.Release(action);

            var recycled = ReferencePool.Acquire<InputAction>();
            Assert.IsNull(recycled.ActionName);
            Assert.AreEqual(default(InputActionType), recycled.ActionType);
            Assert.IsNull(recycled.DefaultBinding);
        }

        // ============ InputContext (readonly struct) Tests ============

        /// <summary>
        /// 默认构造的 InputContext 所有字段应为默认值
        /// </summary>
        [Test]
        public void InputContext_DefaultValues_AreDefault()
        {
            InputContext ctx = default;

            Assert.IsNull(ctx.ActionName);
            Assert.AreEqual(default(InputPhase), ctx.Phase);
            Assert.IsNull(ctx.Value);
        }

        /// <summary>
        /// 通过构造函数创建的 InputContext 应正确存储所有字段值
        /// </summary>
        [Test]
        public void InputContext_Constructor_SetsProperties()
        {
            var ctx = new InputContext("Jump", InputPhase.Performed, 1.0f);

            Assert.AreEqual("Jump", ctx.ActionName);
            Assert.AreEqual(InputPhase.Performed, ctx.Phase);
            Assert.AreEqual(1.0f, ctx.Value);
        }

        /// <summary>
        /// 相同字段值的两个 InputContext 应相等
        /// </summary>
        [Test]
        public void InputContext_Equals_SameValues_ReturnsTrue()
        {
            var ctx1 = new InputContext("Jump", InputPhase.Performed, 1.0f);
            var ctx2 = new InputContext("Jump", InputPhase.Performed, 1.0f);

            Assert.IsTrue(ctx1.Equals(ctx2));
            Assert.IsTrue(ctx1 == ctx2);
            Assert.AreEqual(ctx1.GetHashCode(), ctx2.GetHashCode());
        }

        /// <summary>
        /// 不同字段值的两个 InputContext 应不相等
        /// </summary>
        [Test]
        public void InputContext_Equals_DifferentValues_ReturnsFalse()
        {
            var ctx1 = new InputContext("Jump", InputPhase.Performed, 1.0f);
            var ctx2 = new InputContext("Move", InputPhase.Performed, 1.0f);

            Assert.IsFalse(ctx1.Equals(ctx2));
            Assert.IsTrue(ctx1 != ctx2);
        }

        /// <summary>
        /// Object.Equals 应正确处理非 InputContext 类型
        /// </summary>
        [Test]
        public void InputContext_Equals_NonInputContext_ReturnsFalse()
        {
            var ctx = new InputContext("Jump", InputPhase.Performed, 1.0f);

            Assert.IsFalse(ctx.Equals(null));
            Assert.IsFalse(ctx.Equals("string"));
        }

        /// <summary>
        /// ToString 应返回包含关键信息的格式化字符串
        /// </summary>
        [Test]
        public void InputContext_ToString_ReturnsExpectedFormat()
        {
            var ctx = new InputContext("Jump", InputPhase.Started, null);

            string result = ctx.ToString();

            Assert.That(result, Does.Contain("InputContext"), "应包含类型名");
            Assert.That(result, Does.Contain("Jump"), "应包含 ActionName");
            Assert.That(result, Does.Contain("Started"), "应包含 Phase");
        }
    }
}
