using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 资源质检结果查看窗口。
    /// 展示校验结果列表，按严重度着色、支持筛选和导出。
    /// </summary>
    public class AssetValidatorWindow : EditorWindow
    {
        private AssetValidator m_Validator;
        private List<RuleResult> m_Results;
        private Vector2 m_ScrollPosition;
        private string m_TargetPath = "Assets";
        private bool m_Recursive = true;
        private bool m_IsRunning;
        private string m_FilterText = string.Empty;
        private RuleSeverity m_MinSeverityFilter = RuleSeverity.Info;

        private readonly string[] m_SeverityLabels =
            { "All", "Info+", "Warning+", "Error+", "Fatal Only" };

        /// <summary>
        /// 打开资源质检窗口。
        /// </summary>
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Asset Validator", false, 210)]
        public static void OpenWindow()
        {
            var window = GetWindow<AssetValidatorWindow>("Asset Validator");
            window.Show();
        }

        private void OnEnable()
        {
            m_Validator = new AssetValidator();
            m_Results = new List<RuleResult>();
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawResults();
            DrawStatusBar();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Asset Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Target Path:", GUILayout.Width(80));
                m_TargetPath = EditorGUILayout.TextField(m_TargetPath);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                m_Recursive = EditorGUILayout.Toggle("Recursive", m_Recursive);
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledGroupScope(m_IsRunning))
                {
                    if (GUILayout.Button("Validate", GUILayout.Width(100)))
                    {
                        RunValidation();
                    }
                }

                if (GUILayout.Button("Clear", GUILayout.Width(80)))
                {
                    m_Results.Clear();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Search:", GUILayout.Width(55));
                m_FilterText = EditorGUILayout.TextField(m_FilterText);

                EditorGUILayout.LabelField("Min Severity:", GUILayout.Width(80));
                m_MinSeverityFilter = (RuleSeverity)GUILayout.SelectionGrid(
                    (int)m_MinSeverityFilter, m_SeverityLabels, 5, EditorStyles.miniButton);
            }

            EditorGUILayout.Space();
        }

        private void DrawResults()
        {
            var filtered = FilterResults();

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            if (filtered.Count == 0)
            {
                EditorGUILayout.LabelField(
                    m_IsRunning ? "Validating..." : "No results. Click 'Validate' to start.",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                foreach (RuleResult result in filtered)
                {
                    DrawResultItem(result);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawResultItem(RuleResult result)
        {
            Color originalBg = GUI.backgroundColor;
            Color contentColor = GetSeverityColor(result.Severity);

            GUI.backgroundColor = contentColor;
            EditorGUILayout.BeginVertical("box");

            // 标题行
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"[{result.Severity}] {result.RuleName}",
                    EditorStyles.boldLabel,
                    GUILayout.Width(250));
                EditorGUILayout.LabelField(result.AssetPath, EditorStyles.miniLabel);
            }

            // 消息行
            EditorGUILayout.LabelField(result.Message, EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
            GUI.backgroundColor = originalBg;
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            int total = m_Results.Count;
            int errors = m_Results.Count(r => r.Severity >= RuleSeverity.Error);
            int warnings = m_Results.Count(r => r.Severity == RuleSeverity.Warning);

            EditorGUILayout.LabelField(
                $"Total: {total}  |  Errors: {errors}  |  Warnings: {warnings}",
                EditorStyles.miniLabel);

            if (total > 0 && GUILayout.Button("Export to JSON", EditorStyles.toolbarButton))
            {
                ExportResults();
            }

            EditorGUILayout.EndHorizontal();
        }

        private List<RuleResult> FilterResults()
        {
            return m_Results
                .Where(r => (int)r.Severity >= (int)m_MinSeverityFilter)
                .Where(r => string.IsNullOrEmpty(m_FilterText)
                    || r.AssetPath.IndexOf(m_FilterText, StringComparison.OrdinalIgnoreCase) >= 0
                    || r.Message.IndexOf(m_FilterText, StringComparison.OrdinalIgnoreCase) >= 0
                    || r.RuleName.IndexOf(m_FilterText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        private Color GetSeverityColor(RuleSeverity severity)
        {
            switch (severity)
            {
                case RuleSeverity.Info:
                    return new Color(0.8f, 0.9f, 0.8f);
                case RuleSeverity.Warning:
                    return new Color(1.0f, 0.9f, 0.6f);
                case RuleSeverity.Error:
                    return new Color(1.0f, 0.7f, 0.7f);
                case RuleSeverity.Fatal:
                    return new Color(1.0f, 0.5f, 0.5f);
                default:
                    return Color.white;
            }
        }

        private void RunValidation()
        {
            m_IsRunning = true;
            m_Results.Clear();
            EditorApplication.delayCall += () =>
            {
                try
                {
                    m_Results = new List<RuleResult>(
                        m_Validator.Validate(m_TargetPath, m_Recursive));
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AssetValidator] Validation failed: {ex.Message}");
                }
                finally
                {
                    m_IsRunning = false;
                    Repaint();
                }
            };
        }

        private void ExportResults()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Validation Results",
                Application.dataPath,
                "ValidationResults.json",
                "json");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            try
            {
                var data = new
                {
                    exportTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    targetPath = m_TargetPath,
                    totalResults = m_Results.Count,
                    results = m_Results.Select(r => new
                    {
                        rule = r.RuleName,
                        severity = r.Severity.ToString(),
                        assetPath = r.AssetPath,
                        message = r.Message,
                    }),
                };

                string json = JsonUtility.ToJson(data, true);
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"[AssetValidator] Results exported to: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetValidator] Export failed: {ex.Message}");
            }
        }
    }
}
