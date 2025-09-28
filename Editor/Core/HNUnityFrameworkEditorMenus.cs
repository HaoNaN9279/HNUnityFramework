using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HN.Framework.Editor
{
    public class HNUnityFrameworkEditorMenus
    {
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Deploy Framework", false, 0)]
        public static void DeployFramework()
        {
            FrameworkDeployer.Deploy();
        }


        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Update Addressable Groups", false, 1)]
        public static void UpdateAddressableGroups()
        {
            AddressablesGroupsUpdater.Update();
        }


    }
}
