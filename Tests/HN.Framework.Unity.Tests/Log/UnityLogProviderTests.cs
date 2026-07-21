using System.Text.RegularExpressions;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common.Debug;
using HN.Framework.Unity.Driver.Platform.Log;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace HN.Framework.Unity.Tests.Log
{
    /// <summary>
    /// UnityLogProvider 的 EditMode 单元测试。
    /// 验证 Debug.Log / Debug.LogWarning / Debug.LogError 的正确路由。
    /// 使用 LogAssert.Expect 捕获并验证日志输出。
    /// </summary>
    [TestFixture]
    public class UnityLogProviderTests
    {
        private UnityLogProvider provider;

        [SetUp]
        public void SetUp()
        {
            provider = new UnityLogProvider();
        }

        [TearDown]
        public void TearDown()
        {
            // LogAssert.Expect 在测试结束时自动验证所有期望
        }

        /// <summary>
        /// Log(string) 应调用 Debug.Log 输出原始消息。
        /// </summary>
        [Test]
        public void Log_StringMethod_LogsToDebug()
        {
            LogAssert.Expect(LogType.Log, new Regex("plain"));
            provider.Log("plain");
        }

        /// <summary>
        /// Log(LogLevel.Debug, ...) 应使用 Debug.Log。
        /// </summary>
        [Test]
        public void Log_DebugLevel_UsesDebugLog()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[Debug\] \[test\] hello"));
            provider.Log(LogLevel.Debug, "test", "hello");
        }

        /// <summary>
        /// Log(LogLevel.Info, ...) 应使用 Debug.Log。
        /// </summary>
        [Test]
        public void Log_InfoLevel_UsesDebugLog()
        {
            LogAssert.Expect(LogType.Log, new Regex(@"\[Info\] \[test\] hello"));
            provider.Log(LogLevel.Info, "test", "hello");
        }

        /// <summary>
        /// Log(LogLevel.Warning, ...) 应使用 Debug.LogWarning。
        /// </summary>
        [Test]
        public void Log_WarningLevel_UsesWarningLog()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Warning\] \[ch\] msg"));
            provider.Log(LogLevel.Warning, "ch", "msg");
        }

        /// <summary>
        /// Log(LogLevel.Error, ...) 应使用 Debug.LogError。
        /// </summary>
        [Test]
        public void Log_ErrorLevel_UsesErrorLog()
        {
            LogAssert.Expect(LogType.Error, new Regex(@"\[Error\] \[ch\] msg"));
            provider.Log(LogLevel.Error, "ch", "msg");
        }

        /// <summary>
        /// Log(LogLevel.Fatal, ...) 应与 Error 一样使用 Debug.LogError。
        /// </summary>
        [Test]
        public void Log_FatalLevel_UsesErrorLog()
        {
            LogAssert.Expect(LogType.Error, new Regex(@"\[Fatal\] \[ch\] msg"));
            provider.Log(LogLevel.Fatal, "ch", "msg");
        }
    }
}
