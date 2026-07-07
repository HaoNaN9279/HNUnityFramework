#nullable enable

using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Capability.Input
{
    /// <summary>
    /// InputActionAsset 加载器，提供从 JSON 字符串或 Addressables 地址加载 .inputactions 资产的辅助方法。
    /// </summary>
    public static class InputActionAssetLoader
    {
        /// <summary>
        /// 从 JSON 字符串创建 <see cref="InputActionAsset"/>。
        /// </summary>
        /// <param name="json">符合 .inputactions 格式的 JSON 字符串。</param>
        /// <returns>解析后生成的 <see cref="InputActionAsset"/> 实例。</returns>
        /// <exception cref="ArgumentNullException">当 json 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">当 json 为空字符串或格式无效时抛出。</exception>
        /// <remarks>
        /// 内部使用 <see cref="InputActionAsset.FromJson"/> 进行解析。
        /// 调用方负责在不再需要时调用 <see cref="UnloadAsset"/> 释放资源。
        /// </remarks>
        public static InputActionAsset LoadFromJson(string json)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON cannot be empty or whitespace.", nameof(json));

            try
            {
                var asset = InputActionAsset.FromJson(json);
                if (asset == null)
                    throw new ArgumentException("Failed to parse JSON: InputActionAsset.FromJson returned null.", nameof(json));
                return asset;
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new ArgumentException($"Failed to parse input action JSON: {ex.Message}", nameof(json), ex);
            }
        }

        /// <summary>
        /// 通过 Addressables 地址加载 <see cref="InputActionAsset"/>。
        /// 当前为占位实现，因 Addressables 包的设置和依赖在测试环境中无法保证可用。
        /// </summary>
        /// <param name="address">Addressables 资产地址。</param>
        /// <returns>加载的 <see cref="InputActionAsset"/> 实例；当前占位实现返回 null。</returns>
        /// <remarks>
        /// 此方法为占位实现，仅输出警告日志。待 Addressables 集成完成后将替换为实际加载逻辑。
        /// </remarks>
        public static InputActionAsset? LoadFromAsset(string address)
        {
            Debug.LogWarning($"[InputActionAssetLoader] LoadFromAsset('{address}'): Addressables loading is not yet implemented. This is a placeholder.");
            return null;
        }

        /// <summary>
        /// 释放 <see cref="InputActionAsset"/> 占用的资源。
        /// </summary>
        /// <param name="asset">要释放的 <see cref="InputActionAsset"/> 实例。若为 null 则为安全空操作。</param>
        public static void UnloadAsset(InputActionAsset? asset)
        {
            asset?.Dispose();
        }
    }
}
