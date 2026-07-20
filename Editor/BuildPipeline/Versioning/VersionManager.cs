using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 版本管理器。提供版本号递增、Git 标签创建和构建清单生成功能。
    /// </summary>
    public static class VersionManager
    {
        private const string VersionConfigPath = "Assets/Settings/BuildPipeline/VersionConfig.asset";

        /// <summary>获取或创建版本配置文件</summary>
        /// <returns>VersionConfig 实例</returns>
        public static VersionConfig GetOrCreateVersionConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<VersionConfig>(VersionConfigPath);
            if (config != null)
            {
                return config;
            }

            // 确保目录存在
            string directory = Path.GetDirectoryName(VersionConfigPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            config = ScriptableObject.CreateInstance<VersionConfig>();
            config.Major = 0;
            config.Minor = 1;
            config.Patch = 0;
            config.BuildNumber = 1;

            AssetDatabase.CreateAsset(config, VersionConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VersionManager] Created new VersionConfig at {VersionConfigPath}");
            return config;
        }

        /// <summary>递增 Build Number</summary>
        /// <returns>递增后的值</returns>
        public static int IncrementBuildNumber()
        {
            var config = GetOrCreateVersionConfig();
            config.BuildNumber++;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log($"[VersionManager] Build number incremented to {config.BuildNumber}");
            return config.BuildNumber;
        }

        /// <summary>递增 Patch 版本号，BuildNumber 重置为 1</summary>
        /// <returns>更新后的版本字符串</returns>
        public static string IncrementPatch()
        {
            var config = GetOrCreateVersionConfig();
            config.Patch++;
            config.BuildNumber = 1;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            string version = GetVersionString(config);
            Debug.Log($"[VersionManager] Patch incremented: {version}");
            return version;
        }

        /// <summary>递增 Minor 版本号，Patch 归零，BuildNumber 重置为 1</summary>
        /// <returns>更新后的版本字符串</returns>
        public static string IncrementMinor()
        {
            var config = GetOrCreateVersionConfig();
            config.Minor++;
            config.Patch = 0;
            config.BuildNumber = 1;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            string version = GetVersionString(config);
            Debug.Log($"[VersionManager] Minor incremented: {version}");
            return version;
        }

        /// <summary>递增 Major 版本号，Minor 和 Patch 归零，BuildNumber 重置为 1</summary>
        /// <returns>更新后的版本字符串</returns>
        public static string IncrementMajor()
        {
            var config = GetOrCreateVersionConfig();
            config.Major++;
            config.Minor = 0;
            config.Patch = 0;
            config.BuildNumber = 1;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            string version = GetVersionString(config);
            Debug.Log($"[VersionManager] Major incremented: {version}");
            return version;
        }

        /// <summary>获取当前版本字符串，格式为 Major.Minor.Patch (BuildNumber)</summary>
        /// <returns>版本字符串</returns>
        public static string GetVersionString()
        {
            var config = GetOrCreateVersionConfig();
            return GetVersionString(config);
        }

        /// <summary>根据 VersionConfig 生成版本字符串</summary>
        private static string GetVersionString(VersionConfig config)
        {
            return $"{config.Major}.{config.Minor}.{config.Patch} ({config.BuildNumber})";
        }

        /// <summary>尝试创建 Git 标签</summary>
        /// <param name="version">版本号字符串</param>
        /// <returns>成功返回 true，否则返回 false</returns>
        public static bool TryCreateGitTag(string version)
        {
            try
            {
                // 检查是否为 Git 仓库
                string projectPath = Application.dataPath;
                string repoPath = Path.GetFullPath(Path.Combine(projectPath, ".."));
                string gitDir = Path.Combine(repoPath, ".git");

                if (!Directory.Exists(gitDir))
                {
                    Debug.LogWarning("[VersionManager] Not a git repository. Skipping tag creation.");
                    return false;
                }

                // Sanitize version string for tag name
                string tagName = "v" + version.Replace(" ", "_").Replace("(", "").Replace(")", "");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"tag -a \"{tagName}\" -m \"Build {version}\"",
                        WorkingDirectory = repoPath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    },
                };

                process.Start();
                process.WaitForExit(30000);

                if (process.ExitCode == 0)
                {
                    Debug.Log($"[VersionManager] Git tag '{tagName}' created successfully.");
                    return true;
                }
                else
                {
                    string error = process.StandardError.ReadToEnd();
                    Debug.LogWarning($"[VersionManager] Failed to create git tag: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersionManager] Exception creating git tag: {ex.Message}");
                return false;
            }
        }

        /// <summary>根据构建上下文生成构建清单</summary>
        /// <param name="context">构建上下文</param>
        /// <returns>BuildManifest 实例</returns>
        public static BuildManifest GenerateBuildManifest(BuildContext context)
        {
            var manifest = new BuildManifest
            {
                BuildTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Version = context.Version,
                Platform = context.BuildTarget.ToString(),
                BuildNumber = GetOrCreateVersionConfig().BuildNumber,
                OutputPath = context.OutputPath,
            };

            // 获取 Git 信息
            try
            {
                manifest.Branch = GetGitBranch();
                manifest.CommitHash = GetGitCommitHash();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[VersionManager] Failed to retrieve git info: {ex.Message}");
            }

            // 提取构建包体大小
            if (context.CustomData.TryGetValue("BuildReport", out var reportObj) &&
                reportObj is BuildReport report)
            {
                manifest.BuildSize = (long)report.summary.totalSize;
            }

            return manifest;
        }

        /// <summary>获取当前 Git 分支名</summary>
        private static string GetGitBranch()
        {
            return RunGitCommand("rev-parse --abbrev-ref HEAD")?.Trim();
        }

        /// <summary>获取当前 Git Commit Hash</summary>
        private static string GetGitCommitHash()
        {
            return RunGitCommand("rev-parse --short HEAD")?.Trim();
        }

        /// <summary>执行 Git 命令并返回标准输出</summary>
        private static string RunGitCommand(string arguments)
        {
            string repoPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            if (!Directory.Exists(Path.Combine(repoPath, ".git")))
            {
                return string.Empty;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = repoPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            return output;
        }
    }
}
