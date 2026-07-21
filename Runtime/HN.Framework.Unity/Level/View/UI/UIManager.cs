#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.UI;
using HN.Framework.Core.Driver.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// UI 管理器，基于 UGUI 实现 IUIManager 接口。
    /// 管理 7 层 Canvas 渲染栈、栈式导航（Push/Pop）和模态面板（Show/Hide）。
    /// UIManager 是纯 C# 类（非 MonoBehaviour），由 GameWorldDriver 在 Awake 中创建并管理生命周期。
    /// Phase 2 增加了 Addressables 异步加载、LitMotion 动画集成、Toast 队列管理和引导系统。
    /// </summary>
    public sealed class UIManager : IUIManager, ITickable, IDisposable
    {
        private const int LayerCount = 7;

        /// <summary>
        /// 缓存的 UI 全局配置。由模块初始化时设置，未设置时使用默认值。
        /// </summary>
        internal static UISettings? s_CachedSettings;

        /// <summary>
        /// 最大并发 Toast 数量。优先读取 <see cref="UISettings"/>，回退默认值 3。
        /// </summary>
        private static int MaxConcurrentToasts =>
            s_CachedSettings?.MaxConcurrentToasts ?? 3;

        /// <summary>
        /// Toast 默认显示时长。优先读取 <see cref="UISettings"/>，回退默认值 2 秒。
        /// </summary>
        private static float DefaultToastDuration =>
            s_CachedSettings?.DefaultToastDuration ?? 2f;

        /// <summary>
        /// 通用 UI 动画默认时长。优先读取 <see cref="UISettings"/>，回退默认值 0.3 秒。
        /// </summary>
        private static float DefaultAnimationDuration =>
            s_CachedSettings?.DefaultAnimationDuration ?? 0.3f;

        /// <summary>
        /// 模态遮罩透明度。优先读取 <see cref="UISettings"/>，回退默认值 0.5。
        /// </summary>
        internal static float DialogMaskAlpha =>
            s_CachedSettings?.DialogMaskAlpha ?? 0.5f;

        /// <summary>
        /// Toast 动画时长。优先读取 <see cref="UISettings"/>，回退默认值 0.2 秒。
        /// </summary>
        private static float ToastAnimationDuration =>
            s_CachedSettings?.ToastAnimationDuration ?? 0.2f;

        /// <summary>
        /// Toast 默认字号。优先读取 <see cref="UISettings"/>，回退默认值 24。
        /// </summary>
        private static int ToastFontSize =>
            s_CachedSettings?.ToastFontSize ?? 24;

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

        // ── Phase 2 新增字段 ──

        /// <summary>已显示的对话框面板（key 为 panelPath）。</summary>
        private readonly Dictionary<string, UIDialog> _currentDialogs = new();

        /// <summary>等待显示的 Toast 队列。</summary>
        private readonly Queue<ToastItem> _toastQueue = new();

        /// <summary>当前正在显示的 Toast 及其剩余时间。</summary>
        private readonly List<ActiveToast> _activeToastList = new();

        /// <summary>当前正在运行的新手引导面板。</summary>
        private UIGuide? _currentGuide;

        /// <summary>正在进行的异步加载操作。</summary>
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _pendingLoads = new();

        /// <summary>
        /// Toast 队列中的单条消息数据。
        /// </summary>
        private struct ToastItem
        {
            public string Message;
            public float Duration;
        }

        /// <summary>
        /// 正在显示的 Toast 追踪信息，用于 Tick 中的计时管理。
        /// </summary>
        private sealed class ActiveToast
        {
            public UIToast Toast = null!;
            public float RemainingTime;
        }

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
        [Obsolete("Use PushAsync for Addressables loading.")]
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

            // 从 Resources 加载
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

            UILayer layer = UILayer.UI;
            int index = LayerToIndex(layer);
            Transform? layerRoot = _layerRoots[index];

            if (layerRoot == null)
            {
                UnityEngine.Debug.LogError($"[UIManager] Push(): Layer root for '{layer}' is null. Did Initialize() run?");
                DestroyObject(instance);
                return;
            }

            // 暂停当前栈顶面板
            if (_panelStack.Count > 0)
            {
                var previous = _panelStack.Peek();
                previous.Pause();
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

            // 恢复新的栈顶面板
            if (_panelStack.Count > 0)
            {
                var resumed = _panelStack.Peek();
                resumed.Resume();
            }
        }

        /// <inheritdoc />
        [Obsolete("Use ShowAsync for Addressables loading.")]
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

        // ── Addressables 异步加载 ──

        /// <summary>
        /// 通过 Addressables 异步加载面板 Prefab，失败时回退到 Resources.Load。
        /// </summary>
        /// <param name="panelPath">面板资源路径（Addressables key 或 Resources 路径）。</param>
        /// <param name="onLoaded">加载完成回调，参数为加载的 GameObject 或 null。</param>
        private void LoadPanelPrefabAsync(string panelPath, Action<GameObject?> onLoaded)
        {
            if (string.IsNullOrEmpty(panelPath))
            {
                onLoaded?.Invoke(null);
                return;
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(panelPath);
                handle.Completed += (op) =>
                {
                    try
                    {
                        if (op.Status == AsyncOperationStatus.Succeeded)
                        {
                            onLoaded?.Invoke(op.Result);
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"[UIManager] Addressables failed to load '{panelPath}', falling back to Resources. Error: {op.OperationException?.Message}");
                            var prefab = Resources.Load<GameObject>(panelPath);
                            onLoaded?.Invoke(prefab);
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[UIManager] Error in Addressables callback for '{panelPath}': {ex.Message}");
                        var prefab = Resources.Load<GameObject>(panelPath);
                        onLoaded?.Invoke(prefab);
                    }
                    finally
                    {
                        _pendingLoads.Remove(panelPath);
                    }
                };
                _pendingLoads[panelPath] = handle;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[UIManager] Failed to start Addressables load for '{panelPath}': {ex.Message}");
                var prefab = Resources.Load<GameObject>(panelPath);
                onLoaded?.Invoke(prefab);
            }
        }

        // ── PushAsync / ShowAsync ──

        /// <inheritdoc />
        public void PushAsync(string panelPath)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError($"[UIManager] PushAsync('{panelPath}') called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(panelPath))
            {
                UnityEngine.Debug.LogError("[UIManager] PushAsync(): panelPath is null or empty.");
                return;
            }

            LoadPanelPrefabAsync(panelPath, (prefab) =>
            {
                if (prefab == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] PushAsync(): Failed to load panel '{panelPath}'.");
                    return;
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                var panel = instance.GetComponent<UIPanel>();
                if (panel == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] PushAsync(): Prefab at '{panelPath}' does not have a UIPanel component.");
                    DestroyObject(instance);
                    return;
                }

                int index = LayerToIndex(UILayer.UI);
                Transform? layerRoot = _layerRoots[index];

                if (layerRoot == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] PushAsync(): Layer root for UI layer is null.");
                    DestroyObject(instance);
                    return;
                }

                // 暂停当前栈顶面板
                if (_panelStack.Count > 0)
                {
                    var previous = _panelStack.Peek();
                    previous.Pause();
                }

                panel.Open(layerRoot);
                _panelStack.Push(panel);
            });
        }

        /// <inheritdoc />
        public void ShowAsync(string panelPath)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError($"[UIManager] ShowAsync('{panelPath}') called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(panelPath))
            {
                UnityEngine.Debug.LogError("[UIManager] ShowAsync(): panelPath is null or empty.");
                return;
            }

            // 已显示的同路径面板不重复创建
            if (_nonStackPanels.ContainsKey(panelPath))
            {
                UnityEngine.Debug.LogWarning($"[UIManager] ShowAsync(): Panel '{panelPath}' is already shown.");
                return;
            }

            LoadPanelPrefabAsync(panelPath, (prefab) =>
            {
                if (prefab == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] ShowAsync(): Failed to load panel '{panelPath}'.");
                    return;
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                var panel = instance.GetComponent<UIPanel>();
                if (panel == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] ShowAsync(): Prefab at '{panelPath}' does not have a UIPanel component.");
                    DestroyObject(instance);
                    return;
                }

                int index = LayerToIndex(UILayer.UI);
                Transform? layerRoot = _layerRoots[index];

                if (layerRoot == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] ShowAsync(): Layer root for UI layer is null.");
                    DestroyObject(instance);
                    return;
                }

                panel.Open(layerRoot);
                _nonStackPanels[panelPath] = panel;
            });
        }

        // ── ShowDialog ──

        /// <inheritdoc />
        public void ShowDialog(string panelPath, Action? onConfirm = null, Action? onCancel = null)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError($"[UIManager] ShowDialog('{panelPath}') called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(panelPath))
            {
                UnityEngine.Debug.LogError("[UIManager] ShowDialog(): panelPath is null or empty.");
                return;
            }

            // 同路径对话框已显示时不重复创建
            if (_currentDialogs.ContainsKey(panelPath))
            {
                UnityEngine.Debug.LogWarning($"[UIManager] ShowDialog(): Dialog '{panelPath}' is already shown.");
                return;
            }

            LoadPanelPrefabAsync(panelPath, (prefab) =>
            {
                if (prefab == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] ShowDialog(): Failed to load dialog '{panelPath}'.");
                    return;
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                var dialog = instance.GetComponent<UIDialog>();
                if (dialog == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] ShowDialog(): Prefab at '{panelPath}' does not have a UIDialog component.");
                    DestroyObject(instance);
                    return;
                }

                int index = LayerToIndex(UILayer.Popup);
                Transform? layerRoot = _layerRoots[index];

                if (layerRoot == null)
                {
                    UnityEngine.Debug.LogError("[UIManager] ShowDialog(): Popup layer root is null.");
                    DestroyObject(instance);
                    return;
                }

                dialog.Show(layerRoot, onConfirm, onCancel);

                // 订阅关闭事件以清理 _currentDialogs
                var capturedPath = panelPath;
                Action? closeHandler = null;
                closeHandler = () =>
                {
                    _currentDialogs.Remove(capturedPath);
                    dialog.OnConfirm -= closeHandler;
                    dialog.OnCancel -= closeHandler;
                };
                dialog.OnConfirm += closeHandler;
                dialog.OnCancel += closeHandler;

                _currentDialogs[panelPath] = dialog;
            });
        }

        // ── ShowToast ──

        /// <inheritdoc />
        public void ShowToast(string message, float duration = -1f)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError("[UIManager] ShowToast() called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(message))
                return;

            if (duration <= 0f)
                duration = DefaultToastDuration;

            var item = new ToastItem { Message = message, Duration = duration };

            if (_activeToastList.Count < MaxConcurrentToasts)
            {
                ShowToastDirect(item);
            }
            else
            {
                _toastQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// 立即创建并显示一个 Toast 面板（不经过队列）。
        /// </summary>
        /// <param name="item">Toast 数据。</param>
        private void ShowToastDirect(ToastItem item)
        {
            int index = LayerToIndex(UILayer.Toast);
            Transform? toastRoot = _layerRoots[index];

            if (toastRoot == null)
            {
                UnityEngine.Debug.LogError("[UIManager] ShowToast(): Toast layer root is null.");
                return;
            }

            // 程序化创建 Toast GameObject
            var toastGo = new GameObject("UIToast", typeof(RectTransform));
            toastGo.layer = toastRoot.gameObject.layer;
            toastGo.transform.SetParent(toastRoot, false);

            // 创建消息文本子对象
            var textGo = new GameObject("Message", typeof(RectTransform));
            textGo.transform.SetParent(toastGo.transform, false);
            var textComp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
            textComp.text = item.Message;
            textComp.fontSize = ToastFontSize;
            textComp.alignment = TMPro.TextAlignmentOptions.Center;

            var toast = toastGo.AddComponent<UIToast>();
            toast.messageText = textComp;

            toast.OnDismissed += OnToastDismissed;

            toast.Show(toastRoot, item.Message, item.Duration);

            _activeToastList.Add(new ActiveToast { Toast = toast, RemainingTime = item.Duration });
        }

        /// <summary>
        /// Toast 关闭回调：减少活跃计数，从队列中取出下一个 Toast 显示。
        /// </summary>
        private void OnToastDismissed()
        {
            // 从活跃列表中移除已关闭的 Toast
            _activeToastList.RemoveAll(a => a.Toast == null || a.Toast.gameObject == null);

            if (_toastQueue.Count > 0)
            {
                var next = _toastQueue.Dequeue();
                ShowToastDirect(next);
            }
        }

        // ── StartGuide / StopGuide ──

        /// <inheritdoc />
        public void StartGuide(string guideId, Action? onCompleted = null)
        {
            if (!_initialized)
            {
                UnityEngine.Debug.LogError("[UIManager] StartGuide() called before Initialize().");
                return;
            }

            if (string.IsNullOrEmpty(guideId))
            {
                UnityEngine.Debug.LogError("[UIManager] StartGuide(): guideId is null or empty.");
                return;
            }

            // 如果已有引导正在运行，先停止
            if (_currentGuide != null)
            {
                StopGuide();
            }

            LoadPanelPrefabAsync(guideId, (prefab) =>
            {
                if (prefab == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] StartGuide(): Failed to load guide '{guideId}'.");
                    return;
                }

                var instance = UnityEngine.Object.Instantiate(prefab);
                var guide = instance.GetComponent<UIGuide>();
                if (guide == null)
                {
                    UnityEngine.Debug.LogError($"[UIManager] StartGuide(): Prefab at '{guideId}' does not have a UIGuide component.");
                    DestroyObject(instance);
                    return;
                }

                int index = LayerToIndex(UILayer.Guide);
                Transform? guideRoot = _layerRoots[index];

                if (guideRoot == null)
                {
                    UnityEngine.Debug.LogError("[UIManager] StartGuide(): Guide layer root is null.");
                    DestroyObject(instance);
                    return;
                }

                guide.OnGuideCompleted = onCompleted;

                // 打开引导面板到 Guide 层
                guide.Open(guideRoot);

                _currentGuide = guide;
            });
        }

        /// <inheritdoc />
        public void StopGuide()
        {
            if (_currentGuide != null)
            {
                _currentGuide.Close();
                _currentGuide = null;
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
        /// 每帧更新，管理 Toast 倒计时和 LitMotion 动画系统。
        /// </summary>
        public void Tick()
        {
            float dt = Time.deltaTime;

            // Toast 倒计时管理
            var expired = new List<ActiveToast>();
            foreach (var active in _activeToastList)
            {
                active.RemainingTime -= dt;
                if (active.RemainingTime <= 0f)
                {
                    expired.Add(active);
                }
            }

            foreach (var e in expired)
            {
                _activeToastList.Remove(e);
                if (e.Toast != null && e.Toast.gameObject != null)
                {
                    e.Toast.Close();
                }
            }

            // LitMotion 手动调度（在非 MonoBehaviour 环境中确保动画更新）
            LitMotion.ManualMotionDispatcher.Default.Update(dt);
        }

        /// <summary>
        /// 帧末尾更新。
        /// </summary>
        public void LateTick() { }

        // ── IDisposable ──

        /// <summary>
        /// 释放 UI 管理器创建的所有 Canvas GameObject、面板、异步加载和清空内部状态。
        /// 可多次安全调用。
        /// </summary>
        public void Dispose()
        {
            // 取消所有 pending 的异步加载
            foreach (var kvp in _pendingLoads)
            {
                if (kvp.Value.IsValid())
                {
                    Addressables.Release(kvp.Value);
                }
            }
            _pendingLoads.Clear();

            // 停止当前引导
            if (_currentGuide != null)
            {
                _currentGuide.Close();
                _currentGuide = null;
            }

            // 关闭所有 Toast
            foreach (var active in _activeToastList)
            {
                if (active.Toast != null && active.Toast.gameObject != null)
                {
                    active.Toast.OnDismissed -= OnToastDismissed;
                    active.Toast.Close();
                }
            }
            _activeToastList.Clear();
            _toastQueue.Clear();

            // 关闭所有对话框
            foreach (var kvp in _currentDialogs)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    kvp.Value.Close();
                }
            }
            _currentDialogs.Clear();

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
