#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HN.Framework.Core.Capability.Localization;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 按语言加载资源的工具类。使用路径拼接模式定位本地化资源。
    /// 路径规则：{basePath}/{locale.Code}/{assetName}
    /// 例如：Assets/Localization/Textures/zh-CN/icon_health.png
    /// </summary>
    public static class AssetLocalizer
    {
        /// <summary>
        /// 异步加载指定语言的本地化资源（基于回调）。
        /// </summary>
        /// <typeparam name="T">资源类型（如 Texture2D、Sprite、GameObject）。</typeparam>
        /// <param name="locale">目标语言区域。</param>
        /// <param name="basePath">基础资源路径。</param>
        /// <param name="assetName">资源名称（含扩展名）。</param>
        /// <param name="onLoaded">加载完成回调。成功时传入加载的资源，失败时传入 null。</param>
        /// <exception cref="ArgumentException">当 locale.Code 为 null 时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 basePath 或 assetName 为 null 时抛出。</exception>
        public static void LoadLocalizedAssetAsync<T>(
            Locale locale,
            string basePath,
            string assetName,
            Action<T?> onLoaded) where T : UnityEngine.Object
        {
            ThrowIfLocaleCodeNull(locale);

            var localizedPath = GetLocalizedAssetPath(basePath, locale, assetName);
            var handle = Addressables.LoadAssetAsync<T>(localizedPath);
            handle.Completed += completedHandle =>
            {
                onLoaded?.Invoke(
                    completedHandle.Status == AsyncOperationStatus.Succeeded
                        ? completedHandle.Result
                        : null);
            };
        }

        /// <summary>
        /// 以协程方式加载指定语言的本地化资源。
        /// </summary>
        /// <typeparam name="T">资源类型（如 Texture2D、Sprite、GameObject）。</typeparam>
        /// <param name="locale">目标语言区域。</param>
        /// <param name="basePath">基础资源路径。</param>
        /// <param name="assetName">资源名称（含扩展名）。</param>
        /// <param name="onLoaded">加载完成回调。成功时传入加载的资源，失败时传入 null。</param>
        /// <returns>用于协程挂起的 IEnumerator。</returns>
        /// <exception cref="ArgumentException">当 locale.Code 为 null 时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 basePath 或 assetName 为 null 时抛出。</exception>
        public static IEnumerator LoadLocalizedAssetCoroutine<T>(
            Locale locale,
            string basePath,
            string assetName,
            Action<T?> onLoaded) where T : UnityEngine.Object
        {
            ThrowIfLocaleCodeNull(locale);

            var localizedPath = GetLocalizedAssetPath(basePath, locale, assetName);
            var handle = Addressables.LoadAssetAsync<T>(localizedPath);
            yield return handle;
            onLoaded?.Invoke(
                handle.Status == AsyncOperationStatus.Succeeded
                    ? handle.Result
                    : null);
        }

        /// <summary>
        /// 获取资源在不同语言下的所有可能路径（用于预加载检查）。
        /// </summary>
        /// <param name="basePath">基础路径。</param>
        /// <param name="assetName">资源名称。</param>
        /// <param name="locales">所有目标语言区域。</param>
        /// <returns>所有语言的完整资源路径数组。</returns>
        /// <exception cref="ArgumentNullException">当 locales 为 null 时抛出。</exception>
        public static string[] GetAllLocalizedPaths(
            string basePath, string assetName, IEnumerable<Locale> locales)
        {
            if (locales == null)
            {
                throw new ArgumentNullException(nameof(locales));
            }

            return locales.Select(l => GetLocalizedAssetPath(basePath, l, assetName)).ToArray();
        }

        /// <summary>
        /// 获取本地化资源在指定语言下的完整路径。
        /// 路径规则：{basePath}/{locale.Code}/{assetName}
        /// </summary>
        /// <param name="basePath">基础路径。</param>
        /// <param name="locale">语言区域。</param>
        /// <param name="assetName">资源名称。</param>
        /// <returns>拼接后的完整路径。</returns>
        /// <exception cref="ArgumentException">当 locale.Code 为 null 时抛出。</exception>
        /// <exception cref="ArgumentNullException">当 basePath 或 assetName 为 null 时抛出。</exception>
        public static string GetLocalizedAssetPath(
            string basePath, Locale locale, string assetName)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException(nameof(basePath));
            }

            ThrowIfLocaleCodeNull(locale);

            if (assetName == null)
            {
                throw new ArgumentNullException(nameof(assetName));
            }

            return $"{basePath}/{locale.Code}/{assetName}";
        }

        /// <summary>
        /// 校验 locale.Code 不为 null。default(Locale) 的 Code 为 null，代表未初始化。
        /// </summary>
        private static void ThrowIfLocaleCodeNull(Locale locale)
        {
            if (locale.Code == null)
            {
                throw new ArgumentException("Locale.Code cannot be null. The Locale may be uninitialized (default).", nameof(locale));
            }
        }
    }
}
