#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.UI;
using HN.Framework.Core.Driver.Common;
using UnityEngine;

namespace HN.Framework.Unity.Capability.UI
{
    /// <summary>
    /// UI 管理器，基于 UGUI 实现 IUIManager 接口。
    /// 管理 7 层 Canvas 渲染栈、栈式导航（Push/Pop）和模态面板（Show/Hide）。
    /// UIManager 是纯 C# 类（非 MonoBehaviour），由 GameWorldDriver 在 Awake 中创建并管理生命周期。
    /// Phase 1 实现基础生命周期，Phase 2 将添加动画管理。
    /// </summary>
    public sealed class UIManager : IUIManager, ITickable, IDisposable
    {
        private const int LayerCount = 7;

        /// <summary>
        /// 将 UILayer 枚举值映射为连续的数组索引（0-6），因为 UILayer 的值是 SortOrder 而非索引。
        /// </summary>
        private static int LayerToIndex(UILayer layer) => layer switch
        {
            UILayer.Background => 0,
            UILayer.Scene => 1,
            UILayer.UI => 2,
            UILayer.Popup => 3,
            UILayer.Toast => 4,
            UILayer.Guide => 5,
            UILayer.System => 6,
            _ => -1
        };

        private readonly Transform? _container;
        private bool _initialized;
        private readonly Canvas?[] _layerCanvases;
        private readonly Transform?[] _layerRoots;
        private readonly Stack<UIPanel> _panelStack;
        private readonly Dictionary<string, UIPanel> _nonStackPanels;

        /// <summary>
        /// 创建 UIManager 实例，Canvas 对象将标记为 DontDestroyOnLoad。
        /// </summary>
        public UIManager()
        {
            _layerCanvases = new Canvas?[LayerCount];
            _layerRoots = new Transform?[LayerCount];
            _panelStack = new Stack<UIPanel>();
            _nonStackPanels = new Dictionary<string, UIPanel>();
        }

        /// <summary>
        /// 创建 UIManager 实例，Canvas 对象将挂载到指定容器下。
        /// </summary>
        /// <param name="container">Canvas 的父容器 Transform。</param>
        public UIManager(Transform container) : this()
        {
            _container = container;
        }

        /// <summary>
        /// 辅助方法：在 Play Mode 使用 Destroy（正常生命周期），Edit Mode 使用 DestroyImmediate。
        /// </summary>
        private static void DestroyObject(UnityEngine.Object obj)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// 获取导航栈中当前面板的数量（仅供测试使用）。
        /// </summary>
        internal int PanelStackCount => _panelStack.Count;

        // ── 初始化 ──

        /// <summary>
        /// 初始化 UI 管理器，为每个 UILayer 创建对应的 Canvas GameObject。
        /// 重复调用无害（幂等）。
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                int index = LayerToIndex(layer);

                // 防御：确保 UILayer 值在合法范围内
                if (index < 0 || index >= LayerCount)
                    continue;

