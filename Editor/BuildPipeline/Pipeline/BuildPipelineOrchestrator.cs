using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建管线编排器。按顺序执行构建步骤，遇错即停。
    /// </summary>
    public class BuildPipelineOrchestrator
    {
        private readonly List<IBuildStep> _steps = new();
        private readonly BuildContext _context = new();

        /// <summary>构建上下文</summary>
        public BuildContext Context => _context;

        /// <summary>已注册的构建步骤列表（只读）</summary>
        public IReadOnlyList<IBuildStep> Steps => _steps;

        /// <summary>添加一个构建步骤</summary>
        /// <param name="step">构建步骤实例</param>
        /// <exception cref="ArgumentNullException">step 为 null 时抛出</exception>
        public void AddStep(IBuildStep step)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            _steps.Add(step);
        }

        /// <summary>批量添加构建步骤</summary>
        /// <param name="steps">构建步骤实例数组</param>
        public void AddSteps(params IBuildStep[] steps)
        {
            foreach (var step in steps)
            {
                AddStep(step);
            }
        }

        /// <summary>移除指定步骤</summary>
        /// <param name="step">要移除的步骤实例</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveStep(IBuildStep step)
        {
            return _steps.Remove(step);
        }

        /// <summary>清空所有已注册的步骤</summary>
        public void ClearSteps()
        {
            _steps.Clear();
        }

        /// <summary>配置构建参数</summary>
        /// <param name="buildTarget">构建目标平台</param>
        /// <param name="buildTargetGroup">构建目标平台组</param>
        /// <param name="outputPath">输出路径</param>
        /// <param name="version">版本号</param>
        public void Configure(
            BuildTarget buildTarget,
            BuildTargetGroup buildTargetGroup,
            string outputPath,
            string version)
        {
            _context.BuildTarget = buildTarget;
            _context.BuildTargetGroup = buildTargetGroup;
            _context.OutputPath = outputPath;
            _context.Version = version;
        }

        /// <summary>按顺序执行所有构建步骤。任一步骤返回 false 或抛出异常时，中止管线并返回 false。</summary>
        /// <returns>所有步骤是否全部执行成功</returns>
        public bool Execute()
        {
            _context.StepLogs.Clear();

            Debug.Log($"[BuildPipeline] Starting pipeline execution. Version: {_context.Version}, Target: {_context.BuildTarget}");

            foreach (var step in _steps)
            {
                if (_context.IsCancelled)
                {
                    Debug.LogWarning($"[BuildPipeline] Build cancelled. Skipping '{step.StepName}'.");
                    _context.StepLogs.Add($"Build cancelled before '{step.StepName}'.");
                    return false;
                }

                Debug.Log($"[BuildPipeline] Executing step: {step.StepName} - {step.Description}");
                _context.StepLogs.Add($"--- {step.StepName} ---");

                try
                {
                    bool success = step.Execute(_context);
                    if (!success)
                    {
                        Debug.LogError($"[BuildPipeline] Step '{step.StepName}' failed. Pipeline aborted.");
                        _context.StepLogs.Add($"Step '{step.StepName}' FAILED.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[BuildPipeline] Step '{step.StepName}' threw exception: {ex}");
                    _context.StepLogs.Add($"Step '{step.StepName}' EXCEPTION: {ex.Message}");
                    return false;
                }

                Debug.Log($"[BuildPipeline] Step '{step.StepName}' completed.");
            }

            Debug.Log("[BuildPipeline] All steps completed successfully.");
            _context.StepLogs.Add("Pipeline execution completed successfully.");
            return true;
        }

        /// <summary>取消构建。后续步骤将跳过。</summary>
        public void Cancel()
        {
            _context.IsCancelled = true;
        }
    }
}
