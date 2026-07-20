#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 条件评估结果。
    /// 记录条件是否满足、是否已失败、当前进度和调试信息。
    /// </summary>
    public readonly struct ConditionResult
    {
        /// <summary>条件是否满足。</summary>
        public readonly bool IsMet;

        /// <summary>是否已失败（如 Counter 中超时、状态不满足且无法补救等）。</summary>
        public readonly bool HasFailed;

        /// <summary>当前进度，取值范围 [0.0, 1.0]。</summary>
        public readonly float Progress;

        /// <summary>调试信息。</summary>
        public readonly string? DebugInfo;

        /// <summary>
        /// 初始化条件评估结果。
        /// </summary>
        /// <param name="isMet">条件是否满足。</param>
        /// <param name="hasFailed">是否已失败。</param>
        /// <param name="progress">当前进度。</param>
        /// <param name="debugInfo">调试信息。</param>
        private ConditionResult(bool isMet, bool hasFailed, float progress, string? debugInfo)
        {
            IsMet = isMet;
            HasFailed = hasFailed;
            Progress = Math.Clamp(progress, 0f, 1f);
            DebugInfo = debugInfo;
        }

        /// <summary>条件已满足（进度 100%）。</summary>
        public static ConditionResult Met() => new(true, false, 1f, null);

        /// <summary>条件未满足，指定当前进度。</summary>
        /// <param name="progress">当前进度，取值范围 [0.0, 1.0]。</param>
        public static ConditionResult NotMet(float progress) => new(false, false, progress, null);

        /// <summary>条件已失败。</summary>
        /// <param name="reason">失败原因描述。</param>
        public static ConditionResult Failed(string reason) => new(false, true, 0f, reason);

        /// <summary>
        /// 逻辑与：所有结果都满足才满足；任一失败则失败。
        /// 进度取各结果的最小值。
        /// </summary>
        /// <param name="results">待组合的条件评估结果数组。</param>
        /// <returns>组合后的评估结果。</returns>
        /// <exception cref="ArgumentNullException">results 为 null。</exception>
        public static ConditionResult And(params ConditionResult[] results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (results.Length == 0)
            {
                return Met();
            }

            bool allMet = true;
            bool anyFailed = false;
            float minProgress = 1f;
            string? debugInfo = null;

            foreach (ConditionResult result in results)
            {
                if (!result.IsMet)
                {
                    allMet = false;
                }

                if (result.HasFailed)
                {
                    anyFailed = true;
                    debugInfo = result.DebugInfo;
                }

                if (result.Progress < minProgress)
                {
                    minProgress = result.Progress;
                }
            }

            if (anyFailed)
            {
                return new ConditionResult(false, true, minProgress, debugInfo);
            }

            if (allMet)
            {
                return Met();
            }

            return new ConditionResult(false, false, minProgress, null);
        }

        /// <summary>
        /// 逻辑或：任一结果满足则满足；全部失败才失败。
        /// 进度取各结果的最大值。
        /// </summary>
        /// <param name="results">待组合的条件评估结果数组。</param>
        /// <returns>组合后的评估结果。</returns>
        /// <exception cref="ArgumentNullException">results 为 null。</exception>
        public static ConditionResult Or(params ConditionResult[] results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (results.Length == 0)
            {
                return Met();
            }

            bool anyMet = false;
            bool allFailed = true;
            float maxProgress = 0f;
            string? debugInfo = null;

            foreach (ConditionResult result in results)
            {
                if (result.IsMet)
                {
                    anyMet = true;
                }

                if (!result.HasFailed)
                {
                    allFailed = false;
                }
                else
                {
                    debugInfo ??= result.DebugInfo;
                }

                if (result.Progress > maxProgress)
                {
                    maxProgress = result.Progress;
                }
            }

            if (anyMet)
            {
                return Met();
            }

            if (allFailed)
            {
                return new ConditionResult(false, true, maxProgress, debugInfo);
            }

            return new ConditionResult(false, false, maxProgress, null);
        }

        /// <summary>
        /// 逻辑非：对单个结果取反。
        /// </summary>
        /// <param name="result">待取反的条件评估结果。</param>
        /// <returns>取反后的评估结果。</returns>
        public static ConditionResult Not(ConditionResult result)
        {
            if (result.HasFailed)
            {
                return Met();
            }

            if (result.IsMet)
            {
                return Failed("NOT 条件：原条件已满足，取反后视为失败。");
            }

            return new ConditionResult(true, false, 1f - result.Progress, null);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (HasFailed)
            {
                return $"Failed: {DebugInfo ?? "无详情"}";
            }

            if (IsMet)
            {
                return "Met (100%)";
            }

            return $"NotMet ({Progress:P0})";
        }
    }
}
