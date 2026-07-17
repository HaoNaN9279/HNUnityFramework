using System;

namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场角色槽位数据，定义 Timeline 中使用的角色相关信息
    /// </summary>
    [Serializable]
    public struct CutsceneRole
    {
        /// <summary>角色名（用于 Track 绑定解析的键）</summary>
        public string RoleName { get; set; }
        
        /// <summary>显示名称（用于 UI 字幕的说话者名称）</summary>
        public string DisplayName { get; set; }
        
        /// <summary>默认绑定键（未指定绑定时使用的回退场景路径/Tag）</summary>
        public string DefaultBindingKey { get; set; }

        public CutsceneRole(string roleName, string displayName, string defaultBindingKey = "")
        {
            RoleName = roleName ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            DefaultBindingKey = defaultBindingKey ?? string.Empty;
        }
    }
}
