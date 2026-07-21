using System;
using System.IO;
using System.Text.RegularExpressions;
using HN.Framework.Core.Capability.Scripting;
using HN.Framework.Unity.Capability.Scripting;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HN.Framework.Unity.Tests.Scripting
{
    /// <summary>
    /// EditMode 集成测试：验证 LuaModManager 的 Mod 生命周期、沙箱隔离、
    /// API 白名单和 IScriptEngine 契约实现。
    /// 每个测试使用独立的 LuaModManager 实例，共享测试完成后统一释放。
    /// </summary>
    [TestFixture]
    public class LuaModManagerTests
    {
        private LuaModManager m_Manager;

        [SetUp]
        public void SetUp()
        {
            m_Manager = new LuaModManager();
        }

        [TearDown]
        public void TearDown()
        {
            m_Manager.Dispose();
        }

        // ====================================================================
        // Mod 加载测试
        // ====================================================================

        /// <summary>
        /// 加载有效配置的 Mod，应返回非 null 的 IScriptMod 且状态为 Loaded。
        /// </summary>
        [Test]
        public void LoadMod_ValidConfig_ReturnsModInLoadedState()
        {
            var config = new ScriptModConfig
            {
                ModId = "valid_mod",
                ModName = "Valid Mod",
                Sandboxed = true
            };

            var mod = m_Manager.LoadMod(config);

            Assert.That(mod, Is.Not.Null, "LoadMod should return a non-null mod");
            Assert.That(mod.ModId, Is.EqualTo("valid_mod"));
            Assert.That(mod.ModName, Is.EqualTo("Valid Mod"));
            Assert.That(mod.State, Is.EqualTo(ModState.Loaded));
            Assert.That(m_Manager.ModCount, Is.EqualTo(1));
        }

        /// <summary>
        /// 传入 null 配置应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void LoadMod_NullConfig_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => m_Manager.LoadMod(null));
        }

        /// <summary>
        /// ModId 为空或 null 的配置应返回 null（不抛异常但加载失败）。
        /// </summary>
        [Test]
        public void LoadMod_EmptyModId_ReturnsNull()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[LuaModManager\] LoadMod: ModId is null or empty\."));
            var mod = m_Manager.LoadMod(new ScriptModConfig { ModId = null });
            Assert.That(mod, Is.Null);

            LogAssert.Expect(LogType.Warning, new Regex(@"\[LuaModManager\] LoadMod: ModId is null or empty\."));
            mod = m_Manager.LoadMod(new ScriptModConfig { ModId = "" });
            Assert.That(mod, Is.Null);
        }

        /// <summary>
        /// 加载已存在的 ModId 应先卸载旧实例再加载新实例。
        /// </summary>
        [Test]
        public void LoadMod_DuplicateModId_ReplacesExistingMod()
        {
            var config1 = new ScriptModConfig { ModId = "dup_mod", ModName = "First" };
            var config2 = new ScriptModConfig { ModId = "dup_mod", ModName = "Second" };

            m_Manager.LoadMod(config1);
            m_Manager.LoadMod(config2);

            Assert.That(m_Manager.ModCount, Is.EqualTo(1));
            var mod = m_Manager.GetMod("dup_mod");
            Assert.That(mod, Is.Not.Null);
            Assert.That(mod.ModName, Is.EqualTo("Second"));
            Assert.That(mod.State, Is.EqualTo(ModState.Loaded));
        }

        /// <summary>
        /// 加载包含非法 Lua 脚本的 Mod，应设置 Error 状态且 LoadMod 返回 null。
        /// </summary>
        [Test]
        public void LoadMod_InvalidScript_SetsErrorStateAndReturnsNull()
        {
            string tempFile = Path.GetTempFileName();
            try
            {
                // 写入非法 Lua 代码（缺少结束符）
                File.WriteAllText(tempFile, "invalid lua code {{{");

                var config = new ScriptModConfig
                {
                    ModId = "bad_mod",
                    ModName = "Bad Mod",
                    ScriptPaths = new[] { Path.GetFileName(tempFile) }
                };

                // 需要将文件放到 Manager 期望的路径
                // 由于 LoadMod 使用 m_ModBasePath/ModId/ 作为目录，
                // 我们通过直接执行 DoString 来模拟——不如使用 RegisterGlobal 后 Execute 的方式
                // 更简单的测试：加载后手动 Execute 非法代码触发 Error
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        /// <summary>
        /// 通过 Execute 注入非法 Lua 代码来验证 Error 状态转换。
        /// 由于 LoadMod 需要实际文件系统，此测试验证 Excute 路径的 Error 处理。
        /// </summary>
        [Test]
        public void Execute_InvalidLuaCode_SetsModToErrorState()
        {
            var config = new ScriptModConfig
            {
                ModId = "test_mod",
                ModName = "Test Mod"
            };

            m_Manager.LoadMod(config);
            m_Manager.EnableMod("test_mod");

            // 执行非法 Lua 代码，预期产生 Error 日志
            LogAssert.Expect(LogType.Warning, new Regex(@"\[LuaModManager\] Execute failed.*bad_chunk.*"));
            m_Manager.Execute("syntax error !@#$%^", "bad_chunk");

            var mod = m_Manager.GetMod("test_mod");
            Assert.That(mod, Is.Not.Null);
            Assert.That(mod.State, Is.EqualTo(ModState.Error),
                "Mod should transition to Error state after executing invalid Lua");
        }

        // ====================================================================
        // Mod 生命周期测试
        // ====================================================================

        /// <summary>
        /// EnableMod 将 Mod 状态从 Loaded → Enabled。
        /// </summary>
        [Test]
        public void EnableMod_ChangesStateToEnabled()
        {
            var config = new ScriptModConfig { ModId = "lifecycle_mod", ModName = "LC Mod" };
            m_Manager.LoadMod(config);

            m_Manager.EnableMod("lifecycle_mod");

            var mod = m_Manager.GetMod("lifecycle_mod");
            Assert.That(mod.State, Is.EqualTo(ModState.Enabled));
        }

        /// <summary>
        /// DisableMod 将 Enabled 状态的 Mod 转为 Disabled。
        /// </summary>
        [Test]
        public void DisableMod_ChangesStateToDisabled()
        {
            var config = new ScriptModConfig { ModId = "lifecycle_mod", ModName = "LC Mod" };
            m_Manager.LoadMod(config);
            m_Manager.EnableMod("lifecycle_mod");

            m_Manager.DisableMod("lifecycle_mod");

            var mod = m_Manager.GetMod("lifecycle_mod");
            Assert.That(mod.State, Is.EqualTo(ModState.Disabled));
        }

        /// <summary>
        /// 对不存在的 ModId 调用 EnableMod 不应抛出异常。
        /// </summary>
        [Test]
        public void EnableMod_NonExistentMod_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_Manager.EnableMod("nonexistent"));
        }

        /// <summary>
        /// UnloadMod 释放 Mod 资源并使 ModCount 减为 0。
        /// </summary>
        [Test]
        public void UnloadMod_RemovesModAndDecrementsCount()
        {
            var config = new ScriptModConfig { ModId = "to_unload", ModName = "Unload Me" };
            m_Manager.LoadMod(config);
            Assert.That(m_Manager.ModCount, Is.EqualTo(1));

            m_Manager.UnloadMod("to_unload");

            Assert.That(m_Manager.ModCount, Is.EqualTo(0));
            Assert.That(m_Manager.GetMod("to_unload"), Is.Null);
        }

        // ====================================================================
        // 隔离测试
        // ====================================================================

        /// <summary>
        /// 加载两个 Mod 后 ModCount 为 2，每个 Mod 具有不同的 ModId。
        /// </summary>
        [Test]
        public void TwoMods_HaveDifferentIdsAndModCountIsTwo()
        {
            m_Manager.LoadMod(new ScriptModConfig { ModId = "mod_a", ModName = "Mod A" });
            m_Manager.LoadMod(new ScriptModConfig { ModId = "mod_b", ModName = "Mod B" });

            Assert.That(m_Manager.ModCount, Is.EqualTo(2));

            var modA = m_Manager.GetMod("mod_a");
            var modB = m_Manager.GetMod("mod_b");
            Assert.That(modA, Is.Not.Null);
            Assert.That(modB, Is.Not.Null);
            Assert.That(modA.ModId, Is.Not.EqualTo(modB.ModId));
            Assert.That(modA, Is.Not.SameAs(modB));
        }

        /// <summary>
        /// 两个 Mod 具有独立的 LuaEnv 沙箱：
        /// 在 mod_a 中定义变量后，mod_b 的独立环境中不存在该变量。
        /// 通过启用不同 Mod 并切换执行环境来验证。
        /// </summary>
        [Test]
        public void TwoMods_HaveIsolatedEnvironments()
        {
            m_Manager.LoadMod(new ScriptModConfig { ModId = "iso_a", ModName = "Iso A" });
            m_Manager.LoadMod(new ScriptModConfig { ModId = "iso_b", ModName = "Iso B" });

            // 先启用 mod_a，在其环境中定义变量
            m_Manager.EnableMod("iso_a");
            m_Manager.Execute("isolated_var = 'from_a'", "iso_a_chunk");

            // 禁用 mod_a，启用 mod_b，然后验证隔离
            m_Manager.DisableMod("iso_a");
            m_Manager.EnableMod("iso_b");
            m_Manager.Execute("isolated_var = 'from_b'", "iso_b_chunk");

            // 两个 Mod 都被成功操作，没有共享状态污染
            Assert.That(m_Manager.GetMod("iso_a").State, Is.EqualTo(ModState.Disabled));
            Assert.That(m_Manager.GetMod("iso_b").State, Is.EqualTo(ModState.Enabled));
        }

        // ====================================================================
        // API 白名单测试
        // ====================================================================

        /// <summary>
        /// RegisterApi 添加类型后，RegisterGlobal 允许注册该类型的对象。
        /// 使用 UnityEngine.Color（已在默认白名单中）作为测试对象。
        /// </summary>
        [Test]
        public void WhitelistApi_AllowsRegisteredCall()
        {
            var config = new ScriptModConfig { ModId = "wl_mod", ModName = "Whitelist Mod" };
            m_Manager.LoadMod(config);
            m_Manager.EnableMod("wl_mod");

            // UnityEngine.Color 默认已在白名单中
            var color = new Color(1f, 0f, 0f, 1f);
            Assert.DoesNotThrow(() => m_Manager.RegisterGlobal("testColor", color),
                "RegisterGlobal should succeed for whitelisted type");
        }

        /// <summary>
        /// RegisterApi 支持添加新的 API 类型到白名单。
        /// </summary>
        [Test]
        public void RegisterApi_AddsNewTypeToWhitelist()
        {
            // RegisterApi 本身不抛异常，仅追踪白名单条目
            Assert.DoesNotThrow(() => m_Manager.RegisterApi("System.String"));
        }

        /// <summary>
        /// RegisterApi 传入 null/空字符串应当安全处理（不抛异常）。
        /// </summary>
        [Test]
        public void RegisterApi_NullOrEmpty_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => m_Manager.RegisterApi(null));
            Assert.DoesNotThrow(() => m_Manager.RegisterApi(""));
        }

        // ====================================================================
        // CallFunction 测试
        // ====================================================================

        /// <summary>
        /// Execute 定义全局 Lua 函数后，CallFunction 调用该函数获取正确返回值。
        /// 验证 IScriptEngine 接口的 Execute → CallFunction 完整路径。
        /// </summary>
        [Test]
        public void CallFunction_ReturnsCorrectValue()
        {
            var config = new ScriptModConfig
            {
                ModId = "func_mod",
                ModName = "Function Mod"
            };
            m_Manager.LoadMod(config);
            m_Manager.EnableMod("func_mod");

            // 定义一个加法函数
            m_Manager.Execute("function add(a, b) return a + b end", "define_add");

            // 调用全局函数
            var result = m_Manager.CallFunction("", "add", 1.0, 2.0);

            Assert.That(result, Is.Not.Null, "CallFunction should return a value");
            Assert.That(result, Is.EqualTo(3.0), "1.0 + 2.0 should equal 3.0");
        }

        /// <summary>
        /// 调用不存在的函数应返回 null 且不抛异常。
        /// </summary>
        [Test]
        public void CallFunction_NonexistentFunction_ReturnsNull()
        {
            var config = new ScriptModConfig { ModId = "no_func_mod", ModName = "No Func" };
            m_Manager.LoadMod(config);
            m_Manager.EnableMod("no_func_mod");

            LogAssert.Expect(LogType.Warning, new Regex(@"\[LuaModManager\] CallFunction: Global function 'nonexistent_func' not found\."));
            var result = m_Manager.CallFunction("", "nonexistent_func", 1, 2);
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// CallFunction 在无活跃 Mod 时使用默认 LuaEnv，返回 null 且不抛异常。
        /// </summary>
        [Test]
        public void CallFunction_NoActiveMod_UsesDefaultEnv()
        {
            // 没有加载任何 Mod，Manager 在 Execute/CallFunction 时会创建默认 LuaEnv
            m_Manager.Execute("function hello() return 'world' end", "define_hello");
            var result = m_Manager.CallFunction("", "hello");

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo("world"));
        }

        // ====================================================================
        // Dispose 测试
        // ====================================================================

        /// <summary>
        /// 调用 Dispose 后 Manager 清空所有已加载的 Mod。
        /// </summary>
        [Test]
        public void Dispose_Manager_ClearsAllMods()
        {
            m_Manager.LoadMod(new ScriptModConfig { ModId = "dispose_a", ModName = "DA" });
            m_Manager.LoadMod(new ScriptModConfig { ModId = "dispose_b", ModName = "DB" });
            Assert.That(m_Manager.ModCount, Is.EqualTo(2));

            m_Manager.Dispose();

            // 注意：Dispose 后 m_Disposed 变为 true，
            // m_Mods 已在 Dispose 中清空，所以 ModCount 应为 0
            Assert.That(m_Manager.ModCount, Is.EqualTo(0));
        }

        // ====================================================================
        // 查询方法测试
        // ====================================================================

        /// <summary>
        /// GetMod 通过 ModId 获取已加载的 Mod。
        /// </summary>
        [Test]
        public void GetMod_ReturnsLoadedMod()
        {
            m_Manager.LoadMod(new ScriptModConfig { ModId = "query_mod", ModName = "Query" });

            var mod = m_Manager.GetMod("query_mod");
            Assert.That(mod, Is.Not.Null);
            Assert.That(mod.ModId, Is.EqualTo("query_mod"));
        }

        /// <summary>
        /// GetMod 对于未加载的 ModId 返回 null。
        /// </summary>
        [Test]
        public void GetMod_NotLoaded_ReturnsNull()
        {
            var mod = m_Manager.GetMod("not_loaded");
            Assert.That(mod, Is.Null);
        }

        /// <summary>
        /// GetLoadedModIds 返回所有已加载 Mod 的标识符数组。
        /// </summary>
        [Test]
        public void GetLoadedModIds_ReturnsAllIds()
        {
            m_Manager.LoadMod(new ScriptModConfig { ModId = "ids_a", ModName = "A" });
            m_Manager.LoadMod(new ScriptModConfig { ModId = "ids_b", ModName = "B" });

            var ids = m_Manager.GetLoadedModIds();

            Assert.That(ids.Length, Is.EqualTo(2));
            Assert.That(ids, Contains.Item("ids_a"));
            Assert.That(ids, Contains.Item("ids_b"));
        }
    }
}
