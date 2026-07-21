using System.Collections.Generic;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Unity.Driver.Platform;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.Input
{
    /// <summary>
    /// 输入调试器窗口，用于在运行时实时监控输入系统状态。
    /// 显示 GameWorld 输入管理器注册状态及最近触发的输入动作。
    /// 通过 <see cref="OnInspectorUpdate"/> 驱动窗口实时刷新。
    /// V1 版本不直接依赖 UnityEngine.InputSystem 命名空间。
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
            _recentActions.Clear();
        }

        private void OnDisable()
        {
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
            DrawStatusLine("InputManager:", inputManager != null ? "Registered" : "Not Registered", inputManager != null);

            if (inputManager != null)
            {
                var blocker = inputManager.Blocker;
                if (blocker != null)
                {
                    bool blocked = blocker.IsBlocked(0);
                    DrawStatusLine("InputBlocker:", blocked ? "Blocked" : "Active", !blocked);
                }
            }
        }

        /// <summary>
        /// 绘制最近触发的输入动作列表。
        /// </summary>
        private void DrawRecentActions()
        {
            EditorGUILayout.LabelField(
                $"Recently Fired Actions (Last {MaxRecentActions})", EditorStyles.boldLabel);

            if (_recentActions.Count == 0)
            {
                EditorGUILayout.LabelField("  No actions recorded. Enter Play Mode to capture input events.");
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

                EditorGUILayout.LabelField(
                    $"[{action.Phase,-9}] {action.ActionName}",
                    labelStyle);
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 表示一条最近触发的输入动作记录。
        /// </summary>
        private readonly struct RecentAction
        {
            public readonly string ActionName;
            public readonly InputPhase Phase;

            public RecentAction(string actionName, InputPhase phase)
            {
                ActionName = actionName;
                Phase = phase;
            }
        }

        /// <summary>
        /// 绘制单行状态标签（Label + 带颜色的状态值）。
        /// </summary>
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
