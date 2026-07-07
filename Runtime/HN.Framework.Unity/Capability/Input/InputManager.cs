#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityInputAction = UnityEngine.InputSystem.InputAction;

namespace HN.Framework.Unity.Capability.Input
{
    /// <summary>
    /// 输入管理器，基于 Unity InputSystem 实现 IInputManager 接口。
    /// 负责输入动作的注册/注销、ActionMap 管理，以及通过输入屏蔽器控制输入可用性。
    /// InputSystem 是事件驱动的，因此 InputManager 不实现 ITickable。
    /// </summary>
    public sealed class InputManager : IInputManager, IDisposable
    {
        private readonly InputActionAsset _asset;
        private readonly IInputBlocker _blocker;
        private readonly Dictionary<string, InputActionMap> _actionMaps;
        private readonly Dictionary<string, List<Action<InputContext>>> _callbacks;
        private readonly Dictionary<string, Action<UnityInputAction.CallbackContext>> _handlers;

        /// <summary>
        /// 初始化输入管理器。
        /// </summary>
        /// <param name="asset">InputActionAsset 输入配置资产。</param>
        /// <param name="blocker">可选的输入屏蔽器。若为 null，则使用内部默认屏蔽器。</param>
        /// <exception cref="ArgumentNullException">当 asset 为 null 时抛出。</exception>
        public InputManager(InputActionAsset asset, IInputBlocker? blocker = null)
        {
            _asset = asset ?? throw new ArgumentNullException(nameof(asset));
            _blocker = blocker ?? new InputBlocker();
            _actionMaps = new Dictionary<string, InputActionMap>();
            _callbacks = new Dictionary<string, List<Action<InputContext>>>();
            _handlers = new Dictionary<string, Action<UnityInputAction.CallbackContext>>();

            BuildActionMapIndex();
        }

        /// <summary>
        /// 获取输入屏蔽器实例。
        /// </summary>
        public IInputBlocker Blocker => _blocker;

        /// <summary>
        /// 注册输入动作回调。同一个回调注册两次将被去重（空操作）。
        /// </summary>
        /// <param name="actionName">输入动作名称。</param>
        /// <param name="callback">输入上下文回调。</param>
        /// <exception cref="ArgumentException">当 actionName 为 null 或空字符串时抛出。</exception>
        public void RegisterAction(string actionName, Action<InputContext> callback)
        {
            if (actionName == null)
                throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrEmpty(actionName))
                throw new ArgumentException("Action name cannot be empty.", nameof(actionName));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            var action = _asset.FindAction(actionName, throwIfNotFound: true);

            if (!_callbacks.TryGetValue(actionName, out var list))
            {
                list = new List<Action<InputContext>>();
                _callbacks[actionName] = list;
            }

            // Deduplicate: same callback registered twice is a no-op
            if (list.Contains(callback))
                return;

            bool isFirst = list.Count == 0;
            list.Add(callback);

            if (isFirst)
            {
                var handler = CreateActionHandler(actionName);
                _handlers[actionName] = handler;
                action.started += handler;
                action.performed += handler;
                action.canceled += handler;
            }
        }

        /// <summary>
        /// 注销输入动作回调。若该动作已无回调订阅，则同时取消 InputAction 的事件订阅。
        /// </summary>
        /// <param name="actionName">输入动作名称。</param>
        /// <param name="callback">输入上下文回调。</param>
        /// <exception cref="ArgumentException">当 actionName 为 null 或空字符串时抛出。</exception>
        public void UnregisterAction(string actionName, Action<InputContext> callback)
        {
            if (actionName == null)
                throw new ArgumentNullException(nameof(actionName));
            if (string.IsNullOrEmpty(actionName))
                throw new ArgumentException("Action name cannot be empty.", nameof(actionName));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!_callbacks.TryGetValue(actionName, out var list))
                return;

            list.Remove(callback);

