using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if HAS_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

using Object = System.Object;

namespace HN.Framework.Core.Driver.Common
{
    /// <summary>
    /// 异步加载资源返回的句柄
    /// 通过这个句柄可以获取加载的结果、是否完成、进度等信息
    /// </summary>
    public class AsyncLoadHandle
    {
        #region 构造函数
#if HAS_ADDRESSABLES
        /// <summary>
        /// 构造函数
        /// 使用Addressables的返回句柄构造
        /// </summary>
        /// <param name="handle"></param>
        public AsyncLoadHandle(AsyncOperationHandle handle)
        {
            m_target = handle.Result;
            m_isDone = handle.IsDone;
            m_percentComplete = handle.PercentComplete;
            m_status = GetStatus(handle.Status);
            handle.Completed += (op) =>
            {
                m_isDone = true;
                m_status = GetStatus(op.Status);
                CompletedEvent?.Invoke();
            };
        }
#endif

        /// <summary>
        /// 构造函数
        /// 使用Unity的Resources的返回句柄构造
        /// </summary>
        /// <param name="request"></param>
        public AsyncLoadHandle(ResourceRequest request)
        {
            m_target = request.asset;
            m_isDone = request.isDone;
            m_percentComplete = request.progress;
            m_status = request.isDone ? AsyncLoadStatus.Succeeded : AsyncLoadStatus.Progressing;
            request.completed += (op) =>
            {
                m_isDone = true;
                m_status = AsyncLoadStatus.Succeeded;
                CompletedEvent?.Invoke();
            };
        }

        public AsyncLoadHandle(Object asset)
        {
            m_target = asset;
            m_isDone = true;
            m_percentComplete = 1f;
            m_status = AsyncLoadStatus.Succeeded;
        }
        #endregion

        #region 对外函数
        #endregion

        #region 内部函数
#if HAS_ADDRESSABLES
        private AsyncLoadStatus GetStatus(AsyncOperationStatus status)
        {
            switch (status)
            {
                case AsyncOperationStatus.None:
                    return AsyncLoadStatus.Progressing;
                case AsyncOperationStatus.Succeeded:
                    return AsyncLoadStatus.Succeeded;
                case AsyncOperationStatus.Failed:
                    return AsyncLoadStatus.Failed;
            }
            return AsyncLoadStatus.Failed;
        }
#endif
        #endregion


        /// <summary>
        /// 异步加载状态枚举
        /// </summary>
        public enum AsyncLoadStatus
        {
            /// <summary>
            /// 正在加载
            /// </summary>
            Progressing,

            /// <summary>
            /// 加载成功
            /// </summary>
            Succeeded,

            /// <summary>
            /// 加载失败
            /// </summary>
            Failed,
        }

        #region 对外属性
        /// <summary>
        /// 加载完成时的回调事件
        /// </summary>
        public Action CompletedEvent;

        /// <summary>
        /// 被加载的资源
        /// </summary>
        public Object Target => m_target;

        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool IsDone => m_isDone;

        /// <summary>
        /// 加载进度
        /// </summary>
        public float PercentComplete => m_percentComplete;

        /// <summary>
        /// 加载状态
        /// </summary>
        public AsyncLoadStatus Status => m_status;
        #endregion

        protected Object m_target;
        protected bool m_isDone;
        protected float m_percentComplete;
        protected AsyncLoadStatus m_status;
    }
}
