using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HN.Framework
{
    /// <summary>
    /// 游戏框架类
    /// </summary>
    public abstract class HNUnityFramework : MonoBehaviour, ITickable
    {
        #region Unity生命周期
        private void Awake()
        {
            OnAwake();
        }

        /// <summary>
        /// 框架初始化时调用
        /// </summary>
        protected virtual void OnAwake()
        {
            HNLogicTime.Initialize();
            ObjectPoolManager.Initialize();
            ProcedureManager.Initialize();
            ControllerManager.Initialize();
        }

        private void Start()
        {
            LoadGlobalSetting();
            OnStart();
        }

        /// <summary>
        /// 框架启动时调用
        /// </summary>
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

        private void Update()
        {
            OnUpdate();
        }

        /// <summary>
        /// 每帧更新逻辑
        /// </summary>
        protected virtual void OnUpdate()
        {
            LogicTimeUpdate(Tick);
        }

        private void LateUpdate()
        {
            OnLateUpdate();
        }

        /// <summary>
        /// 晚帧更新逻辑
        /// </summary>
        protected virtual void OnLateUpdate()
        {
            LogicTimeUpdate(LateTick);
        }

        private void OnDestroy()
        {
            ControllerManager.Uninitialize();
            ProcedureManager.ShutdownProcedure();
            ProcedureManager.Uninitialize();
            ObjectPoolManager.Uninitialize();
        }
        #endregion

        public void Tick()
        {
            ObjectPoolManager.TickObjectPoolManager();
            ProcedureManager.TickProcedureManager();
            ControllerManager.TickControllerManager();
        }

        public void LateTick()
        {
            ObjectPoolManager.LateTickObjectPoolManager();
            ProcedureManager.LateTickProcedureManager();
            ControllerManager.LateTickControllerManager();
        }

        private void LoadGlobalSetting()
        {
            var handle = Addressables.LoadAssetAsync<HNUnityFrameworkGlobalSettings>(HNUnityFrameworkGlobalSettings.GetGlobalSettingsLoadPath());
            globalSettings = handle.WaitForCompletion();
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


        /// <summary>
        /// 全局设置实例
        /// </summary>
        protected HNUnityFrameworkGlobalSettings globalSettings;

        /// <summary>
        /// 固定逻辑帧间隔时间
        /// </summary>
        protected double fixedLogicFrameTime;
        /// <summary>
        /// 累积逻辑帧时间
        /// </summary>
        protected double accumulatedTime;
        /// <summary>
        /// 当前运行时间
        /// </summary>
        protected double currentTime;
    }
}