            if (list.Count == 0)
            {
                _callbacks.Remove(actionName);

                if (_handlers.TryGetValue(actionName, out var handler))
                {
                    var action = _asset.FindAction(actionName);
                    if (action != null)
                    {
                        action.started -= handler;
                        action.performed -= handler;
                        action.canceled -= handler;
                    }

                    _handlers.Remove(actionName);
                }
            }
        }

        /// <summary>
        /// 启用指定的 ActionMap。
        /// </summary>
        /// <param name="mapName">ActionMap 名称。</param>
        public void EnableActionMap(string mapName)
        {
            if (_actionMaps.TryGetValue(mapName, out var map))
            {
                map.Enable();
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[InputManager] EnableActionMap: ActionMap '{mapName}' not found.");
            }
        }

        /// <summary>
        /// 禁用指定的 ActionMap。
        /// </summary>
        /// <param name="mapName">ActionMap 名称。</param>
        public void DisableActionMap(string mapName)
        {
            if (_actionMaps.TryGetValue(mapName, out var map))
            {
                map.Disable();
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[InputManager] DisableActionMap: ActionMap '{mapName}' not found.");
            }
        }

        /// <summary>
        /// 切换控制方案。V1 版本中为占位实现，仅输出警告日志。
        /// ControlScheme 切换需要 .inputactions 资产结构的深层支持，后续版本完善。
        /// </summary>
        /// <param name="scheme">控制方案名称。</param>
        public void SetControlScheme(string scheme)
        {
            UnityEngine.Debug.LogWarning($"[InputManager] SetControlScheme('{scheme}'): Not implemented in V1. ControlScheme switching requires .inputactions asset structure support.");
        }

        /// <summary>
        /// 释放所有资源：取消所有动作的事件订阅、清空回调字典、
        /// 禁用所有 ActionMap 并释放引用。适用于域重载时的清理。
        /// </summary>
        public void Dispose()
        {
            // Unsubscribe all action handlers
            foreach (var kvp in _handlers)
            {
                var actionName = kvp.Key;
                var handler = kvp.Value;

                var action = _asset.FindAction(actionName);
                if (action != null)
                {
                    action.started -= handler;
                    action.performed -= handler;
                    action.canceled -= handler;
                }
            }

            _handlers.Clear();
            _callbacks.Clear();

            // Disable all action maps
            foreach (var map in _actionMaps.Values)
            {
                map.Disable();
            }

            _actionMaps.Clear();
        }

        /// <summary>
        /// 扫描资产中的所有 ActionMap，建立名称索引。
        /// </summary>
        private void BuildActionMapIndex()
        {
            foreach (var map in _asset.actionMaps)
            {
                _actionMaps[map.name] = map;
            }
        }

        /// <summary>
        /// 为指定动作创建一个事件处理器，负责将 Unity 的 CallbackContext 转换为
        /// 框架的 InputContext，并在通过屏蔽检查后分发给所有已注册的回调。
        /// </summary>
        /// <param name="actionName">动作名称。</param>
        /// <returns>可订阅到 InputAction 事件的回调委托。</returns>
        private Action<UnityInputAction.CallbackContext> CreateActionHandler(string actionName)
        {
            return context =>
            {
                var phase = MapPhase(context.phase);
                var value = context.ReadValueAsObject();
                var inputContext = new InputContext(actionName, phase, value);

                if (_blocker.IsBlocked(0))
                    return;

                if (_callbacks.TryGetValue(actionName, out var list))
                {
                    // Iterate a copy to avoid modification-during-iteration issues
                    var snapshot = list.ToArray();
                    foreach (var callback in snapshot)
                    {
                        callback(inputContext);
                    }
                }
            };
        }

        /// <summary>
        /// 将 Unity 的 InputActionPhase 映射为框架的 InputPhase。
        /// Waiting 和 Disabled 阶段不会作为回调触发，此处仅处理标准三种阶段。
        /// </summary>
        private static InputPhase MapPhase(InputActionPhase phase)
        {
            return phase switch
            {
                InputActionPhase.Started => InputPhase.Started,
                InputActionPhase.Performed => InputPhase.Performed,
                InputActionPhase.Canceled => InputPhase.Canceled,
                _ => InputPhase.Canceled,
            };
        }
    }
}
