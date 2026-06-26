using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HN.Framework.Editor
{
    public class HNUnityFrameworkEditorMenus
    {
        /// <summary>
        /// 部署框架，执行框架初始化与资源配置。
        /// </summary>
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Deploy Framework", false, 0)]
        public static void DeployFramework()
        {
            FrameworkDeployer.Deploy();
        }


        /// <summary>
        /// 更新 Addressable 分组配置，同步资源组设置。
        /// </summary>
        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Update Addressable Groups", false, 1)]
        public static void UpdateAddressableGroups()
        {
            AddressablesGroupsUpdater.Update();
        }


    }
}
