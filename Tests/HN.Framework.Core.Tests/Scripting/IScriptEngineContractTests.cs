using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using HN.Framework.Core.Capability.Scripting;
using HN.Framework.Core.Driver;

namespace HN.Framework.Core.Tests.Scripting
{
    /// <summary>
    /// 契约测试：验证 C15 Scripting 模块的接口定义和枚举值，确保不因意外修改而破坏契约。
    /// 使用反射验证接口成员的存在性、返回类型和参数签名。
    /// </summary>
    [TestFixture]
    public class IScriptEngineContractTests
    {
        // ====================================================================
        // IScriptEngine 接口契约
        // ====================================================================

        /// <summary>
        /// Execute(string, string) 方法必须在 IScriptEngine 接口上声明。
        /// </summary>
        [Test]
        public void IScriptEngine_Interface_DefinesExecuteMethod()
        {
            var methods = typeof(IScriptEngine).GetMethods();
            Assert.That(methods, Has.Some.Matches<MethodInfo>(m =>
                m.Name == "Execute" &&
                m.ReturnType == typeof(void) &&
                HasStringStringParameters(m)));
        }

        /// <summary>
        /// RegisterGlobal(string, object) 方法必须在 IScriptEngine 接口上声明。
        /// </summary>
        [Test]
        public void IScriptEngine_Interface_DefinesRegisterGlobalMethod()
        {
            var methods = typeof(IScriptEngine).GetMethods();
            Assert.That(methods, Has.Some.Matches<MethodInfo>(m =>
                m.Name == "RegisterGlobal" &&
                m.ReturnType == typeof(void) &&
                HasStringObjectParameters(m)));
        }

        /// <summary>
        /// CallFunction(string, string, object[]) 方法必须在 IScriptEngine 接口上声明。
        /// 返回类型应为 object。
        /// </summary>
        [Test]
        public void IScriptEngine_Interface_DefinesCallFunctionMethod()
        {
            var methods = typeof(IScriptEngine).GetMethods();
            Assert.That(methods, Has.Some.Matches<MethodInfo>(m =>
                m.Name == "CallFunction" &&
                m.ReturnType == typeof(object)));
        }

        /// <summary>
        /// IScriptEngine 必须直接声明 Dispose() 方法以释放脚本引擎资源。
        /// </summary>
        [Test]
        public void IScriptEngine_Interface_DefinesDisposeMethod()
        {
            var method = typeof(IScriptEngine).GetMethod("Dispose");
            Assert.That(method, Is.Not.Null,
                "IScriptEngine should declare a Dispose method");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(method.GetParameters().Length, Is.EqualTo(0));
        }

        /// <summary>
        /// IScriptEngine 必须恰好声明 4 个成员方法
        /// （Execute, RegisterGlobal, CallFunction, Dispose）。
        /// </summary>
        [Test]
        public void IScriptEngine_Interface_HasExpectedMemberCount()
        {
            var methods = typeof(IScriptEngine).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.That(methods.Length, Is.EqualTo(4),
                "IScriptEngine should declare exactly 4 methods");
        }

        // ====================================================================
        // IScriptMod 接口契约
        // ====================================================================

