#nullable enable

using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 奖励发放结果。
    /// 不可变值类型，记录单次或批量发放的成败状态与详情。
    /// </summary>
    public readonly struct RewardDeliveryResult
    {
        /// <summary>是否发放成功。</summary>
        public readonly bool Success;

        /// <summary>已发放的条目列表（成功时可能为 null）。</summary>
        public readonly IReadOnlyList<(RewardType Type, int Id, int Amount)>? DeliveredItems;

        /// <summary>失败时的错误信息。</summary>
        public readonly string? ErrorMessage;

        private RewardDeliveryResult(bool success, IReadOnlyList<(RewardType, int, int)>? items, string? error)
        {
            Success = success;
            DeliveredItems = items;
            ErrorMessage = error;
        }

        /// <summary>创建一个成功结果。</summary>
        /// <param name="items">已发放的条目列表，可为 null。</param>
        public static RewardDeliveryResult SuccessResult(List<(RewardType, int, int)>? items = null)
        {
            return new RewardDeliveryResult(true, items, null);
        }

        /// <summary>创建一个失败结果。</summary>
        /// <param name="error">错误描述信息。</param>
        public static RewardDeliveryResult FailureResult(string error)
        {
            return new RewardDeliveryResult(false, null, error);
        }

        /// <summary>
        /// 合并多个发放结果。
        /// 任一结果失败则整体视为失败，失败信息取最后一个非空错误；
        /// 成功时合并所有已发放条目。
        /// </summary>
        /// <param name="results">待合并的结果数组。</param>
        public static RewardDeliveryResult Merge(params RewardDeliveryResult[] results)
        {
            if (results == null || results.Length == 0)
            {
                return SuccessResult();
            }

            bool allSuccess = true;
            string? lastError = null;
            var merged = new List<(RewardType, int, int)>();

            foreach (var r in results)
            {
                if (!r.Success)
                {
                    allSuccess = false;
                    if (r.ErrorMessage != null)
                    {
                        lastError = r.ErrorMessage;
                    }
                }

                if (r.DeliveredItems != null)
                {
                    merged.AddRange(r.DeliveredItems);
                }
            }

            return allSuccess
                ? SuccessResult(merged)
                : FailureResult(lastError ?? "Unknown error");
        }
    }
}
