using System.Collections.Generic;

namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画资源引用，存储 Addressables key + 默认角色绑定映射
    /// </summary>
    [System.Serializable]
    public struct CutsceneAssetRef
    {
        /// <summary>Addressables key 或资源路径</summary>
        public string AddressablesKey { get; set; }

        /// <summary>默认角色绑定映射表：角色名 → 绑定目标标识</summary>
        public Dictionary<string, string> DefaultBindings { get; set; }

        public CutsceneAssetRef(string addressablesKey, Dictionary<string, string>? defaultBindings = null)
        {
            AddressablesKey = addressablesKey ?? string.Empty;
            DefaultBindings = defaultBindings ?? new Dictionary<string, string>();
        }
    }
}
