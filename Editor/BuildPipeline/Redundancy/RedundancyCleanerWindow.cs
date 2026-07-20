using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 冗余资源清理窗口。
    /// 展示冗余资源列表，支持一键标记删除和移动到备份目录。
    /// </summary>
    public class RedundancyCleanerWindow : EditorWindow
    {
        private DependencyGraph m_Graph;
        private RedundancyScanner m_Scanner;
        private Vector2 m_ScrollPosition;
        private bool m_IsScanning;
        private string m_BackupFolder = "Assets/_Redundant";
        private HashSet<string> m_SelectedForMove;
        private bool m_SelectAll = true;
        private string m_StatusMessage = string.Empty;

        /// <summary>
        /// 打开冗余资源清理窗口。
        /// </summary>
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Redundancy Cleaner", false, 211)]
        public static void OpenWindow()
        {
            var window = GetWindow<RedundancyCleanerWindow>("Redundancy Cleaner");
            window.Show();
        }

        private void OnEnable()
        {
            m_SelectedForMove = new HashSet<string>();
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
            EditorGUILayout.LabelField("Redundancy Cleaner", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Scans for unreferenced assets and helps clean them up.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Backup Folder:", GUILayout.Width(90));
                m_BackupFolder = EditorGUILayout.TextField(m_BackupFolder);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(m_IsScanning))
                {
                    if (GUILayout.Button("Build Graph & Scan", GUILayout.Width(150)))
                    {
                        StartScan();
                    }
                }

                if (GUILayout.Button("Clear Results", GUILayout.Width(100)))
                {
                    ClearResults();
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawResults()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            if (m_Scanner == null || m_Scanner.RedundantAssets.Count == 0)
            {
                EditorGUILayout.LabelField(
                    m_IsScanning
                        ? "Scanning..."
                        : "No results. Click 'Build Graph & Scan' to start.",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                // Select/Deselect all
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool newSelectAll = EditorGUILayout.Toggle("Select All", m_SelectAll, GUILayout.Width(200));
                    if (newSelectAll != m_SelectAll)
                    {
                        m_SelectAll = newSelectAll;
                        UpdateSelection();
                    }

                    EditorGUILayout.LabelField(
                        $"Found: {m_Scanner.RedundantAssets.Count} redundant assets");
                }

                EditorGUILayout.Space();

                foreach (string assetPath in m_Scanner.RedundantAssets)
                {
                    DrawAssetItem(assetPath);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawAssetItem(string assetPath)
        {
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                bool isSelected = m_SelectedForMove.Contains(assetPath);
                bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));

                if (newSelected != isSelected)
                {
                    if (newSelected)
                    {
                        m_SelectedForMove.Add(assetPath);
                    }
                    else
                    {
                        m_SelectedForMove.Remove(assetPath);
                        m_SelectAll = false;
                    }
                }

                // 路径行
                EditorGUILayout.LabelField(assetPath);

                // 定位按钮
                if (GUILayout.Button("Ping", GUILayout.Width(40)))
                {
                    Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (obj != null)
                    {
                        EditorGUIUtility.PingObject(obj);
                    }
                }
            }
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (!string.IsNullOrEmpty(m_StatusMessage))
            {
                EditorGUILayout.LabelField(m_StatusMessage, EditorStyles.miniLabel);
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledGroupScope(
                m_Scanner == null || m_SelectedForMove.Count == 0))
            {
                if (GUILayout.Button(
                    $"Move {m_SelectedForMove.Count} to Backup",
                    EditorStyles.toolbarButton))
                {
                    MoveSelectedToBackup();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void StartScan()
        {
            m_IsScanning = true;
            m_StatusMessage = "Building dependency graph...";
            m_SelectedForMove.Clear();

            EditorApplication.delayCall += () =>
            {
                try
                {
                    m_Graph = new DependencyGraph();
                    m_Graph.Build("Assets", (current, total) =>
                    {
                        if (EditorApplication.timeSinceStartup % 1 < 0.1)
                        {
                            m_StatusMessage = $"Building graph: {current}/{total}";
                            Repaint();
                        }
                    });

                    m_StatusMessage = "Scanning for redundant assets...";
                    m_Scanner = new RedundancyScanner(m_Graph);
                    m_Scanner.Scan((current, total) =>
                    {
                        if (EditorApplication.timeSinceStartup % 1 < 0.1)
                        {
                            m_StatusMessage = $"Scanning: {current}/{total}";
                            Repaint();
                        }
                    });

                    long savings = m_Scanner.GetEstimatedSavings();
                    string sizeStr = EditorUtility.FormatBytes(savings);
                    m_StatusMessage =
                        $"Found {m_Scanner.RedundantAssets.Count} redundant assets, " +
                        $"estimated savings: {sizeStr}";

                    UpdateSelection();
                }
                catch (System.Exception ex)
                {
                    m_StatusMessage = $"Error: {ex.Message}";
                    Debug.LogError($"[RedundancyCleaner] Scan failed: {ex.Message}");
                }
                finally
                {
                    m_IsScanning = false;
                    Repaint();
                }
            };
        }

        private void ClearResults()
        {
            m_Scanner = null;
            m_Graph = null;
            m_SelectedForMove.Clear();
            m_StatusMessage = string.Empty;
            m_SelectAll = true;
        }

        private void UpdateSelection()
        {
            m_SelectedForMove.Clear();
            if (m_SelectAll && m_Scanner != null)
            {
                foreach (string path in m_Scanner.RedundantAssets)
                {
                    m_SelectedForMove.Add(path);
                }
            }
        }

        private void MoveSelectedToBackup()
        {
            if (m_Scanner == null || m_SelectedForMove.Count == 0)
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "Move Redundant Assets",
                $"Move {m_SelectedForMove.Count} assets to '{m_BackupFolder}'?\n" +
                "You can review and delete them manually later.",
                "Move", "Cancel"))
            {
                return;
            }

            // 使用扫描器的 MoveToFolder 方法
            int moved = m_Scanner.MoveToFolder(m_BackupFolder);
            m_StatusMessage = $"Moved {moved} redundant assets to {m_BackupFolder}";

            // 刷新结果
            ClearResults();
        }
    }
}
