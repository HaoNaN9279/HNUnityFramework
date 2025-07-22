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
            // AssetManager初始化
            AssetManager.Initialize();

            // FSMManager初始化
            FSMManager.Initialize();

            // ProcedureManager初始化
            ProcedureManager.Initialize();

            // ControllerManager初始化
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
            // ControllerManager销毁
            ControllerManager.Uninitialize();

            // 关闭ProcedureManager
            ProcedureManager.ShutdownProcedure();

            // ProcedureManager销毁
            ProcedureManager.Uninitialize();

            // FSMManager销毁
            FSMManager.Uninitialize();

            // AssetManager销毁
            AssetManager.Uninitialize();
        }
        #endregion

        public void Tick()
        {
            // AssetManager更新
            AssetManager.TickAssetManager();

            // FSMManager更新
            FSMManager.TickFSMManager();

            // ProcedureManager更新
            ProcedureManager.TickProcedureManager();

            // ControllerManager更新
            ControllerManager.TickControllerManager();
        }

        public void LateTick()
        {
            // AssetManager更新
            AssetManager.LateTickAssetManager();

            // FSMManager更新
            FSMManager.LateTickFSMManager();

            // ProcedureManager更新
            ProcedureManager.LateTickProcedureManager();

            // ControllerManager更新
            ControllerManager.LateTickControllerManager();
        }



    }
}