                CreateLayerCanvas(layer, index);
            }
        }

        /// <summary>
        /// 为指定层创建 Canvas 及其 Root 子对象。
        /// </summary>
        private void CreateLayerCanvas(UILayer layer, int index)
        {
            string layerName = layer.ToString();

            // 创建 Canvas GameObject
            var canvasGo = new GameObject($"{layerName}Canvas", typeof(RectTransform), typeof(Canvas));
            if (_container != null)
            {
                canvasGo.transform.SetParent(_container, false);
            }
            else
            {
                UnityEngine.Object.DontDestroyOnLoad(canvasGo);
            }

            // 配置 Canvas
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = (int)layer;
            _layerCanvases[index] = canvas;

            // 创建 Root 子对象
            var rootGo = new GameObject($"{layerName}Root", typeof(RectTransform));
            rootGo.transform.SetParent(canvasGo.transform, false);
            _layerRoots[index] = rootGo.transform;
        }

        // ── IUIManager 实现 ──

        /// <inheritdoc />
        public void Push(string panelPath)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError($"[UIManager] Push('{panelPath}') called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(panelPath))
            {
                UnityEngine.Debug.LogError("[UIManager] Push(): panelPath is null or empty.");
                return;
            }

            // Phase 1: 从 Resources 加载
            var prefab = Resources.Load<GameObject>(panelPath);
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Push(): Failed to load panel from Resources path '{panelPath}'.");
                return;
            }

            var instance = UnityEngine.Object.Instantiate(prefab);
            var panel = instance.GetComponent<UIPanel>();
            if (panel == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Push(): Prefab at '{panelPath}' does not have a UIPanel component.");
                DestroyObject(instance);
                return;
            }

            // Phase 1: 默认层为 UILayer.UI，Phase 2 将通过 PanelAttribute 指定
            UILayer layer = UILayer.UI;
            int index = LayerToIndex(layer);
            Transform? layerRoot = _layerRoots[index];

            if (layerRoot == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Push(): Layer root for '{layer}' is null. Did Initialize() run?");
                DestroyObject(instance);
                return;
            }

            panel.Open(layerRoot);
            _panelStack.Push(panel);
        }

        /// <inheritdoc />
        public void Pop()
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogWarning("[UIManager] Pop() called before Initialize().");
                return;
            }

            if (_panelStack.Count == 0)
                return;

            var panel = _panelStack.Pop();
            if (panel != null)
            {
                panel.Close();
            }
        }

        /// <inheritdoc />
        public void Show(string panelPath)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError($"[UIManager] Show('{panelPath}') called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(panelPath))
            {
                UnityEngine.Debug.LogError("[UIManager] Show(): panelPath is null or empty.");
                return;
            }

            // 已显示的同路径面板不重复创建
            if (_nonStackPanels.ContainsKey(panelPath))
            {
                UnityEngine.Debug.LogWarning($"[UIManager] Show(): Panel '{panelPath}' is already shown.");
                return;
            }

            var prefab = Resources.Load<GameObject>(panelPath);
            if (prefab == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Show(): Failed to load panel from Resources path '{panelPath}'.");
                return;
            }

            var instance = UnityEngine.Object.Instantiate(prefab);
            var panel = instance.GetComponent<UIPanel>();
            if (panel == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Show(): Prefab at '{panelPath}' does not have a UIPanel component.");
                DestroyObject(instance);
                return;
            }

            UILayer layer = UILayer.UI;
            int index = LayerToIndex(layer);
            Transform? layerRoot = _layerRoots[index];

            if (layerRoot == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Show(): Layer root for '{layer}' is null.");
                DestroyObject(instance);
                return;
            }

            panel.Open(layerRoot);
            _nonStackPanels[panelPath] = panel;
        }

        /// <inheritdoc />
        public void Hide(string panelPath)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogWarning("[UIManager] Hide() called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(panelPath))
            {
                UnityEngine.Debug.LogError("[UIManager] Hide(): panelPath is null or empty.");
                return;
            }

            if (_nonStackPanels.TryGetValue(panelPath, out var panel))
            {
                panel.Close();
                _nonStackPanels.Remove(panelPath);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[UIManager] Hide(): Panel '{panelPath}' is not currently shown.");
            }
        }

        // ── 层级 Canvas 访问 ──

        /// <summary>
        /// 获取指定 UI 层的 Canvas 组件。
        /// 注意：此方法返回 UnityEngine.Canvas 类型，因此不在 IUIManager 接口中
        /// （Core 程序集开启了 noEngineReferences）。
        /// </summary>
        /// <param name="layer">UI 层级。</param>
        /// <returns>对应的 Canvas 组件；若层无效则返回 null。</returns>
        public Canvas? GetLayerCanvas(UILayer layer)
        {
            int index = LayerToIndex(layer);
            if (index < 0 || index >= LayerCount)
                return null;

            return _layerCanvases[index];
        }

        /// <summary>
        /// 获取指定 UI 层的 Root Transform，UIPanel 子类可在此下添加子 UI。
        /// </summary>
        /// <param name="layer">UI 层级。</param>
        /// <returns>层的根 Transform；若层无效则返回 null。</returns>
        internal Transform? GetLayerRoot(UILayer layer)
        {
            int index = LayerToIndex(layer);
            if (index < 0 || index >= LayerCount)
                return null;

            return _layerRoots[index];
        }

        // ── ITickable ──

        /// <summary>
        /// Phase 1：空实现，预留给 Phase 2 的运行时动画管理。
        /// </summary>
        public void Tick() { }

        /// <summary>
        /// Phase 1：空实现，预留给 Phase 2 的运行时动画管理。
        /// </summary>
        public void LateTick() { }

        // ── IDisposable ──

        /// <summary>
        /// 释放 UI 管理器创建的所有 Canvas GameObject 并清空内部状态。
        /// 可多次安全调用。
        /// </summary>
        public void Dispose()
        {
            // 关闭导航栈中所有面板
            while (_panelStack.Count > 0)
            {
                var panel = _panelStack.Pop();
                if (panel != null)
                {
                    panel.Close();
                }
            }

            // 关闭所有非栈面板
            foreach (var kvp in _nonStackPanels)
            {
                kvp.Value?.Close();
            }
            _nonStackPanels.Clear();

            // 销毁所有 Canvas GameObject
            for (int i = 0; i < LayerCount; i++)
            {
                var canvas = _layerCanvases[i];
                if (canvas != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(canvas.gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(canvas.gameObject);

                    _layerCanvases[i] = null;
                    _layerRoots[i] = null;
                }
            }

            _initialized = false;
        }
    }
}
