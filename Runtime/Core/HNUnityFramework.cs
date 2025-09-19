using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    /// <summary>
    /// 游戏框架类
    /// </summary>
    public abstract class HNUnityFramework : MonoBehaviour, ITickable
    {
        #region Unity生命周期
        void Awake()
        {
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            globalSettings = HNUnityFrameworkGlobalSettings.GetOrCreateSettings();

            HNLogicTime.Initialize();
            AssetManager.Initialize();
            ObjectPoolManager.Initialize();
            ProcedureManager.Initialize();
            ControllerManager.Initialize();
        }

        void Start()
        {
            OnStart();
        }

        protected virtual void OnStart()
        {
            if (globalSettings.LogicRateMode == LogicRateMode.Custom)
            {
                fixedLogicFrameTime = Mathf.Min(1.0f / globalSettings.LogicRate, globalSettings.MaxFrameTime);
            }
            else if (globalSettings.LogicRateMode == LogicRateMode.NoLimit)
            {
                fixedLogicFrameTime = globalSettings.MaxFrameTime;
            }

            currentTime = Time.realtimeSinceStartupAsDouble;
        }

        void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {
            LogicTimeUpdate(Tick);
        }

        void LateUpdate()
        {
            OnLateUpdate();
        }

        protected virtual void OnLateUpdate()
        {
            LogicTimeUpdate(LateTick);
        }

        void OnDestroy()
        {
            ControllerManager.Uninitialize();
            ProcedureManager.ShutdownProcedure();
            ProcedureManager.Uninitialize();
            ObjectPoolManager.Uninitialize();
            AssetManager.Uninitialize();
        }
        #endregion

        public void Tick()
        {
            AssetManager.TickAssetManager();
            ObjectPoolManager.TickObjectPoolManager();
            ProcedureManager.TickProcedureManager();
            ControllerManager.TickControllerManager();
        }

        public void LateTick()
        {
            AssetManager.LateTickAssetManager();
            ObjectPoolManager.LateTickObjectPoolManager();
            ProcedureManager.LateTickProcedureManager();
            ControllerManager.LateTickControllerManager();
        }

        protected void LogicTimeUpdate(Action tickFunc)
        {
            double newTime = Time.realtimeSinceStartupAsDouble;
            double frameTime = newTime - currentTime;
            currentTime = newTime;

            if (frameTime > globalSettings.MaxFrameTime)
                frameTime = globalSettings.MaxFrameTime;

            accumulatedTime += frameTime;

            int steps = 0;
            while (accumulatedTime >= fixedLogicFrameTime && steps < globalSettings.MaxStepsPerFrame)
            {
                UpdateLogicTime(fixedLogicFrameTime);

                tickFunc.Invoke();
                accumulatedTime -= fixedLogicFrameTime;
                steps++;
            }

            if (steps >= globalSettings.MaxStepsPerFrame && accumulatedTime >= fixedLogicFrameTime)
            {
                Debug.LogWarning($"Logic update accumulated {accumulatedTime / fixedLogicFrameTime} steps.");
            }
        }

        private void UpdateLogicTime(double fixedLogicFrameTime)
        {
            HNLogicTime.Time += fixedLogicFrameTime;
            HNLogicTime.DeltaTime = fixedLogicFrameTime;
            HNLogicTime.LogicFrameCount++;
        }


        protected HNUnityFrameworkGlobalSettings globalSettings;

        protected double fixedLogicFrameTime;
        protected double accumulatedTime;
        protected double currentTime;
    }
}