        /// <summary>
        /// IScriptMod 必须声明 string 类型的 ModId 只读属性。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesModIdProperty()
        {
            var prop = typeof(IScriptMod).GetProperty("ModId");
            Assert.That(prop, Is.Not.Null, "IScriptMod should have ModId property");
            Assert.That(prop.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(prop.CanRead, Is.True);
            Assert.That(prop.CanWrite, Is.False);
        }

        /// <summary>
        /// IScriptMod 必须声明 string 类型的 ModName 只读属性。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesModNameProperty()
        {
            var prop = typeof(IScriptMod).GetProperty("ModName");
            Assert.That(prop, Is.Not.Null, "IScriptMod should have ModName property");
            Assert.That(prop.PropertyType, Is.EqualTo(typeof(string)));
            Assert.That(prop.CanRead, Is.True);
            Assert.That(prop.CanWrite, Is.False);
        }

        /// <summary>
        /// IScriptMod 必须声明 ModState 类型的 State 只读属性。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesStateProperty()
        {
            var prop = typeof(IScriptMod).GetProperty("State");
            Assert.That(prop, Is.Not.Null, "IScriptMod should have State property");
            Assert.That(prop.PropertyType, Is.EqualTo(typeof(ModState)));
            Assert.That(prop.CanRead, Is.True);
        }

        /// <summary>
        /// IScriptMod 必须声明 OnLoad() 方法。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesOnLoadMethod()
        {
            var method = typeof(IScriptMod).GetMethod("OnLoad");
            Assert.That(method, Is.Not.Null, "IScriptMod should have OnLoad method");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(method.GetParameters().Length, Is.EqualTo(0));
        }

        /// <summary>
        /// IScriptMod 必须声明 OnEnable() 方法。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesOnEnableMethod()
        {
            var method = typeof(IScriptMod).GetMethod("OnEnable");
            Assert.That(method, Is.Not.Null, "IScriptMod should have OnEnable method");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(method.GetParameters().Length, Is.EqualTo(0));
        }

        /// <summary>
        /// IScriptMod 必须声明 OnDisable() 方法。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesOnDisableMethod()
        {
            var method = typeof(IScriptMod).GetMethod("OnDisable");
            Assert.That(method, Is.Not.Null, "IScriptMod should have OnDisable method");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(method.GetParameters().Length, Is.EqualTo(0));
        }

        /// <summary>
        /// IScriptMod 必须声明 OnUnload() 方法。
        /// </summary>
        [Test]
        public void IScriptMod_Interface_DefinesOnUnloadMethod()
        {
            var method = typeof(IScriptMod).GetMethod("OnUnload");
            Assert.That(method, Is.Not.Null, "IScriptMod should have OnUnload method");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
            Assert.That(method.GetParameters().Length, Is.EqualTo(0));
        }

        // ====================================================================
        // ModState 枚举契约
        // ====================================================================

        /// <summary>
        /// ModState 枚举必须包含全部 5 个预期值：
        /// NotLoaded=0, Loaded=1, Enabled=2, Disabled=3, Error=4。
        /// </summary>
        [Test]
        public void ModState_Enum_HasAllExpectedValues()
        {
            var values = Enum.GetValues(typeof(ModState));
            Assert.That(values.Length, Is.EqualTo(5),
                "ModState should have exactly 5 values");

            Assert.That((int)ModState.NotLoaded, Is.EqualTo(0));
            Assert.That((int)ModState.Loaded, Is.EqualTo(1));
            Assert.That((int)ModState.Enabled, Is.EqualTo(2));
            Assert.That((int)ModState.Disabled, Is.EqualTo(3));
            Assert.That((int)ModState.Error, Is.EqualTo(4));
        }

        // ====================================================================
        // ScriptModConfig 数据模型契约
        // ====================================================================

        /// <summary>
        /// ScriptModConfig 的 Sandboxed 属性默认为 true。
        /// </summary>
        [Test]
        public void ScriptModConfig_HasDefaultSandboxedTrue()
        {
            var config = new ScriptModConfig();
            Assert.That(config.Sandboxed, Is.True,
                "ScriptModConfig.Sandboxed should default to true");
        }

        /// <summary>
        /// ScriptModConfig 的所有属性均可读写。
        /// </summary>
        [Test]
        public void ScriptModConfig_CanSetAllProperties()
        {
            var config = new ScriptModConfig
            {
                ModId = "test_id",
                ModName = "Test Mod",
                Version = "1.0.0",
                ScriptPaths = new[] { "main.lua", "utils.lua" },
                Sandboxed = false
            };

            Assert.That(config.ModId, Is.EqualTo("test_id"));
            Assert.That(config.ModName, Is.EqualTo("Test Mod"));
            Assert.That(config.Version, Is.EqualTo("1.0.0"));
            Assert.That(config.ScriptPaths, Is.EquivalentTo(new[] { "main.lua", "utils.lua" }));
            Assert.That(config.Sandboxed, Is.False);
        }

        // ====================================================================
        // IHotUpdateEntry 接口契约
        // ====================================================================

        /// <summary>
        /// IHotUpdateEntry 必须声明 OnRegisterGameModules(GameWorld) 方法。
        /// </summary>
        [Test]
        public void IHotUpdateEntry_Interface_DefinesOnRegisterGameModules()
        {
            var method = typeof(IHotUpdateEntry).GetMethod("OnRegisterGameModules");
            Assert.That(method, Is.Not.Null,
                "IHotUpdateEntry should have OnRegisterGameModules method");
            Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));

            var parameters = method.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(1));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(GameWorld)));
        }

        /// <summary>
        /// GameWorld 是一个具体类（非抽象、非接口），可被 IHotUpdateEntry 接收。
        /// </summary>
        [Test]
        public void GameWorld_IsConcreteClass()
        {
            var type = typeof(GameWorld);
            Assert.That(type.IsClass, Is.True);
            Assert.That(type.IsAbstract, Is.False);
            Assert.That(type.IsInterface, Is.False);
        }

        /// <summary>
        /// IHotUpdateEntry 是一个接口（非类、非抽象类）。
        /// </summary>
        [Test]
        public void IHotUpdateEntry_IsInterface()
        {
            Assert.That(typeof(IHotUpdateEntry).IsInterface, Is.True);
        }

        // ====================================================================
        // 辅助方法
        // ====================================================================

        /// <summary>
        /// 检查 MethodInfo 是否具有 (string, string) 参数签名。
        /// </summary>
        private static bool HasStringStringParameters(MethodInfo m)
        {
            var p = m.GetParameters();
            return p.Length == 2
                && p[0].ParameterType == typeof(string)
                && p[1].ParameterType == typeof(string);
        }

        /// <summary>
        /// 检查 MethodInfo 是否具有 (string, object) 参数签名。
        /// </summary>
        private static bool HasStringObjectParameters(MethodInfo m)
        {
            var p = m.GetParameters();
            return p.Length == 2
                && p[0].ParameterType == typeof(string)
                && p[1].ParameterType == typeof(object);
        }
    }
}
