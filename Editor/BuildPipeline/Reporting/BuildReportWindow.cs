using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建报告查看窗口。
    /// 加载并展示历史构建清单，支持查看版本、平台、时间、包体大小和校验结果。
    /// </summary>
    public class BuildReportWindow : EditorWindow
    {
        private List<BuildReportEntry> m_Entries = new();
        private int m_SelectedIndex = -1;
        private Vector2 m_ListScrollPosition;
        private Vector2 m_DetailScrollPosition;
        private string m_ReportsFolderPath;

        /// <summary>
        /// 打开构建报告窗口。
        /// </summary>
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Build Reports", false, 213)]
        public static void OpenWindow()
        {
            var window = GetWindow<BuildReportWindow>("Build Reports");
            window.Show();
        }

        private void OnEnable()
        {
            m_ReportsFolderPath = EditorPrefs.GetString(
                "HN_BuildReports_FolderPath",
                Path.Combine(Application.dataPath, "..", "BuildReports"));

            RefreshReportList();
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawReportList();
                DrawReportDetail();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Reports Folder:", GUILayout.Width(90));
                m_ReportsFolderPath = EditorGUILayout.TextField(m_ReportsFolderPath);

                if (GUILayout.Button("Browse", EditorStyles.toolbarButton, GUILayout.Width(55)))
                {
                    string path = EditorUtility.OpenFolderPanel(
                        "Select Reports Folder",
                        m_ReportsFolderPath,
                        string.Empty);
                    if (!string.IsNullOrEmpty(path))
                    {
                        m_ReportsFolderPath = path;
                        EditorPrefs.SetString("HN_BuildReports_FolderPath", m_ReportsFolderPath);
                        RefreshReportList();
                    }
                }

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(55)))
                {
                    RefreshReportList();
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(
                    $"{m_Entries.Count} report(s)",
                    EditorStyles.miniLabel);
            }
        }

        private void DrawReportList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            EditorGUILayout.LabelField("Builds", EditorStyles.boldLabel);

            m_ListScrollPosition = EditorGUILayout.BeginScrollView(m_ListScrollPosition);

            if (m_Entries.Count == 0)
            {
                EditorGUILayout.LabelField(
                    "No reports found.",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < m_Entries.Count; i++)
                {
                    BuildReportEntry entry = m_Entries[i];

                    bool wasSelected = (m_SelectedIndex == i);
                    GUI.backgroundColor = wasSelected
                        ? new Color(0.4f, 0.6f, 1.0f)
                        : Color.white;

                    if (GUILayout.Button(
                        $"{entry.Version}\n{entry.Platform}",
                        GUILayout.Height(36)))
                    {
                        m_SelectedIndex = i;
                    }

                    GUI.backgroundColor = Color.white;
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            GUILayout.Box(string.Empty, GUILayout.Width(2), GUILayout.ExpandHeight(true));
        }

        private void DrawReportDetail()
        {
            EditorGUILayout.BeginVertical();

            if (m_SelectedIndex < 0 || m_SelectedIndex >= m_Entries.Count)
            {
                EditorGUILayout.LabelField(
                    "Select a build from the list to view details.",
                    EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            BuildReportEntry entry = m_Entries[m_SelectedIndex];

            EditorGUILayout.LabelField("Build Details", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            m_DetailScrollPosition = EditorGUILayout.BeginScrollView(m_DetailScrollPosition);

            EditorGUILayout.LabelField("Version", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(entry.Version ?? "-");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Platform", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(entry.Platform ?? "-");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Time", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(entry.BuildTime ?? "-");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Size", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(FormatFileSize(entry.BuildSize));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Build Number", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(entry.BuildNumber.ToString());
            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(entry.Branch))
            {
                EditorGUILayout.LabelField("Branch", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(entry.Branch);
                EditorGUILayout.Space();
            }

            if (!string.IsNullOrEmpty(entry.CommitHash))
            {
                EditorGUILayout.LabelField("Commit", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(entry.CommitHash);
                EditorGUILayout.Space();
            }

            if (!string.IsNullOrEmpty(entry.OutputPath))
            {
                EditorGUILayout.LabelField("Output Path", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(entry.OutputPath);
                EditorGUILayout.Space();
            }

            // 校验结果
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);
            Color originalColor = GUI.color;
            GUI.color = entry.ValidationErrorCount > 0 ? Color.red : Color.green;
            EditorGUILayout.LabelField($"Errors: {entry.ValidationErrorCount}");
            GUI.color = entry.ValidationWarningCount > 0
                ? new Color(1.0f, 0.8f, 0.3f)
                : Color.green;
            EditorGUILayout.LabelField($"Warnings: {entry.ValidationWarningCount}");
            GUI.color = originalColor;

            EditorGUILayout.Space();

            // 原始文件路径
            EditorGUILayout.LabelField("Source File", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                entry.SourceFilePath ?? "-",
                EditorStyles.wordWrappedMiniLabel);

            if (GUILayout.Button("Open File Location"))
            {
                if (!string.IsNullOrEmpty(entry.SourceFilePath))
                {
                    EditorUtility.RevealInFinder(entry.SourceFilePath);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void RefreshReportList()
        {
            m_Entries.Clear();
            m_SelectedIndex = -1;

            if (!Directory.Exists(m_ReportsFolderPath))
            {
                return;
            }

            string[] jsonFiles = Directory.GetFiles(
                m_ReportsFolderPath,
                "*.json",
                SearchOption.TopDirectoryOnly);

            foreach (string filePath in jsonFiles)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    BuildReportEntry entry = ParseReportJson(json, filePath);
                    if (entry != null)
                    {
                        m_Entries.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[BuildReportWindow] Failed to parse {filePath}: {ex.Message}");
                }
            }

            // 按时间倒序排列（最新的在前）
            m_Entries.Sort((a, b) =>
                string.Compare(b.BuildTime, a.BuildTime, StringComparison.Ordinal));
        }

        /// <summary>
        /// 从原始 JSON 字符串中解析构建报告条目。
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <param name="sourceFilePath">来源文件路径</param>
        /// <returns>解析后的构建报告条目</returns>
        private static BuildReportEntry ParseReportJson(string json, string sourceFilePath)
        {
            var entry = new BuildReportEntry { SourceFilePath = sourceFilePath };

            entry.Version = ExtractJsonStringValue(json, "version");
            entry.Platform = ExtractJsonStringValue(json, "platform");
            entry.BuildTime = ExtractJsonStringValue(json, "buildTime");
            entry.Branch = ExtractJsonStringValue(json, "branch");
            entry.CommitHash = ExtractJsonStringValue(json, "commitHash");
            entry.OutputPath = ExtractJsonStringValue(json, "outputPath");

            entry.BuildSize = ExtractJsonLongValue(json, "buildSize");
            entry.BuildNumber = (int)ExtractJsonLongValue(json, "buildNumber");
            entry.ValidationErrorCount = (int)ExtractJsonLongValue(json, "validationErrorCount");
            entry.ValidationWarningCount = (int)ExtractJsonLongValue(json, "validationWarningCount");

            return entry;
        }

        private static string ExtractJsonStringValue(string json, string key)
        {
            string searchPattern = $"\"{key}\":";
            int startIndex = json.IndexOf(searchPattern, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            int valueStart = json.IndexOf('"', startIndex + searchPattern.Length);
            if (valueStart < 0)
            {
                return string.Empty;
            }

            int valueEnd = json.IndexOf('"', valueStart + 1);
            if (valueEnd < 0)
            {
                return string.Empty;
            }

            return json.Substring(valueStart + 1, valueEnd - valueStart - 1);
        }

        private static long ExtractJsonLongValue(string json, string key)
        {
            string searchPattern = $"\"{key}\":";
            int startIndex = json.IndexOf(searchPattern, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return 0;
            }

            int valueStart = startIndex + searchPattern.Length;
            // 跳过空格
            while (valueStart < json.Length && json[valueStart] == ' ')
            {
                valueStart++;
            }

            int valueEnd = valueStart;
            while (valueEnd < json.Length && (char.IsDigit(json[valueEnd]) || json[valueEnd] == '-'))
            {
                valueEnd++;
            }

            if (valueEnd > valueStart)
            {
                string numberStr = json.Substring(valueStart, valueEnd - valueStart);
                if (long.TryParse(numberStr, out long result))
                {
                    return result;
                }
            }

            return 0;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F2} {suffixes[suffixIndex]}";
        }

        /// <summary>
        /// 构建报告条目，用于在窗口中展示。
        /// </summary>
        private class BuildReportEntry
        {
            public string Version;
            public string Platform;
            public string BuildTime;
            public string Branch;
            public string CommitHash;
            public string OutputPath;
            public long BuildSize;
            public int BuildNumber;
            public int ValidationErrorCount;
            public int ValidationWarningCount;
            public string SourceFilePath;
        }
    }
}
