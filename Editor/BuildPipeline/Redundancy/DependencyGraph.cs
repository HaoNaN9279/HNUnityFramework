using System;
using System.Collections.Generic;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 资源引用图数据结构。
    /// 基于 AssetDatabase 构建资源的全量依赖关系，支持引用分析和冗余检测。
    /// </summary>
    public class DependencyGraph
    {
        private readonly Dictionary<string, HashSet<string>> m_Dependencies;
        private readonly Dictionary<string, HashSet<string>> m_Referencers;
        private bool m_Built;

        /// <summary>
        /// 创建一个空的依赖图。
        /// </summary>
        public DependencyGraph()
        {
            m_Dependencies = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            m_Referencers = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 所有已分析的资源路径。
        /// </summary>
        public IReadOnlyCollection<string> AllAssets => m_Dependencies.Keys;

        /// <summary>
        /// 获取指定资源的直接依赖项。
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>依赖项集合（如未找到返回空集合）</returns>
        public IReadOnlyCollection<string> GetDependencies(string assetPath)
        {
            if (m_Dependencies.TryGetValue(assetPath, out var deps))
            {
                return deps;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// 获取直接引用指定资源的资源列表。
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>引用者集合</returns>
        public IReadOnlyCollection<string> GetReferencers(string assetPath)
        {
            if (m_Referencers.TryGetValue(assetPath, out var refs))
            {
                return refs;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// 获取指定资源的间接引用者数量（递归）。
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>间接引用者数量</returns>
        public int GetTotalReferencerCount(string assetPath)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CountReferencers(assetPath, visited);
            // 排除自身
            return visited.Contains(assetPath) ? visited.Count - 1 : visited.Count;
        }

        /// <summary>
        /// 构建全量依赖图。
        /// 扫描指定目录下的所有资源，分析依赖关系。
        /// </summary>
        /// <param name="rootPath">根路径，如 "Assets"</param>
        /// <param name="onProgress">进度回调</param>
        public void Build(string rootPath = "Assets", Action<int, int> onProgress = null)
        {
            m_Dependencies.Clear();
            m_Referencers.Clear();

            string[] allGuids = AssetDatabase.FindAssets("t:Object", new[] { rootPath });
            var assetPaths = new List<string>();

            foreach (string guid in allGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    assetPaths.Add(path);
                }
            }

            int total = assetPaths.Count;
            for (int i = 0; i < total; i++)
            {
                onProgress?.Invoke(i + 1, total);
                string path = assetPaths[i];

                try
                {
                    string[] deps = AssetDatabase.GetDependencies(path, false);
                    var depSet = new HashSet<string>(deps, StringComparer.OrdinalIgnoreCase);
                    depSet.Remove(path); // 排除自身
                    m_Dependencies[path] = depSet;

                    foreach (string dep in depSet)
                    {
                        if (!m_Referencers.ContainsKey(dep))
                        {
                            m_Referencers[dep] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }

                        m_Referencers[dep].Add(path);
                    }
                }
                catch
                {
                    // 跳过无法分析依赖的资源
                }
            }

            m_Built = true;
        }

        /// <summary>
        /// 图是否已构建。
        /// </summary>
        public bool IsBuilt => m_Built;

        /// <summary>
        /// 获取图统计信息。
        /// </summary>
        /// <returns>统计信息字符串</returns>
        public string GetStats()
        {
            int totalAssets = m_Dependencies.Count;
            int totalEdges = 0;
            int orphanCount = 0;

            foreach (var kvp in m_Dependencies)
            {
                totalEdges += kvp.Value.Count;
                if (kvp.Value.Count == 0
                    && (!m_Referencers.ContainsKey(kvp.Key)
                        || m_Referencers[kvp.Key].Count == 0))
                {
                    orphanCount++;
                }
            }

            return $"Assets: {totalAssets}, Edges: {totalEdges}, Orphans: {orphanCount}";
        }

        private void CountReferencers(string assetPath, HashSet<string> visited)
        {
            if (!visited.Add(assetPath))
            {
                return;
            }

            if (m_Referencers.TryGetValue(assetPath, out var directReferencers))
            {
                foreach (string referencer in directReferencers)
                {
                    CountReferencers(referencer, visited);
                }
            }
        }
    }
}
