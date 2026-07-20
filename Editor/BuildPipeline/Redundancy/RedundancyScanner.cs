using System;
using System.Collections.Generic;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 冗余资源扫描器。
    /// 基于 DependencyGraph 分析引用关系，识别未被任何非冗余资源引用的孤立资源。
    /// </summary>
    public class RedundancyScanner
    {
        private readonly DependencyGraph m_Graph;
        private List<string> m_RedundantAssets;
        private bool m_Scanned;

        /// <summary>
        /// 创建冗余扫描器。
        /// </summary>
        /// <param name="graph">依赖图（需已构建）</param>
        public RedundancyScanner(DependencyGraph graph)
        {
            m_Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            m_RedundantAssets = new List<string>();
        }

        /// <summary>
        /// 扫描结果：冗余资源列表。
        /// </summary>
        public IReadOnlyList<string> RedundantAssets => m_RedundantAssets.AsReadOnly();

        /// <summary>
        /// 已扫描标记。
        /// </summary>
        public bool IsScanned => m_Scanned;

        /// <summary>
        /// 执行冗余扫描。
        /// 策略：将项目资源分为"根资源"和"非根资源"。
        /// 根资源是直接在 Project 窗口中可见的常用资源类型（Scenes, Prefabs, ScriptableObjects, 等）。
        /// 未被任何根资源直接或间接引用的资源标记为冗余。
        /// </summary>
        /// <param name="onProgress">进度回调</param>
        public void Scan(Action<int, int> onProgress = null)
        {
            if (!m_Graph.IsBuilt)
            {
                throw new InvalidOperationException(
                    "DependencyGraph must be built before scanning.");
            }

            m_RedundantAssets.Clear();

            // 确定"根资源"类型：这些是用户直接使用的资源
            var rootGuids = new HashSet<string>();
            string[] rootTypes =
            {
                "t:Scene", "t:Prefab", "t:ScriptableObject",
                "t:Material", "t:AnimatorController", "t:AnimationClip",
                "t:AudioMixer", "t:RenderTexture", "t:PhysicMaterial",
            };

            foreach (string typeFilter in rootTypes)
            {
                string[] guids = AssetDatabase.FindAssets(typeFilter, new[] { "Assets" });
                foreach (string guid in guids)
                {
                    rootGuids.Add(guid);
                }
            }

            // 收集所有非根资源（潜在冗余）
            string[] allGuids = AssetDatabase.FindAssets("t:Object", new[] { "Assets" });
            int total = allGuids.Length;
            int processed = 0;

            foreach (string guid in allGuids)
            {
                onProgress?.Invoke(++processed, total);

                if (rootGuids.Contains(guid))
                {
                    continue;
                }

                // 跳过文件夹
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    continue;
                }

                // 跳过特定路径
                if (ShouldSkip(path))
                {
                    continue;
                }

                // 检查是否被任何根资源引用
                int referencerCount = m_Graph.GetTotalReferencerCount(path);

                // 如果引用者为 0，则可能为冗余
                if (referencerCount == 0)
                {
                    // 二次验证：检查是否有任何直接引用
                    var directRefs = m_Graph.GetReferencers(path);
                    if (directRefs.Count == 0)
                    {
                        m_RedundantAssets.Add(path);
                    }
                }
            }

            m_Scanned = true;
        }

        /// <summary>
        /// 获取冗余资源的预估节省空间（字节）。
        /// </summary>
        /// <returns>总字节数</returns>
        public long GetEstimatedSavings()
        {
            long totalBytes = 0;

            foreach (string path in m_RedundantAssets)
            {
                try
                {
                    var systemPath = System.IO.Path.GetFullPath(path);
                    if (System.IO.File.Exists(systemPath))
                    {
                        var fileInfo = new System.IO.FileInfo(systemPath);
                        totalBytes += fileInfo.Length;
                    }
                }
                catch
                {
                    // 跳过无法读取的文件
                }
            }

            return totalBytes;
        }

        /// <summary>
        /// 将标记的冗余资源移动到指定目录（以备审查）。
        /// </summary>
        /// <param name="backupFolder">备份目录，如 "Assets/_Redundant"</param>
        /// <returns>移动成功的资源数量</returns>
        public int MoveToFolder(string backupFolder)
        {
            int moved = 0;

            if (!AssetDatabase.IsValidFolder(backupFolder))
            {
                string parent = System.IO.Path.GetDirectoryName(backupFolder);
                string folderName = System.IO.Path.GetFileName(backupFolder);
                AssetDatabase.CreateFolder(parent, folderName);
            }

            foreach (string path in m_RedundantAssets)
            {
                string fileName = System.IO.Path.GetFileName(path);
                string destPath = $"{backupFolder}/{fileName}";

                if (AssetDatabase.MoveAsset(path, destPath) == string.Empty)
                {
                    moved++;
                }
            }

            AssetDatabase.SaveAssets();
            return moved;
        }

        private static bool ShouldSkip(string path)
        {
            // 跳过编辑器特有目录
            if (path.StartsWith("Assets/Editor/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("Assets/Plugins/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("Assets/StreamingAssets/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 跳过脚本文件
            if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
