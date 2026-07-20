using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.EditorUI.Utility
{
    /// <summary>
    /// 布局辅助工具类，提供 Inspector 绘制中常用的 UI 元素方法。
    /// </summary>
    public static class EditorLayout
    {
        /// <summary>
        /// 绘制一条水平分割线。
        /// </summary>
        /// <param name="color">线条颜色，默认灰色</param>
        /// <param name="thickness">线条厚度（像素）</param>
        /// <param name="padding">上下内边距（像素）</param>
        public static void DrawHorizontalLine(Color? color = null, float thickness = 1f, float padding = 4f)
        {
            Color lineColor = color ?? new Color(0.5f, 0.5f, 0.5f, 0.5f);
            Rect rect = EditorGUILayout.GetControlRect(false, thickness + padding * 2);
            rect.y += padding;
            rect.height = thickness;
            EditorGUI.DrawRect(rect, lineColor);
        }

        /// <summary>
        /// 绘制一个带图标的消息框（类似 EditorGUILayout.HelpBox 的额外封装）。
        /// </summary>
        public static void DrawHelpBox(string message, MessageType type, bool wide = true)
        {
            EditorGUILayout.HelpBox(message, type, wide);
        }

        /// <summary>
        /// 绘制粗体标签。
        /// </summary>
        public static void DrawBoldLabel(string text, params GUILayoutOption[] options)
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            EditorGUILayout.LabelField(text, style, options);
        }

        /// <summary>
        /// 绘制带颜色的标签。
        /// </summary>
        public static void DrawColoredLabel(string text, Color color, params GUILayoutOption[] options)
        {
            Color original = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField(text, options);
            GUI.color = original;
        }

        /// <summary>
        /// 开始一个带框线的分组区域。
        /// </summary>
        public static void BeginBoxGroup(string title = null)
        {
            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            }
            EditorGUILayout.BeginVertical(GUI.skin.box);
        }

        /// <summary>
        /// 结束一个带框线的分组区域。
        /// </summary>
        public static void EndBoxGroup()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        /// <summary>
        /// 绘制一个可折叠的分组标题，返回当前折叠状态。
        /// </summary>
        /// <param name="foldout">当前折叠状态</param>
        /// <param name="title">分组标题</param>
        /// <returns>更新后的折叠状态</returns>
        public static bool DrawFoldout(bool foldout, string title)
        {
            return EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldout);
        }

        /// <summary>
        /// 绘制一个可折叠的分组标题（带粗体样式），返回当前折叠状态。
        /// </summary>
        public static bool DrawBoldFoldout(bool foldout, string title)
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
            return EditorGUILayout.Foldout(foldout, title, true, style);
        }
    }
}
