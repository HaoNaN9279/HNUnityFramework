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
        /// 通过 Addressables 地址加载 <see cref="InputActionAsset"/>。当前为占位实现。
        /// </summary>
        public static InputActionAsset? LoadFromAsset(string address)
        {
            UnityEngine.Debug.LogWarning($"[InputActionAssetLoader] LoadFromAsset('{address}'): Addressables loading is not yet implemented.");
            return null;
        }

        /// <summary>
        /// 释放 <see cref="InputActionAsset"/> 占用的资源。
        /// </summary>
        public static void UnloadAsset(InputActionAsset? asset)
        {
            // InputActionAsset is not IDisposable; null-safe no-op
        }
    }
}
