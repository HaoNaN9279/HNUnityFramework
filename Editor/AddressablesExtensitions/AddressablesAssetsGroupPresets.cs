using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HN.Framework.Editor
{
    /// <summary>
    /// Addressables 资源组预设集合，存储在 ScriptableObject 中。
    /// </summary>
    public class AddressablesAssetsGroupPresets : ScriptableObject
    {
        [SerializeField]
        private List<AddressablesAssetsGroupPreset> m_GroupPresets;

        /// <summary>
        /// 资源组预设列表。
        /// </summary>
        public List<AddressablesAssetsGroupPreset> GroupPresets => m_GroupPresets;
    }


    /// <summary>
    /// 单个 Addressables 资源组预设，包含组名称和路径关键字。
    /// </summary>
    [Serializable]
    public struct AddressablesAssetsGroupPreset
    {
        [SerializeField]
        private string m_GroupName;

        /// <summary>
        /// 资源组名称。
        /// </summary>
        public string GroupName
        {
            readonly get => m_GroupName;
            set => m_GroupName = value;
        }

        [SerializeField]
        private string m_PathKeywords;

        /// <summary>
        /// 路径关键字，用于匹配资源路径。
        /// </summary>
        public string PathKeywords
        {
            readonly get => m_PathKeywords;
            set => m_PathKeywords = value;
        }
    }
}
