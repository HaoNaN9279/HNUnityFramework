using System;
using System.Collections.Generic;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建步骤上下文，传递配置、状态和报告数据。
    /// </summary>
    public class BuildContext
    {
        /// <summary>构建目标平台</summary>
        public UnityEditor.BuildTarget BuildTarget { get; set; }

        /// <summary>构建目标平台组</summary>
        public UnityEditor.BuildTargetGroup BuildTargetGroup { get; set; }

        /// <summary>输出路径</summary>
        public string OutputPath { get; set; }

        /// <summary>版本号</summary>
        public string Version { get; set; }

        /// <summary>是否启用质检门禁</summary>
        public bool EnableValidation { get; set; }

        /// <summary>构建是否已取消</summary>
        public bool IsCancelled { get; set; }

        /// <summary>自定义键值对存储</summary>
        public Dictionary<string, object> CustomData { get; set; } = new();

        /// <summary>步骤日志</summary>
        public List<string> StepLogs { get; set; } = new();
    }

    /// <summary>
    /// 构建步骤接口。
    /// </summary>
    public interface IBuildStep
    {
        /// <summary>步骤名称</summary>
        string StepName { get; }

        /// <summary>步骤描述</summary>
        string Description { get; }

        /// <summary>执行步骤</summary>
        /// <param name="context">构建上下文</param>
        /// <returns>步骤是否执行成功</returns>
        bool Execute(BuildContext context);
    }
}
