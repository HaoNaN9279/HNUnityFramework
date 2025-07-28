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
            // 资源管理器初始化
            AssetManager.Initialize();

            // FSMManager初始化
            FSMManager.Initialize();

            // 对象池管理器初始化
            ObjectPoolManager.Initialize();

            // 流程管理器初始化
            ProcedureManager.Initialize();

            // Controller管理器初始化
            ControllerManager.Initialize();
        }

        void Start()
        {
            
        }

        void Update()
        {
            Tick();
        }

        void LateUpdate()
        {
            LateTick();
        }

        void OnDestroy()
        {
            // Controller管理器销毁
            ControllerManager.Uninitialize();

            // 关闭流程管理器
            ProcedureManager.ShutdownProcedure();

            // 流程管理器销毁
            ProcedureManager.Uninitialize();

            // 对象池管理器销毁
            ObjectPoolManager.Uninitialize();

            // FSMManager销毁
            FSMManager.Uninitialize();

            // 资源管理器销毁
            AssetManager.Uninitialize();
        }
        #endregion

        public void Tick()
        {
            // 资源管理器更新
            AssetManager.TickAssetManager();

            // FSMManager更新
            FSMManager.TickFSMManager();

            // 对象池管理器更新
            ObjectPoolManager.TickObjectPoolManager();

            // 流程管理器更新
            ProcedureManager.TickProcedureManager();

            // Controller管理器更新
            ControllerManager.TickControllerManager();
        }

        public void LateTick()
        {
            // 资源管理器更新
            AssetManager.LateTickAssetManager();

            // FSMManager更新
            FSMManager.LateTickFSMManager();

            ObjectPoolManager.LateTickObjectPoolManager();

            // 流程管理器更新
            ProcedureManager.LateTickProcedureManager();

            // Controller管理器更新
            ControllerManager.LateTickControllerManager();
        }



    }
}
