using System.IO;
using UnityEditor;

namespace HN.Framework.Editor.Sheet
{
    /// <summary>
    /// Luban CLI 的外部路径配置。
    /// 使用 EditorPrefs 持久化存储，用户可自行部署 Luban 后在此配置其路径。
    /// </summary>
    /// <remarks>
    /// Luban 不作为框架的内置依赖。用户需自行下载部署 Luban，
    /// 然后在 Sheet 编辑器面板中配置 Luban CLI 的路径。
    /// 未配置 Luban 时，Sheet 编辑器的基础功能（打开/编辑/保存 Excel）不受影响。
    /// </remarks>
    public static class LubanPathConfig
    {
        private const string EditorPrefsKey = "HN.Framework.LubanPath";

        /// <summary>
        /// 获取用户配置的 Luban CLI 路径。
        /// </summary>
        /// <returns>配置的路径，未配置时返回 <see cref="string.Empty"/>。</returns>
        public static string GetPath()
        {
            return EditorPrefs.GetString(EditorPrefsKey, string.Empty);
        }

        /// <summary>
        /// 设置 Luban CLI 路径并持久化存储。
        /// </summary>
        /// <param name="path">Luban CLI 的可执行文件或根目录路径。</param>
        public static void SetPath(string path)
        {
            EditorPrefs.SetString(EditorPrefsKey, path ?? string.Empty);
        }

        /// <summary>
        /// 清除已配置的 Luban CLI 路径。
        /// </summary>
        public static void ResetPath()
        {
            EditorPrefs.DeleteKey(EditorPrefsKey);
        }

        /// <summary>
        /// 检查当前配置的路径是否有效（路径非空且文件或目录存在）。
        /// </summary>
        /// <returns>路径有效返回 true；未配置或路径不存在返回 false。</returns>
        public static bool IsValid()
        {
            var path = GetPath();
            return !string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path));
        }
    }
}
