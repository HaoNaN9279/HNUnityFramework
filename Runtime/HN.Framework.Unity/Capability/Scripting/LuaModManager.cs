#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using HN.Framework.Core.Capability.Scripting;
using XLua;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Scripting
{
    /// <summary>
    /// xLua Mod 脚本管理器，实现 IScriptEngine 接口，提供沙箱隔离和 API 白名单机制。
    /// 每个 Mod 拥有独立的 LuaEnv 实例，确保脚本执行环境相互隔离。
    /// 由 GameWorld 作为服务注入，不采用静态单例模式。
    /// </summary>
    public sealed class LuaModManager : IScriptEngine
    {
        /// <summary>
        /// 跟踪单个已加载 Mod 的状态、Lua 环境和配置。
        /// 实现 IScriptMod 接口以支持统一的 Mod 生命周期管理。
        /// </summary>
        private sealed class LuaModInstance : IScriptMod
        {
            /// <inheritdoc/>
            public string ModId { get; }

            /// <inheritdoc/>
            public string ModName { get; }

            /// <inheritdoc/>
            public ModState State { get; set; }

            /// <summary>
            /// 该 Mod 独占的 Lua 虚拟机实例，实现沙箱隔离。
            /// </summary>
            public LuaEnv? LuaEnv { get; set; }

            /// <summary>
            /// Mod 的配置数据。
            /// </summary>
            public ScriptModConfig Config { get; }

            /// <summary>
            /// 初始化 Mod 实例为未加载状态。
            /// </summary>
            /// <param name="config">Mod 配置数据。</param>
            /// <exception cref="ArgumentNullException">当 config 为 null 时抛出。</exception>
            public LuaModInstance(ScriptModConfig config)
            {
                Config = config ?? throw new ArgumentNullException(nameof(config));
                ModId = config.ModId;
                ModName = config.ModName ?? config.ModId;
                State = ModState.NotLoaded;
            }

            /// <inheritdoc/>
            public void OnLoad()
            {
                State = ModState.Loaded;
            }

            /// <inheritdoc/>
            public void OnEnable()
            {
                State = ModState.Enabled;
            }

            /// <inheritdoc/>
            public void OnDisable()
            {
                State = ModState.Disabled;
            }

            /// <inheritdoc/>
            public void OnUnload()
            {
                DisposeLuaEnv();
            }

            /// <summary>
            /// 安全释放 LuaEnv 实例并将状态重置为未加载。
            /// </summary>
            public void DisposeLuaEnv()
            {
                try
                {
                    LuaEnv?.Dispose();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[LuaModManager] Error disposing LuaEnv for Mod '{ModId}': {e.Message}");
                }
                finally
                {
                    LuaEnv = null;
                    State = ModState.NotLoaded;
                }
            }
        }

        private readonly Dictionary<string, LuaModInstance> m_Mods = new Dictionary<string, LuaModInstance>();
        private readonly HashSet<string> m_Whitelist = new HashSet<string>();
        private readonly string m_ModBasePath;
        private LuaEnv? m_DefaultEnv;
        private bool m_Disposed;

        /// <summary>
        /// 获取当前已加载的 Mod 数量。
        /// </summary>
        public int ModCount => m_Mods.Count;

        /// <summary>
        /// 使用默认路径（Application.streamingAssetsPath/Mods）初始化 LuaModManager
        /// 并注册默认 API 白名单。
        /// </summary>
        public LuaModManager()
        {
            m_ModBasePath = Path.Combine(Application.streamingAssetsPath, "Mods");
            RegisterDefaultWhitelist();
        }

        /// <summary>
        /// 使用自定义 Mod 基础路径初始化 LuaModManager 并注册默认 API 白名单。
        /// </summary>
        /// <param name="modBasePath">Mod 脚本存放的基础路径。若为 null，则使用默认路径。</param>
        public LuaModManager(string? modBasePath)
        {
            m_ModBasePath = modBasePath ?? Path.Combine(Application.streamingAssetsPath, "Mods");
            RegisterDefaultWhitelist();
        }

        // ──────────────────────────────────────────────────
        //  API 白名单
        // ──────────────────────────────────────────────────

        /// <summary>
        /// 注册一个允许 Mod 脚本访问的 API 类型。
        /// 只有白名单中的类型才能通过 RegisterGlobal 注册到 Lua 环境。
        /// </summary>
        /// <param name="fullTypeName">类型的完整限定名（包含命名空间），例如 "UnityEngine.Debug"。</param>
        public void RegisterApi(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return;

            m_Whitelist.Add(fullTypeName);
            UnityEngine.Debug.Log($"[LuaModManager] API whitelist: added '{fullTypeName}'.");
        }

        /// <summary>
        /// 注册默认的 API 白名单类型（UnityEngine 常用类）。
        /// </summary>
        private void RegisterDefaultWhitelist()
        {
            m_Whitelist.Add("UnityEngine.Debug");
            m_Whitelist.Add("UnityEngine.Time");
            m_Whitelist.Add("UnityEngine.Mathf");
            m_Whitelist.Add("UnityEngine.Vector2");
            m_Whitelist.Add("UnityEngine.Vector3");
            m_Whitelist.Add("UnityEngine.Color");
            m_Whitelist.Add("UnityEngine.Quaternion");
        }

        // ──────────────────────────────────────────────────
        //  Mod 生命周期
        // ──────────────────────────────────────────────────

        /// <summary>
        /// 加载 Mod 配置中指定的 Lua 脚本并创建独立的 LuaEnv 沙箱。
        /// 若指定 Mod 已被加载，先卸载旧实例再重新加载。
        /// </summary>
        /// <param name="config">Mod 配置数据。</param>
        /// <returns>已加载的 Mod 实例；失败时返回 null。</returns>
        /// <exception cref="ArgumentNullException">当 config 为 null 时抛出。</exception>
        public IScriptMod? LoadMod(ScriptModConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (string.IsNullOrEmpty(config.ModId))
            {
                UnityEngine.Debug.LogWarning("[LuaModManager] LoadMod: ModId is null or empty.");
                return null;
            }

            if (m_Mods.ContainsKey(config.ModId))
            {
                UnityEngine.Debug.LogWarning($"[LuaModManager] Mod '{config.ModId}' is already loaded. Unloading it first.");
                UnloadMod(config.ModId);
            }

            var instance = new LuaModInstance(config);

            try
            {
                instance.LuaEnv = new LuaEnv();
                ConfigureSandbox(instance.LuaEnv);

                // 执行入口脚本
                if (config.ScriptPaths != null && config.ScriptPaths.Length > 0)
                {
                    string modDir = Path.Combine(m_ModBasePath, config.ModId);

                    foreach (var scriptPath in config.ScriptPaths)
                    {
                        string fullPath = Path.Combine(modDir, scriptPath);
                        string? luaCode = ReadScriptFile(config.ModId, fullPath);
                        if (luaCode != null)
                        {
                            instance.LuaEnv.DoString(luaCode, fullPath);
                        }
                    }
                }

                m_Mods[config.ModId] = instance;
                instance.OnLoad();
                UnityEngine.Debug.Log($"[LuaModManager] Mod '{config.ModName ?? config.ModId}' loaded successfully.");
                return instance;
            }
            catch (Exception e)
            {
                instance.State = ModState.Error;
                UnityEngine.Debug.LogError($"[LuaModManager] Failed to load Mod '{config.ModId}': {e.Message}");

                // 清理失败的 LuaEnv
                try { instance.LuaEnv?.Dispose(); }
                catch { /* 忽略二次错误 */ }

                instance.LuaEnv = null;
                return null;
            }
        }

        /// <summary>
        /// 启用指定 Mod，使其进入运行状态。
        /// </summary>
        /// <param name="modId">Mod 的唯一标识符。</param>
        public void EnableMod(string modId)
        {
            if (!TryGetMod(modId, out var instance))
                return;

            if (instance.State == ModState.Enabled)
                return;

            try
            {
                instance.OnEnable();
                UnityEngine.Debug.Log($"[LuaModManager] Mod '{modId}' enabled.");
            }
            catch (Exception e)
            {
                instance.State = ModState.Error;
                UnityEngine.Debug.LogError($"[LuaModManager] Failed to enable Mod '{modId}': {e.Message}");
            }
        }

        /// <summary>
        /// 禁用指定 Mod，暂停运行逻辑但不释放 LuaEnv 资源。
        /// </summary>
        /// <param name="modId">Mod 的唯一标识符。</param>
        public void DisableMod(string modId)
        {
            if (!TryGetMod(modId, out var instance))
                return;

            if (instance.State != ModState.Enabled)
                return;

            try
            {
                instance.OnDisable();
                UnityEngine.Debug.Log($"[LuaModManager] Mod '{modId}' disabled.");
            }
            catch (Exception e)
            {
                instance.State = ModState.Error;
                UnityEngine.Debug.LogError($"[LuaModManager] Failed to disable Mod '{modId}': {e.Message}");
            }
        }

        /// <summary>
        /// 卸载指定 Mod，释放 LuaEnv 沙箱并从管理器中移除。
        /// </summary>
        /// <param name="modId">Mod 的唯一标识符。</param>
        public void UnloadMod(string modId)
        {
            if (!m_Mods.TryGetValue(modId, out var instance))
                return;

            try
            {
                instance.OnUnload();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[LuaModManager] Error during unload of Mod '{modId}': {e.Message}");
            }

            m_Mods.Remove(modId);
            UnityEngine.Debug.Log($"[LuaModManager] Mod '{modId}' unloaded.");
        }

        // ──────────────────────────────────────────────────
        //  查询
        // ──────────────────────────────────────────────────

        /// <summary>
        /// 获取已加载的 Mod 实例。
        /// </summary>
        /// <param name="modId">Mod 的唯一标识符。</param>
        /// <returns>Mod 实例；未找到返回 null。</returns>
        public IScriptMod? GetMod(string modId)
        {
            m_Mods.TryGetValue(modId, out var instance);
            return instance;
        }

        /// <summary>
        /// 获取所有已加载的 Mod 标识符列表。
        /// </summary>
        /// <returns>Mod 标识符数组。</returns>
        public string[] GetLoadedModIds()
        {
            var ids = new string[m_Mods.Count];
            m_Mods.Keys.CopyTo(ids, 0);
            return ids;
        }

        // ──────────────────────────────────────────────────
        //  IScriptEngine 实现
        // ──────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Execute(string code, string chunkName = "inline")
        {
            if (string.IsNullOrEmpty(code))
                return;

            if (m_Disposed)
            {
                UnityEngine.Debug.LogError("[LuaModManager] Execute: Manager is already disposed.");
                return;
            }

            try
            {
                var env = GetActiveLuaEnv();
                env.DoString(code, chunkName);
            }
            catch (Exception e)
            {
                // 标记当前活跃 Mod 为错误状态
                var activeMod = FindLastEnabledMod();
                if (activeMod != null)
                {
                    activeMod.State = ModState.Error;
                }

                UnityEngine.Debug.LogWarning($"[LuaModManager] Execute failed (chunk: '{chunkName}'): {e.Message}");
            }
        }

        /// <inheritdoc/>
        public void RegisterGlobal(string name, object obj)
        {
            if (string.IsNullOrEmpty(name))
            {
                UnityEngine.Debug.LogError("[LuaModManager] RegisterGlobal: name is null or empty.");
                return;
            }

            if (obj == null)
            {
                UnityEngine.Debug.LogError("[LuaModManager] RegisterGlobal: obj is null.");
                return;
            }

            if (m_Disposed)
            {
                UnityEngine.Debug.LogError("[LuaModManager] RegisterGlobal: Manager is already disposed.");
                return;
            }

            // 白名单检查
            var typeName = obj.GetType().FullName;
            if (typeName != null && !m_Whitelist.Contains(typeName))
            {
                UnityEngine.Debug.LogError(
                    $"[LuaModManager] RegisterGlobal: Type '{typeName}' is not in the API whitelist. " +
                    $"Use RegisterApi(\"{typeName}\") to add it.");
                return;
            }

            try
            {
                var env = GetActiveLuaEnv();
                env.Global.Set(name, obj);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[LuaModManager] RegisterGlobal failed for '{name}': {e.Message}");
            }
        }

        /// <inheritdoc/>
        public object? CallFunction(string moduleName, string funcName, params object[] args)
        {
            if (m_Disposed)
            {
                UnityEngine.Debug.LogError("[LuaModManager] CallFunction: Manager is already disposed.");
                return null;
            }

            try
            {
                var env = GetActiveLuaEnv();

                if (!string.IsNullOrEmpty(moduleName))
                {
                    using (var table = env.Global.Get<XLua.LuaTable>(moduleName))
                    {
                        if (table == null)
                        {
                            UnityEngine.Debug.LogError(
                                $"[LuaModManager] CallFunction: Module '{moduleName}' not found.");
                            return null;
                        }

                        using (var func = table.Get<XLua.LuaFunction>(funcName))
                        {
                            if (func == null)
                            {
                                UnityEngine.Debug.LogError(
                                    $"[LuaModManager] CallFunction: Function '{funcName}' not found in module '{moduleName}'.");
                                return null;
                            }

                            var results = func.Call(args);
                            return (results != null && results.Length > 0) ? results[0] : null;
                        }
                    }
                }
                else
                {
                    using (var func = env.Global.Get<XLua.LuaFunction>(funcName))
                    {
                        if (func == null)
                        {
                            UnityEngine.Debug.LogWarning(
                                $"[LuaModManager] CallFunction: Global function '{funcName}' not found.");
                            return null;
                        }

                        var results = func.Call(args);
                        return (results != null && results.Length > 0) ? results[0] : null;
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"[LuaModManager] CallFunction failed ('{funcName}'): {e.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;

            // 卸载所有 Mod（UnloadMod 内部处理异常），
            // 拷贝键集合以安全迭代（UnloadMod 会修改 m_Mods）
            var modIds = GetLoadedModIds();
            foreach (var modId in modIds)
            {
                try
                {
                    UnloadMod(modId);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(
                        $"[LuaModManager] Error unloading Mod '{modId}' during dispose: {e.Message}");
                }
            }

            // 释放默认 LuaEnv
            try
            {
                m_DefaultEnv?.Dispose();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"[LuaModManager] Error disposing default LuaEnv: {e.Message}");
            }
            finally
            {
                m_DefaultEnv = null;
            }

            m_Mods.Clear();
            m_Whitelist.Clear();

            UnityEngine.Debug.Log("[LuaModManager] Disposed.");
        }

        // ──────────────────────────────────────────────────
        //  内部辅助方法
        // ──────────────────────────────────────────────────

        /// <summary>
        /// 获取当前活跃的 LuaEnv。
        /// 优先使用最后一个处于 Enabled 状态的 Mod 的 LuaEnv；
        /// 若没有活跃 Mod，则创建或复用默认临时 LuaEnv。
        /// </summary>
        private LuaEnv GetActiveLuaEnv()
        {
            var lastEnabled = FindLastEnabledMod();
            if (lastEnabled?.LuaEnv != null)
                return lastEnabled.LuaEnv;

            if (m_DefaultEnv == null)
            {
                m_DefaultEnv = new LuaEnv();
                ConfigureSandbox(m_DefaultEnv);
            }

            return m_DefaultEnv;
        }

        /// <summary>
        /// 查找最后一个状态为 Enabled 的 Mod 实例。
        /// </summary>
        /// <returns>最后一个已启用的 Mod 实例；没有时返回 null。</returns>
        private LuaModInstance? FindLastEnabledMod()
        {
            LuaModInstance? result = null;
            foreach (var kvp in m_Mods)
            {
                if (kvp.Value.State == ModState.Enabled)
                {
                    result = kvp.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// 配置 LuaEnv 的安全沙箱限制。
        /// 禁用 os.execute、io.popen 等危险 API 以防止恶意脚本。
        /// </summary>
        /// <param name="luaEnv">要配置的 LuaEnv 实例。</param>
        private static void ConfigureSandbox(LuaEnv luaEnv)
        {
            try
            {
                luaEnv.DoString(@"
                    if os then
                        os.execute = nil
                        os.exit = nil
                        os.remove = nil
                        os.rename = nil
                        os.tmpname = nil
                    end
                    if io then
                        io.popen = nil
                    end
                ", "sandbox_init");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning(
                    $"[LuaModManager] Failed to configure sandbox for LuaEnv: {e.Message}");
            }
        }

        /// <summary>
        /// 读取指定路径的 Lua 脚本文件内容。
        /// </summary>
        /// <param name="modId">Mod 标识符（用于日志）。</param>
        /// <param name="filePath">文件绝对路径。</param>
        /// <returns>脚本内容；读取失败返回 null。</returns>
        private static string? ReadScriptFile(string modId, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    UnityEngine.Debug.LogError(
                        $"[LuaModManager] Script file not found for Mod '{modId}': {filePath}");
                    return null;
                }

                return File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"[LuaModManager] Failed to read script file '{filePath}' for Mod '{modId}': {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 尝试从字典中获取已加载的 Mod 实例。
        /// </summary>
        /// <param name="modId">Mod 标识符。</param>
        /// <param name="instance">输出的 Mod 实例。</param>
        /// <returns>找到返回 true，否则返回 false。</returns>
        private bool TryGetMod(string modId, out LuaModInstance instance)
        {
            if (!m_Mods.TryGetValue(modId, out instance))
            {
                UnityEngine.Debug.LogWarning($"[LuaModManager] Mod '{modId}' not found.");
                return false;
            }
            return true;
        }
    }
}
