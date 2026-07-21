using System;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建管线控制窗口。
    /// 提供平台选择、输出路径、版本号等构建参数配置，以及一键构建功能。
    /// </summary>
    public class BuildPipelineWindow : EditorWindow
    {
        private int m_SelectedPlatformIndex;
        private string m_OutputPath = string.Empty;
        private bool m_EnableValidation = true;
        private string m_LogText = string.Empty;
        private Vector2 m_LogScrollPosition;
        private bool m_IsBuilding;

        private static readonly string[] s_PlatformNames =
        {
            "Windows 64",
            "macOS",
            "Linux 64",
            "Android",
            "iOS",
            "WebGL",
        };

        private static readonly BuildTarget[] s_PlatformTargets =
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneLinux64,
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.WebGL,
        };

        /// <summary>
        /// 打开构建管线窗口。
        /// </summary>
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Build Pipeline", false, 212)]
        public static void OpenWindow()
        {
            var window = GetWindow<BuildPipelineWindow>("Build Pipeline");
            window.Show();
        }

        private void OnEnable()
        {
            m_OutputPath = EditorPrefs.GetString("HN_BuildPipeline_OutputPath", "Builds/");

            string savedPlatform = EditorPrefs.GetString("HN_BuildPipeline_Platform", string.Empty);
            if (!string.IsNullOrEmpty(savedPlatform))
            {
                int index = Array.IndexOf(s_PlatformNames, savedPlatform);
                if (index >= 0)
                {
                    m_SelectedPlatformIndex = index;
                }
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetString("HN_BuildPipeline_OutputPath", m_OutputPath);
            EditorPrefs.SetString("HN_BuildPipeline_Platform", s_PlatformNames[m_SelectedPlatformIndex]);
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawBuildSettings();
            DrawBuildButton();
            DrawLogArea();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build Pipeline", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawBuildSettings()
        {
            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledGroupScope(m_IsBuilding))
            {
                m_SelectedPlatformIndex = EditorGUILayout.Popup(
                    "Target Platform",
                    m_SelectedPlatformIndex,
                    s_PlatformNames);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Output Path", GUILayout.Width(100));
                    m_OutputPath = EditorGUILayout.TextField(m_OutputPath);
                    if (GUILayout.Button("Browse", GUILayout.Width(60)))
                    {
                        string path = EditorUtility.SaveFolderPanel(
                            "Select Build Output Directory",
                            m_OutputPath,
                            string.Empty);
                        if (!string.IsNullOrEmpty(path))
                        {
                            m_OutputPath = path;
                        }
                    }
                }

                VersionConfig config = VersionConfig.GetOrCreateSettings();
                if (config != null)
                {
                    EditorGUILayout.LabelField(
                        "Version",
                        config.GetVersionString());
                }

                m_EnableValidation = EditorGUILayout.Toggle(
                    "Enable Validation",
                    m_EnableValidation);
            }

            EditorGUILayout.Space();
        }

        private void DrawBuildButton()
        {
            using (new EditorGUI.DisabledGroupScope(m_IsBuilding))
            {
                if (GUILayout.Button("Build", GUILayout.Height(30)))
                {
                    StartBuild();
                }
            }

            if (m_IsBuilding)
            {
                EditorGUILayout.Space();
                Rect progressRect = GUILayoutUtility.GetRect(18, 18, "TextField");
                EditorGUI.ProgressBar(progressRect, 0f, "Building in progress...");
                EditorGUILayout.Space();
            }

            EditorGUILayout.Space();
        }

        private void DrawLogArea()
        {
            EditorGUILayout.LabelField("Build Log", EditorStyles.boldLabel);

            m_LogScrollPosition = EditorGUILayout.BeginScrollView(
                m_LogScrollPosition,
                GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(
                m_LogText,
                GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void StartBuild()
        {
            m_LogText = string.Empty;
            m_IsBuilding = true;

            EditorPrefs.SetString("HN_BuildPipeline_OutputPath", m_OutputPath);

            BuildTarget target = s_PlatformTargets[m_SelectedPlatformIndex];
            BuildTargetGroup targetGroup = GetBuildTargetGroup(target);

            VersionConfig config = VersionConfig.GetOrCreateSettings();
            string version = config != null ? config.GetVersionString() : "0.0.0";

            AppendLog($"=== Build Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
            AppendLog($"Platform: {target}\n");
            AppendLog($"Version: {version}\n");
            AppendLog($"Output: {m_OutputPath}\n");
            AppendLog($"Validation: {(m_EnableValidation ? "Enabled" : "Disabled")}\n\n");

            // 使用 delayCall 确保 UI 在构建开始前完成刷新
            EditorApplication.delayCall += () =>
            {
                try
                {
                    var orchestrator = new BuildPipelineOrchestrator();
                    orchestrator.Configure(target, targetGroup, m_OutputPath, version);
                    orchestrator.Context.EnableValidation = m_EnableValidation;

                    orchestrator.AddStep(new PreBuildValidationStep());
                    orchestrator.AddStep(new AddressablesBuildStep());
                    orchestrator.AddStep(new PlayerBuildStep());
                    orchestrator.AddStep(new PostBuildStep());

                    bool success = orchestrator.Execute();

                    if (success)
                    {
                        AppendLog("\n*** Build completed successfully! ***\n");
                        Debug.Log("[BuildPipelineWindow] Build completed successfully.");
                    }
                    else
                    {
                        AppendLog("\n*** Build FAILED. See errors above. ***\n");
                        Debug.LogError("[BuildPipelineWindow] Build failed.");
                    }
                }
                catch (Exception ex)
                {
                    AppendLog($"\n*** Build ERROR: {ex.Message} ***\n");
                    Debug.LogError($"[BuildPipelineWindow] Build exception: {ex}");
                }
                finally
                {
                    m_IsBuilding = false;
                    Repaint();
                }
            };
        }

        /// <summary>
        /// 根据 BuildTarget 获取对应的 BuildTargetGroup。
        /// </summary>
        /// <param name="buildTarget">构建目标平台</param>
        /// <returns>对应的构建目标平台组</returns>
        private static BuildTargetGroup GetBuildTargetGroup(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;
                case BuildTarget.Android:
                    return BuildTargetGroup.Android;
                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;
                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;
                default:
                    return BuildTargetGroup.Standalone;
            }
        }

        private void AppendLog(string text)
        {
            m_LogText += text;
        }
    }
}
