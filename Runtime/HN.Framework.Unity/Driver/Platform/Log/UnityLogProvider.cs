using HN.Framework.Core.Capability;
using UnityEngine;

namespace HN.Framework.Unity.Driver.Platform.Log
{
    public class UnityLogProvider : ILogProvider
    {
        public void Log(string message)
        {
            Debug.Log(message);
        }
    }
}
