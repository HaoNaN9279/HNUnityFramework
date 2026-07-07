using System.Collections.Generic;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Unity.Driver.Platform;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HN.Framework.Editor.Input
{
    /// <summary>
    /// 输入调试器窗口，用于在运行时实时监控输入系统状态。
    /// 显示 GameWorld 输入管理器注册状态、已连接设备、控制方案及最近触发的输入动作。
    /// 通过 <see cref="OnInspectorUpdate"/> 驱动窗口实时刷新，无需进入 Play Mode。
    /// </summary>
    public sealed class InputDebugger : EditorWindow
    {
        private const int MaxRecentActions = 20;
        private readonly Queue<RecentAction> _recentActions = new Queue<RecentAction>();
        private Vector2 _scrollPosition;

        /// <summary>
        /// 打开输入调试器窗口。
        /// </summary>
        [MenuItem("Window/HNUnityFramework/Input Debugger")]
        private static void Open()
        {
            var window = GetWindow<InputDebugger>();
            window.titleContent = new GUIContent("Input Debugger");
            window.Show();
        }

        private void OnEnable()
        {
            InputSystem.onActionChange += OnActionChange;
            _recentActions.Clear();
        }

        private void OnDisable()
        {
            InputSystem.onActionChange -= OnActionChange;
            _recentActions.Clear();
        }

        /// <summary>
        /// 利用 Inspector 更新周期驱动窗口重绘，实现准实时刷新。
        /// </summary>
        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            DrawInputSystemStatus();
            EditorGUILayout.Space();
            DrawDevices();
            EditorGUILayout.Space();
            DrawControlScheme();
            EditorGUILayout.Space();
            DrawRecentActions();
        }

        /// <summary>
        /// 绘制输入系统状态区：GameWorld / InputManager / InputBlocker 的当前状态。
        /// </summary>
        private static void DrawInputSystemStatus()
        {
            EditorGUILayout.LabelField("Input System Status", EditorStyles.boldLabel);

            var driver = Object.FindObjectOfType<GameWorldDriver>();
            var world = driver?.World;
            var inputManager = world?.InputManager;

            if (world == null)
            {
                EditorGUILayout.HelpBox(
                    "No GameWorld available in the current scene. " +
                    "Ensure a GameObject with GameWorldDriver exists.",
                    MessageType.Warning);
                return;
            }

            DrawStatusLine("GameWorld:", "Active", true);
            DrawStatusLine(
                "InputManager:",
                inputManager != null ? "Registered" : "Not Registered",
                inputManager != null);

            if (inputManager != null)
            {
                var blocker = inputManager.Blocker;
                if (blocker != null)
                {
                    bool blocked = blocker.IsBlocked(0);
                    DrawStatusLine(
                        "InputBlocker:", blocked ? "Blocked" : "Active", !blocked);
                }
            }
        }

        /// <summary>
        /// 绘制连接设备列表，列出 InputSystem 当前检测到的所有输入设备。
        /// </summary>
        private static void DrawDevices()
        {
            EditorGUILayout.LabelField("Connected Devices", EditorStyles.boldLabel);

            var devices = InputSystem.devices;
            if (devices != null && devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    if (device != null)
                    {
                        EditorGUILayout.LabelField(
                            $"  {device.displayName} ({device.GetType().Name})");
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("  (none)");
            }
        }

        /// <summary>
        /// 绘制当前控制方案信息。V1 中 SetControlScheme 为占位实现，此处显示提示信息。
        /// </summary>
        private static void DrawControlScheme()
        {
            EditorGUILayout.LabelField("Current Control Scheme", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Control scheme switching is not implemented in V1.\n" +
                "InputManager.SetControlScheme is a placeholder.",
                MessageType.Info);
        }

        /// <summary>
        /// 绘制最近触发的输入动作列表，按时间从旧到新排列，最多保留 <see cref="MaxRecentActions"/> 条。
        /// </summary>
        private void DrawRecentActions()
        {
            EditorGUILayout.LabelField(
                $"Recently Fired Actions (Last {MaxRecentActions})", EditorStyles.boldLabel);

            if (_recentActions.Count == 0)
            {
                EditorGUILayout.LabelField("  No actions fired yet. Press some input...");
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(
                _scrollPosition, GUILayout.Height(300));

            foreach (var action in _recentActions)
            {
                Color phaseColor = action.Phase switch
                {
                    InputPhase.Started => Color.cyan,
                    InputPhase.Performed => Color.green,
                    InputPhase.Canceled => Color.gray,
                    _ => Color.white,
                };

                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = phaseColor },
                    fontStyle = FontStyle.Bold,
                };

                string valueDisplay = action.Value != null
                    ? FormatValue(action.Value)
                    : "-";

                EditorGUILayout.LabelField(
                    $"[{action.Phase,-9}] {action.ActionName}  (value: {valueDisplay})",
                    labelStyle);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 将输入值格式化为易读的字符串，针对 Unity 常用类型做友好显示。
        /// </summary>
        /// <param name="value">输入值。</param>
        /// <returns>格式化后的字符串。</returns>
        private static string FormatValue(object value)
        {
            if (value is Vector2 v2)
                return $"({v2.x:F2}, {v2.y:F2})";
            if (value is Vector3 v3)
                return $"({v3.x:F2}, {v3.y:F2}, {v3.z:F2})";
            if (value is float f)
                return f.ToString("F2");
            if (value is double d)
                return d.ToString("F2");
            return value.ToString();
        }

        /// <summary>
        /// 全局 InputSystem 动作变化回调，将事件转换为 <see cref="RecentAction"/> 并加入队列。
        /// 仅在 Started / Performed / Canceled 阶段记录，忽略其他系统事件。
        /// </summary>
        /// <param name="obj">触发变化的 InputAction 实例。</param>
        /// <param name="change">变化类型。</param>
        private void OnActionChange(object obj, InputActionChange change)
        {
            if (obj is not InputAction action)
                return;

            InputPhase? phase = change switch
            {
                InputActionChange.ActionStarted => InputPhase.Started,
                InputActionChange.ActionPerformed => InputPhase.Performed,
                InputActionChange.ActionCanceled => InputPhase.Canceled,
                _ => null,
            };

            if (!phase.HasValue)
                return;

            object value = null;
            if (change == InputActionChange.ActionPerformed)
            {
                try
                {
                    value = action.ReadValueAsObject();
                }
                catch
                {
                    // 读取输入值失败时静默处理（best-effort）
                }
            }

            if (_recentActions.Count >= MaxRecentActions)
                _recentActions.Dequeue();

            _recentActions.Enqueue(new RecentAction(action.name, phase.Value, value));
        }

        /// <summary>
        /// 表示一条最近触发的输入动作记录。
        /// </summary>
        private readonly struct RecentAction
        {
            /// <summary>动作名称。</summary>
            public readonly string ActionName;

            /// <summary>输入阶段。</summary>
            public readonly InputPhase Phase;

            /// <summary>输入值（仅在 Performed 阶段有值）。</summary>
            public readonly object Value;

            public RecentAction(string actionName, InputPhase phase, object value)
            {
                ActionName = actionName;
                Phase = phase;
                Value = value;
            }
        }

        /// <summary>
        /// 绘制单行状态标签（Label + 带颜色的状态值）。
        /// </summary>
        /// <param name="label">标签名称。</param>
        /// <param name="value">状态值。</param>
        /// <param name="isGood">true 显示绿色，false 显示红色。</param>
        private static void DrawStatusLine(string label, string value, bool isGood)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.label, GUILayout.Width(120));
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = isGood ? Color.green : Color.red },
            };
            EditorGUILayout.LabelField(value, style);
            EditorGUILayout.EndHorizontal();
        }
    }
}
