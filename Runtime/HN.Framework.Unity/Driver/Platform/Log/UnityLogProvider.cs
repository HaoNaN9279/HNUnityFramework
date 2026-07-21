using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common.Debug;
using UnityEngine;

namespace HN.Framework.Unity.Driver.Platform.Log
{
    public class UnityLogProvider : ILogProvider
    {
        public void Log(string message)
        {
            Debug.Log(message);
        }

        public void Log(LogLevel level, string channel, string message, object context = null)
        {
            var formatted = string.Format("[{0}] [{1}] {2}", level, channel, message);
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    Debug.LogError(formatted);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formatted);
                    break;
                default:
                    Debug.Log(formatted);
                    break;
            }
        }
    }
}
