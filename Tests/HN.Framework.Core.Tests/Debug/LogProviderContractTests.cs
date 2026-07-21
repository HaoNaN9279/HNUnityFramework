using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Tests.Debug
{
    [TestFixture]
    public class LogProviderContractTests
    {
        private Type _interfaceType;

        [SetUp]
        public void SetUp()
        {
            _interfaceType = typeof(ILogProvider);
        }

        /// <summary>
        /// 验证 ILogProvider 包含 Log(string) 方法（向后兼容）。
        /// </summary>
        [Test]
        public void Interface_HasLogStringMethod()
        {
            var method = _interfaceType.GetMethod("Log", new Type[] { typeof(string) });
            Assert.IsNotNull(method, "ILogProvider should have Log(string) method");
            Assert.AreEqual(typeof(void), method.ReturnType);
        }

        /// <summary>
        /// 验证 ILogProvider 包含结构化 Log 方法。
        /// </summary>
        [Test]
        public void Interface_HasStructuredLogMethod()
        {
            var method = _interfaceType.GetMethod("Log", new Type[] { typeof(LogLevel), typeof(string), typeof(string), typeof(object) });
            Assert.IsNotNull(method, "ILogProvider should have Log(LogLevel, string, string, object) method");
            Assert.AreEqual(typeof(void), method.ReturnType);
        }

        /// <summary>
        /// 验证 ILogProvider 恰好包含 2 个 Log 方法重载。
        /// </summary>
        [Test]
        public void Interface_HasExactlyTwoMethods()
        {
            var methods = _interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var logMethods = methods.Where(m => m.Name == "Log").ToArray();
            Assert.AreEqual(2, logMethods.Length, "ILogProvider should have exactly 2 Log method overloads");
        }
    }
}
