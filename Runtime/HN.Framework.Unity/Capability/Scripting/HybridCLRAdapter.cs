using System;
using System.Collections.Generic;
using System.Reflection;
using HN.Framework.Core.Capability.Scripting;
using HN.Framework.Core.Driver;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HN.Framework.Unity.Capability.Scripting
{
    /// <summary>
    /// HybridCLR 运行时适配器，负责加载热更新 DLL、注册 AOT 补充元数据，
    /// 并与 GameWorld 生命周期集成。通过反射调用 HybridCLR RuntimeApi 以避免硬依赖。
    /// </summary>
    public sealed class HybridCLRAdapter
    {
        private bool m_Initialized;
        private GameWorld m_GameWorld;

        /// <summary>
        /// 缓存的 HybridCLR 构建配置值。由模块初始化时设置，未设置时使用默认值。
        /// </summary>
        internal static string? s_AotMetadataLabelOverride;
        internal static string? s_HotUpdateKeyOverride;

        private static readonly string s_RuntimeApiTypeName = "HybridCLR.RuntimeApi, HybridCLR.Runtime";

        /// <summary>
        /// AOT 元数据 Addressables 标签。
        /// 优先读取配置注入值，回退默认值 "AOTMetadata"。
        /// </summary>
        private static string AotMetadataLabel =>
            s_AotMetadataLabelOverride ?? "AOTMetadata";

        private const int HomologousImageModeSuperSet = 1;

        /// <summary>
        /// 当前绑定的 GameWorld 实例。用于在 InitializeHotUpdate 时传递给热更新入口。
        /// </summary>
        public GameWorld World
        {
            get => m_GameWorld;
            set => m_GameWorld = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// 是否已完成初始化（AOT 元数据已注册）。
        /// </summary>
        public bool IsInitialized => m_Initialized;

        /// <summary>
        /// 创建 HybridCLRAdapter 实例并绑定到指定的 GameWorld。
        /// </summary>
        /// <param name="world">框架的 GameWorld 实例。</param>
        /// <exception cref="ArgumentNullException">当 world 为 null 时抛出。</exception>
        public HybridCLRAdapter(GameWorld world)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
        }

        /// <summary>
        /// 通过 Addressables 同步加载热更新 DLL 字节码并加载为 Assembly。
        /// </summary>
        /// <param name="addressablesKey">DLL 文件在 Addressables 中的键（通常为 .bytes 资源）。</param>
        /// <returns>加载的 Assembly 实例；失败时返回 null。</returns>
        public Assembly LoadHotUpdateAssembly(string addressablesKey)
        {
            if (string.IsNullOrEmpty(addressablesKey))
            {
                UnityEngine.Debug.LogError("[HybridCLRAdapter] LoadHotUpdateAssembly: addressablesKey is null or empty.");
                return null;
            }

            try
            {
                var handle = Addressables.LoadAssetAsync<TextAsset>(addressablesKey);
                handle.WaitForCompletion();

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    UnityEngine.Debug.LogError($"[HybridCLRAdapter] Failed to load hot-update DLL via Addressables key '{addressablesKey}'.");
                    return null;
                }

                var textAsset = handle.Result;
                var assembly = Assembly.Load(textAsset.bytes);
                UnityEngine.Debug.Log($"[HybridCLRAdapter] Loaded hot-update assembly: {assembly.FullName}");
                return assembly;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] Exception loading hot-update assembly '{addressablesKey}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 注册 AOT 补充元数据。使用反射调用 HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly。
        /// </summary>
        /// <param name="metadata">元数据字节数组（来自 .bytes 文件的 dllBytes）。</param>
        /// <returns>注册成功返回 true；失败返回 false。</returns>
        public bool RegisterAOTMetadata(byte[] metadata)
        {
            if (metadata == null || metadata.Length == 0)
            {
                UnityEngine.Debug.LogError("[HybridCLRAdapter] RegisterAOTMetadata: metadata is null or empty.");
                return false;
            }

            try
            {
                var runtimeApiType = Type.GetType(s_RuntimeApiTypeName);
                if (runtimeApiType == null)
                {
                    UnityEngine.Debug.LogError("[HybridCLRAdapter] HybridCLR.RuntimeApi type not found. Is the HybridCLR package installed?");
                    return false;
                }

                // 灵活查找 LoadMetadataForAOTAssembly 方法（避免依赖具体枚举类型签名）
                MethodInfo targetMethod = null;
                foreach (var method in runtimeApiType.GetMethods(BindingFlags.Static | BindingFlags.Public))
                {
                    if (method.Name == "LoadMetadataForAOTAssembly" && method.GetParameters().Length == 2)
                    {
                        targetMethod = method;
                        break;
                    }
                }

                if (targetMethod == null)
                {
                    UnityEngine.Debug.LogError("[HybridCLRAdapter] LoadMetadataForAOTAssembly method not found on HybridCLR.RuntimeApi.");
                    return false;
                }

                // 构造 HomologousImageMode 枚举值（1 = SuperSet）
                var modeParamType = targetMethod.GetParameters()[1].ParameterType;
                var modeValue = Enum.ToObject(modeParamType, HomologousImageModeSuperSet);

                var result = targetMethod.Invoke(null, new object[] { metadata, modeValue });
                var resultCode = Convert.ToInt32(result);

                const int successCode = 0;
                if (resultCode != successCode)
                {
                    UnityEngine.Debug.LogError($"[HybridCLRAdapter] LoadMetadataForAOTAssembly returned error code: {resultCode}");
                    return false;
                }

                UnityEngine.Debug.Log("[HybridCLRAdapter] AOT metadata registered successfully.");
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] Failed to register AOT metadata: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 初始化 HybridCLRAdapter，从 Addressables 的 "AOTMetadata" 标签组加载所有 AOT 补充元数据。
        /// 若已初始化则跳过。
        /// </summary>
        public void Initialize()
        {
            if (m_Initialized)
            {
                UnityEngine.Debug.Log("[HybridCLRAdapter] Already initialized, skipping.");
                return;
            }

            try
            {
                var handle = Addressables.LoadAssetsAsync<TextAsset>(AotMetadataLabel, null);
                handle.WaitForCompletion();

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    int count = 0;
                    foreach (var asset in handle.Result)
                    {
                        if (asset != null)
                        {
                            RegisterAOTMetadata(asset.bytes);
                            count++;
                        }
                    }

                    UnityEngine.Debug.Log($"[HybridCLRAdapter] Loaded {count} AOT metadata asset(s) from label '{AotMetadataLabel}'.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[HybridCLRAdapter] No AOT metadata assets found with label '{AotMetadataLabel}'. AOT metadata registration skipped.");
                }

                m_Initialized = true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] Initialize failed: {e.Message}");
            }
        }

        /// <summary>
        /// 完整的热更新初始化流程：加载 AOT 元数据 → 加载热更新 DLL → 查找入口 → 调用注册。
        /// </summary>
        /// <returns>热更新入口实例；失败时返回 null。</returns>
        public IHotUpdateEntry InitializeHotUpdate()
        {
            // 1. 初始化 AOT 元数据
            if (!m_Initialized)
            {
                Initialize();
            }

            // 2. 加载热更新 DLL
            var hotUpdateKey = s_HotUpdateKeyOverride ?? "HotUpdateDLL";
            var assembly = LoadHotUpdateAssembly(hotUpdateKey);
            if (assembly == null)
            {
                UnityEngine.Debug.LogError("[HybridCLRAdapter] InitializeHotUpdate: Failed to load hot-update assembly.");
                return null;
            }

            // 3. 查找实现了 IHotUpdateEntry 的类型
            var entryType = FindHotUpdateEntryType(assembly);
            if (entryType == null)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] InitializeHotUpdate: No IHotUpdateEntry implementation found in assembly '{assembly.FullName}'.");
                return null;
            }

            // 4. 实例化并调用入口方法
            try
            {
                var entry = (IHotUpdateEntry)Activator.CreateInstance(entryType);
                entry.OnRegisterGameModules(m_GameWorld);
                UnityEngine.Debug.Log($"[HybridCLRAdapter] Hot-update entry '{entryType.FullName}' registered game modules successfully.");
                return entry;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] InitializeHotUpdate: Failed to create/execute hot-update entry: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 在指定程序集中查找实现了 IHotUpdateEntry 接口且可实例化的类型。
        /// </summary>
        /// <param name="assembly">热更新 DLL 的程序集。</param>
        /// <returns>找到的类型；未找到或存在多个时返回 null 并记录错误。</returns>
        private static Type FindHotUpdateEntryType(Assembly assembly)
        {
            var entryTypes = new List<Type>();

            foreach (var type in assembly.GetTypes())
            {
                if (typeof(IHotUpdateEntry).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                {
                    entryTypes.Add(type);
                }
            }

            if (entryTypes.Count == 0)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] No concrete IHotUpdateEntry implementation found in assembly '{assembly.FullName}'.");
                return null;
            }

            if (entryTypes.Count > 1)
            {
                UnityEngine.Debug.LogError($"[HybridCLRAdapter] Multiple IHotUpdateEntry implementations found in assembly '{assembly.FullName}': [{string.Join(", ", entryTypes.ConvertAll(t => t.FullName))}]. Cannot determine which one to use.");
                return null;
            }

            return entryTypes[0];
        }
    }
}
