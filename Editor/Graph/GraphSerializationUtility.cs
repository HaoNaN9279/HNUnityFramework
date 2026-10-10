using System.IO;
using HN.Framework.Core.Level.Logic.Graph;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 图数据序列化工具：MemoryPack 二进制资产（.asset）保存/加载，以及 JSON 导入/导出。
    /// </summary>
    public static class GraphSerializationUtility
    {
        /// <summary>
        /// 将图数据保存为 <see cref="GraphAsset"/>。目标资产不存在时创建，存在时覆盖。
        /// </summary>
        /// <param name="graph">图数据。</param>
        /// <param name="assetPath">资产路径（以 Assets 开头）。</param>
        /// <returns>保存后的图资产；参数无效时返回 <c>null</c>。</returns>
        public static GraphAsset SaveAsset(GraphData graph, string assetPath)
        {
            if (graph == null || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            var asset = AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);
            if (asset == null)
            {
                EnsureFolder(NormalizeFolder(Path.GetDirectoryName(assetPath)));
                asset = ScriptableObject.CreateInstance<GraphAsset>();
                asset.SetGraph(graph);
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            else
            {
                asset.SetGraph(graph);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            return asset;
        }

        /// <summary>
        /// 加载图资产。
        /// </summary>
        /// <param name="assetPath">资产路径。</param>
        /// <returns>图资产；不存在时返回 <c>null</c>。</returns>
        public static GraphAsset LoadAsset(string assetPath)
        {
            return string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<GraphAsset>(assetPath);
        }

        /// <summary>
        /// 将图数据导出为 JSON 文件。
        /// </summary>
        /// <param name="graph">图数据。</param>
        /// <param name="filePath">输出文件路径。</param>
        /// <returns>导出成功返回 <c>true</c>。</returns>
        public static bool ExportJson(GraphData graph, string filePath)
        {
            if (graph == null || string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            File.WriteAllText(filePath, JsonConvert.SerializeObject(graph, Formatting.Indented));
            return true;
        }

        /// <summary>
        /// 从 JSON 文件导入图数据。
        /// </summary>
        /// <param name="filePath">JSON 文件路径。</param>
        /// <returns>导入的图数据；失败时返回 <c>null</c>。</returns>
        public static GraphData ImportJson(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<GraphData>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// 校验图数据一致性（边端点 / 端口引用 / 编组引用 / Id 唯一性）。
        /// </summary>
        /// <param name="graph">图数据。</param>
        /// <param name="error">校验失败时的错误说明。</param>
        /// <returns>通过返回 <c>true</c>。</returns>
        public static bool Validate(GraphData graph, out string error)
        {
            if (graph == null)
            {
                error = "Graph is null.";
                return false;
            }

            return graph.Validate(out error);
        }

        private static string NormalizeFolder(string folder)
        {
            return string.IsNullOrEmpty(folder) ? folder : folder.Replace('\\', '/');
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = NormalizeFolder(Path.GetDirectoryName(folder));
            var name = Path.GetFileName(folder);
            EnsureFolder(parent);

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name) && AssetDatabase.IsValidFolder(parent))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
