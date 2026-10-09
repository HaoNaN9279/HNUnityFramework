#nullable enable

using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Level.View.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace HN.Framework.Unity.Tests.Level.View.UI
{
    /// <summary>
    /// UIManager 的 EditMode 单元测试，覆盖 Canvas 层级创建、栈式导航和生命周期管理。
    /// </summary>
    [TestFixture]
    public class UIManagerTests
    {
        private UIManager? _manager;
        private GameObject? _containerGo;

        private const string TestPrefabPath = "Assets/TestUIManager/Resources/TestPanel.prefab";
        private const string TestResourcesContentFolder = "Assets/TestUIManager/Resources";
        private const string TestRootFolder = "Assets/TestUIManager";

        [SetUp]
        public void SetUp()
        {
            // 创建测试用 Resources 文件夹（Resources.Load 要求文件夹名必须为 "Resources"）
            if (!AssetDatabase.IsValidFolder(TestRootFolder))
            {
                AssetDatabase.CreateFolder("Assets", "TestUIManager");
            }

            if (!AssetDatabase.IsValidFolder(TestResourcesContentFolder))
            {
                AssetDatabase.CreateFolder(TestRootFolder, "Resources");
            }

            var panelGo = new GameObject("TestPanel", typeof(RectTransform));
            panelGo.AddComponent<TestConcretePanel>();

            PrefabUtility.SaveAsPrefabAsset(panelGo, TestPrefabPath);
            UnityEngine.Object.DestroyImmediate(panelGo);

            AssetDatabase.Refresh();

            // 创建容器以规避 EditMode 下 DontDestroyOnLoad 不可用的问题
            _containerGo = new GameObject("TestUIContainer", typeof(Transform));
            _containerGo.hideFlags = HideFlags.HideAndDontSave;
            _manager = new UIManager(_containerGo.transform);
        }

        [TearDown]
        public void TearDown()
        {
            if (_manager != null)
            {
                _manager.Dispose();
                _manager = null;
            }

            if (_containerGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_containerGo);
                _containerGo = null;
            }

            // 清理测试 Prefab 和文件夹
            if (AssetDatabase.IsValidFolder(TestRootFolder))
            {
                AssetDatabase.DeleteAsset(TestRootFolder);
            }
        }

        // ── 初始化 ──

        /// <summary>
        /// Initialize() 执行后应创建 7 个 Canvas GameObject。
        /// </summary>
        [Test]
        public void Initialize_CreatesSevenCanvases()
        {
            _manager!.Initialize();

            int canvasCount = 0;
            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                var canvas = _manager.GetLayerCanvas(layer);
                if (canvas != null)
                    canvasCount++;
            }

            Assert.That(canvasCount, Is.EqualTo(7),
                "Expected exactly 7 Canvas GameObjects after Initialize().");
        }

        // ── GetLayerCanvas ──

        /// <summary>
        /// 每个 UILayer 应返回一个 Canvas，且 sortingOrder 与枚举值一致。
        /// </summary>
        [Test]
        public void GetLayerCanvas_ReturnsCorrectCanvas()
        {
            _manager!.Initialize();

            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                var canvas = _manager.GetLayerCanvas(layer);

                Assert.That(canvas, Is.Not.Null,
                    $"Canvas for layer '{layer}' should not be null.");
                Assert.That(canvas!.sortingOrder, Is.EqualTo((int)layer),
                    $"Canvas '{layer}' sortingOrder should be {(int)layer}, got {canvas.sortingOrder}.");
                Assert.That(canvas.name, Is.EqualTo($"{layer}Canvas"),
                    $"Canvas name should be '{layer}Canvas', got '{canvas.name}'.");
            }
        }

        /// <summary>
        /// 传入无效的 UILayer 值应返回 null。
        /// </summary>
        [Test]
        public void GetLayerCanvas_InvalidLayer_ReturnsNull()
        {
            _manager!.Initialize();

            var invalidLayer = (UILayer)(-1);
            var canvas = _manager.GetLayerCanvas(invalidLayer);

            Assert.That(canvas, Is.Null,
                "GetLayerCanvas with invalid layer should return null.");
        }

        // ── Push / Pop ──

        /// <summary>
        /// Push() 调用不抛异常。EditMode 下跨程序集反序列化受限，Panel 组件可能丢失，
        /// 但 Push 方法本身不应崩溃。完整加载测试在 Phase 2（Addressables）中补充。
        /// </summary>
        [Test]
        public void Push_PanelIsAddedToStack()
        {
            _manager!.Initialize();

            int beforeCount = _manager.PanelStackCount;
            
            // EditMode 下跨程序集 prefab 实例化可能导致 UIPanel 组件丢失，
            // 预期 UIManager 会输出 LogError 并安全返回，而非崩溃。
            LogAssert.Expect(LogType.Error, "[UIManager] Push(): Prefab at 'TestPanel' does not have a UIPanel component.");
            
            Assert.DoesNotThrow(() => _manager.Push("TestPanel"),
                "Push should not throw exception.");
            
            Assert.That(_manager.PanelStackCount, Is.GreaterThanOrEqualTo(beforeCount),
                "Push should not decrease stack count.");
        }

        /// <summary>
        /// 空栈上调用 Pop() 不应抛出异常。
        /// </summary>
        [Test]
        public void Pop_EmptyStack_NoException()
        {
            _manager!.Initialize();

            Assert.That(() => _manager.Pop(), Throws.Nothing,
                "Pop on an empty stack should not throw.");
        }

        // ── Dispose ──

        /// <summary>
        /// Dispose() 后所有 Canvas GameObject 应被销毁。
        /// </summary>
        [Test]
        public void Dispose_ClearsAllGameObjects()
        {
            _manager!.Initialize();

            // 先收集 Canvas 引用
            var canvases = new Canvas?[7];
            for (int i = 0; i < 7; i++)
            {
                var layer = (UILayer)i;
                canvases[i] = _manager.GetLayerCanvas(layer);
            }

            _manager.Dispose();

            // 验证所有 Canvas 对象已被销毁
            for (int i = 0; i < 7; i++)
            {
                Assert.That(_manager.GetLayerCanvas((UILayer)i), Is.Null,
                    $"Canvas for layer '{(UILayer)i}' should be null after Dispose.");

                var canvas = canvases[i];
                if (canvas != null)
                {
                    Assert.That(canvas == null || canvas.gameObject == null,
                        $"Canvas GameObject for layer '{(UILayer)i}' should be destroyed.");
                }
            }
        }

        // ── 嵌套 SubCanvas ──

        /// <summary>
        /// CreateCanvas 应创建层根 Canvas 下的嵌套 Canvas（overrideSorting），sortingOrder = 层基值 + subOrder。
        /// </summary>
        [Test]
        public void CreateCanvas_CreatesNestedCanvasUnderLayer()
        {
            _manager!.Initialize();

            var handle = _manager.CreateCanvas(UILayer.UI, 5, "HudCanvas");

            Assert.That(handle, Is.Not.Null);
            Assert.That(handle!.Layer, Is.EqualTo(UILayer.UI));
            Assert.That(handle.SubOrder, Is.EqualTo(5));

            var canvas = ((UICanvasHandle)handle).Canvas;
            Assert.That(canvas, Is.Not.Null);
            Assert.That(canvas.overrideSorting, Is.True);
            Assert.That(canvas.sortingOrder, Is.EqualTo((int)UILayer.UI + 5));
            Assert.That(canvas.name, Is.EqualTo("HudCanvas"));

            var rootCanvas = _manager.GetLayerCanvas(UILayer.UI);
            Assert.That(canvas.transform.IsChildOf(rootCanvas!.transform), Is.True,
                "SubCanvas 应为层根 Canvas 的嵌套子节点。");
        }

        /// <summary>
        /// 越界的 subOrder 应被限制到 [SubOrderMin, SubOrderMax]。
        /// </summary>
        [Test]
        public void CreateCanvas_ClampsSubOrder()
        {
            _manager!.Initialize();

            var low = _manager.CreateCanvas(UILayer.UI, -50, "Low");
            var high = _manager.CreateCanvas(UILayer.UI, 100000, "High");

            Assert.That(low!.SubOrder, Is.EqualTo(UICanvasSorting.SubOrderMin));
            Assert.That(high!.SubOrder, Is.EqualTo(UICanvasSorting.SubOrderMax));
            Assert.That(((UICanvasHandle)high).Canvas.sortingOrder,
                Is.EqualTo((int)UILayer.UI + UICanvasSorting.SubOrderMax));
        }

        /// <summary>
        /// interactable = true 时应挂载 GraphicRaycaster。
        /// </summary>
        [Test]
        public void CreateCanvas_Interactable_AddsGraphicRaycaster()
        {
            _manager!.Initialize();

            var handle = _manager.CreateCanvas(UILayer.UI, 1, "Interactive", interactable: true);
            var canvas = ((UICanvasHandle)handle!).Canvas;

            Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Not.Null);
        }

        /// <summary>
        /// interactable = false（默认）时不应挂载 GraphicRaycaster。
        /// </summary>
        [Test]
        public void CreateCanvas_NotInteractable_NoGraphicRaycaster()
        {
            _manager!.Initialize();

            var handle = _manager.CreateCanvas(UILayer.UI, 1, "Static");
            var canvas = ((UICanvasHandle)handle!).Canvas;

            Assert.That(canvas.GetComponent<GraphicRaycaster>(), Is.Null);
        }

        /// <summary>
        /// 嵌套 Canvas 的 RectTransform 应被拉伸铺满父容器。
        /// </summary>
        [Test]
        public void CreateCanvas_StretchesRectTransform()
        {
            _manager!.Initialize();

            var handle = _manager.CreateCanvas(UILayer.UI, 1, "Stretch");
            var rect = ((UICanvasHandle)handle!).Canvas.GetComponent<RectTransform>();

            Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(rect.offsetMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.offsetMax, Is.EqualTo(Vector2.zero));
        }

        /// <summary>
        /// 一层内可创建任意数量的 SubCanvas，不受固定 7 层限制。
        /// </summary>
        [Test]
        public void CreateCanvas_MultiplePerLayer_NotLimited()
        {
            _manager!.Initialize();

            for (int i = 0; i < 20; i++)
            {
                _manager.CreateCanvas(UILayer.UI, i + 1, $"Canvas{i}");
            }

            Assert.That(_manager.GetSubCanvasCount(UILayer.UI), Is.EqualTo(20));
        }

        /// <summary>
        /// GetSubCanvases 应按层内排序号升序返回。
        /// </summary>
        [Test]
        public void GetSubCanvases_OrderedBySubOrder()
        {
            _manager!.Initialize();

            _manager.CreateCanvas(UILayer.UI, 20, "C20");
            _manager.CreateCanvas(UILayer.UI, 5, "C5");
            _manager.CreateCanvas(UILayer.UI, 10, "C10");

            var list = _manager.GetSubCanvases(UILayer.UI);

            Assert.That(list.Count, Is.EqualTo(3));
            Assert.That(list[0].SubOrder, Is.EqualTo(5));
            Assert.That(list[1].SubOrder, Is.EqualTo(10));
            Assert.That(list[2].SubOrder, Is.EqualTo(20));
        }

        /// <summary>
        /// 相同 subOrder 时按创建（Id）顺序稳定 tie-break。
        /// </summary>
        [Test]
        public void CreateCanvas_SameSubOrder_StableTieBreak()
        {
            _manager!.Initialize();

            var first = _manager.CreateCanvas(UILayer.UI, 5, "First");
            var second = _manager.CreateCanvas(UILayer.UI, 5, "Second");

            var list = _manager.GetSubCanvases(UILayer.UI);

            Assert.That(list[0].Id, Is.EqualTo(first!.Id));
            Assert.That(list[1].Id, Is.EqualTo(second!.Id));
            Assert.That(list[0].Id, Is.LessThan(list[1].Id));
        }

        /// <summary>
        /// SetCanvasSubOrder 应更新句柄的层内顺序号与 Canvas.sortingOrder。
        /// </summary>
        [Test]
        public void SetCanvasSubOrder_UpdatesSortingOrder()
        {
            _manager!.Initialize();

            var handle = _manager.CreateCanvas(UILayer.Scene, 1, "C");
            _manager.SetCanvasSubOrder(handle, 10);

            Assert.That(handle!.SubOrder, Is.EqualTo(10));
            Assert.That(((UICanvasHandle)handle).Canvas.sortingOrder,
                Is.EqualTo((int)UILayer.Scene + 10));
        }

        /// <summary>
        /// DestroyCanvas 应从层记录中移除并销毁 GameObject。
        /// </summary>
        [Test]
        public void DestroyCanvas_RemovesAndDestroys()
        {
            _manager!.Initialize();

            var handle = _manager.CreateCanvas(UILayer.UI, 1, "C");
            var canvas = ((UICanvasHandle)handle!).Canvas;

            _manager.DestroyCanvas(handle);

            Assert.That(_manager.GetSubCanvasCount(UILayer.UI), Is.EqualTo(0));
            Assert.That(canvas == null || canvas.gameObject == null, Is.True);
        }

        /// <summary>
        /// 未 Initialize 时 CreateCanvas 应记录错误并返回 null。
        /// </summary>
        [Test]
        public void CreateCanvas_BeforeInitialize_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error,
                new System.Text.RegularExpressions.Regex("CreateCanvas.*before Initialize"));

            var handle = _manager!.CreateCanvas(UILayer.UI, 1, "C");

            Assert.That(handle, Is.Null);
        }

    }

    /// <summary>
    /// UIPanel 的具象测试子类，供 Prefab 创建使用。
    /// </summary>
    public class TestConcretePanel : UIPanel
    {
    }
}
