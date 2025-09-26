using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HN.Framework.Editor
{
    public class AddressablesAssetsGroupPresets : ScriptableObject
    {
        [SerializeField]
        public List<AddressablesAssetsGroupPreset> GroupPresets;
    }


    [Serializable]
    public struct AddressablesAssetsGroupPreset
    {
        public string GroupName;
        public string PathKeywords;

    }
}
