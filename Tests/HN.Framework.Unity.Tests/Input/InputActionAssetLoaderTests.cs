#nullable enable

using System;
using HN.Framework.Unity.Capability.Input;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Tests.Input
{
    /// <summary>
    /// InputActionAssetLoader 的 EditMode 单元测试。
    /// 测试从 JSON 字符串加载 .inputactions 资产及占位 Addressables 方法的行为。
    /// </summary>
    [TestFixture]
    public class InputActionAssetLoaderTests
    {
        private InputActionAsset? _asset;

        [TearDown]
        public void TearDown()
        {
            if (_asset != null)
            {
                InputActionAssetLoader.UnloadAsset(_asset);
                _asset = null;
            }
        }

        #region LoadFromJson

        /// <summary>
        /// 传入有效的 .inputactions JSON，应返回非空的 InputActionAsset。
        /// </summary>
        [Test]
        public void LoadFromJson_ValidJson_ReturnsAsset()
        {
            var json = CreateMinimalValidJson();
            _asset = InputActionAssetLoader.LoadFromJson(json);
            Assert.That(_asset, Is.Not.Null);
        }

        /// <summary>
        /// 传入格式错误的 JSON，应抛出 ArgumentException。
        /// </summary>
        [Test]
        public void LoadFromJson_InvalidJson_ThrowsException()
        {
            const string invalidJson = "not valid json content";
            Assert.That(
                () => InputActionAssetLoader.LoadFromJson(invalidJson),
                Throws.ArgumentException);
        }

        /// <summary>
        /// 传入空地图的 JSON，应返回一个包含 0 个 ActionMap 的资产。
        /// </summary>
        [Test]
        public void LoadFromJson_EmptyMaps_ReturnsAssetWithNoMaps()
        {
            var json = CreateEmptyMapsJson();
            _asset = InputActionAssetLoader.LoadFromJson(json);
            Assert.That(_asset, Is.Not.Null);
            Assert.That(_asset.actionMaps, Is.Empty);
        }

        /// <summary>
        /// 传入 null JSON，应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void LoadFromJson_NullJson_ThrowsArgumentNullException()
        {
            Assert.That(
                () => InputActionAssetLoader.LoadFromJson(null!),
                Throws.ArgumentNullException);
        }

        /// <summary>
        /// 传入空字符串 JSON，应抛出 ArgumentException。
        /// </summary>
        [Test]
        public void LoadFromJson_EmptyJson_ThrowsArgumentException()
        {
            Assert.That(
                () => InputActionAssetLoader.LoadFromJson(string.Empty),
                Throws.ArgumentException);
        }

        /// <summary>
        /// 传入空白字符串 JSON，应抛出 ArgumentException。
        /// </summary>
        [Test]
        public void LoadFromJson_WhitespaceJson_ThrowsArgumentException()
        {
            Assert.That(
                () => InputActionAssetLoader.LoadFromJson("   "),
                Throws.ArgumentException);
        }

        #endregion

        #region LoadFromAsset

        /// <summary>
        /// 占位方法 LoadFromAsset 应输出警告而不抛出异常。
        /// </summary>
        [Test]
        public void LoadFromAsset_NotImplemented_DoesNotThrow()
        {
            Assert.That(
                () => InputActionAssetLoader.LoadFromAsset("test-address"),
                Throws.Nothing);
        }

        /// <summary>
        /// 占位方法 LoadFromAsset 当前应返回 null。
        /// </summary>
        [Test]
        public void LoadFromAsset_NotImplemented_ReturnsNull()
        {
            var result = InputActionAssetLoader.LoadFromAsset("test-address");
            Assert.That(result, Is.Null);
        }

        #endregion

        #region UnloadAsset

        /// <summary>
        /// 传入有效的 InputActionAsset 调用 UnloadAsset，应正常释放而不抛出异常。
        /// </summary>
        [Test]
        public void UnloadAsset_ValidAsset_DoesNotThrow()
        {
            var json = CreateMinimalValidJson();
            var asset = InputActionAssetLoader.LoadFromJson(json);
            Assert.That(asset, Is.Not.Null);
            Assert.That(() => InputActionAssetLoader.UnloadAsset(asset), Throws.Nothing);
        }

        /// <summary>
        /// 传入 null 调用 UnloadAsset，应为安全空操作而不抛出异常。
        /// </summary>
        [Test]
        public void UnloadAsset_NullAsset_DoesNotThrow()
        {
            Assert.That(() => InputActionAssetLoader.UnloadAsset(null), Throws.Nothing);
        }

        #endregion

        #region Helpers

        private static string CreateMinimalValidJson()
        {
            return @"{
                ""maps"": [{
                    ""name"": ""TestMap"",
                    ""id"": ""a1000000-0000-0000-0000-000000000001"",
                    ""actions"": [
                        {
                            ""name"": ""TestAction"",
                            ""type"": ""button"",
                            ""id"": ""a1000000-0000-0000-0000-000000000010"",
                            ""expectedControlType"": ""Button""
                        }
                    ],
                    ""bindings"": [
                        {
                            ""name"": """",
                            ""id"": ""a1000000-0000-0000-0000-000000000020"",
                            ""path"": ""<Keyboard>/space"",
                            ""action"": ""TestAction""
                        }
                    ]
                }],
                ""controlSchemes"": []
            }";
        }

        private static string CreateEmptyMapsJson()
        {
            return @"{
                ""maps"": [],
                ""controlSchemes"": []
            }";
        }

        #endregion
    }
}
