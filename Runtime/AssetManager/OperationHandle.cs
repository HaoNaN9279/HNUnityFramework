using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using PlasticGui.WorkspaceWindow.PendingChanges;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace HN.Framework
{
    public class OperationHandle<T>
    {
        public T Result => result;
        public bool IsDone => isDone;
        public float PercentComplete => percentComplete;
        public OperationHandleStatus Status => status;
        
        public event Action<AsyncOperationHandle<T>> Completed;


        private AsyncOperationHandle<T> handle;
        private T result;
        private bool isDone;
        private float percentComplete;
        private OperationHandleStatus status;


        public OperationHandle(AsyncOperationHandle<T> handle)
        {
            this.handle = handle;

            this.result = handle.Result;
            this.isDone = handle.IsDone;
            this.percentComplete = handle.PercentComplete;
            status = GetStatus(handle.Status);

            handle.Completed += (handle) =>
            {
                Completed.Invoke(handle);
            };
        }

        private OperationHandleStatus GetStatus(AsyncOperationStatus status)
        {
            switch (status)
            {
                case AsyncOperationStatus.None:
                    return OperationHandleStatus.Progressing;
                case AsyncOperationStatus.Succeeded:
                    return OperationHandleStatus.Succeeded;
                case AsyncOperationStatus.Failed:
                    return OperationHandleStatus.Failed;
            }
            return OperationHandleStatus.Failed;
        }


        public delegate void OnCompleted(OperationHandle<T> handle);

        public enum OperationHandleStatus
        {
            Progressing,
            Succeeded,
            Failed,
        }
    }
}
