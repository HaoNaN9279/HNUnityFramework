using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Driver;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Unity.Capability.Input;
using HN.Framework.Unity.Capability.Network;
using HN.Framework.Unity.Capability.Physics;
using HN.Framework.Unity.Level.View.UI;
using HN.Framework.Unity.Driver.Platform;
using HN.Framework.Unity.Capability.Asset;
using HN.Framework.Unity.Driver.Platform.Log;
using HN.Framework.Unity.Capability.Camera;
using HN.Framework.Unity.Capability.Localization;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Driver.Platform
{
    public class GameWorldDriver : MonoBehaviour
    {
        public GameWorld World { get; private set; }

        /// <summary>
        /// 帧同步管理器引用，用于生命周期管理。
        /// </summary>
        private LockstepManager m_lockstepManager;

        /// <summary>
        /// FishNet 消息总线引用，用于生命周期管理。
        /// </summary>
        private FishNetMessageBus m_fishNetMessageBus;

        /// <summary>
        /// 帧同步网络驱动引用，用于生命周期管理。
        /// </summary>
        private LockstepNetworkDriver m_lockstepDriver;

        /// <summary>
        /// 本地化管理器引用，用于生命周期管理。
        /// </summary>
        private LocaleManager? _localeManager;

        protected virtual void Awake()
        {
            World = new GameWorld();
            World.LogProvider = new UnityLogProvider();

            var assetManager = new AssetManager();
#if UNITY_EDITOR
            assetManager.SetOperator(new AssetDatabaseOperator());
#else
            assetManager.SetOperator(new AddressablesOperator());
#endif
            World.AssetManager = assetManager;

            // 创建 InputManager，使用空的 InputActionAsset 作为初始配置
            // 运行时可通过 World.InputManager 属性替换为完整的 InputActionAsset 实例
            var emptyInputAsset = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            World.InputManager = new InputManager(emptyInputAsset);

            // 创建 UIManager，初始化 7 层 Canvas
            var uiManager = new UIManager();
            uiManager.Initialize();
            World.UIManager = uiManager;

            // 创建 LocaleManager，注入默认语言和可用语言列表
            // 项目代码可通过重写 OnRegisterGameModules 或直接替换 World.LocaleProvider 自定义
            var defaultLocales = new List<Core.Capability.Localization.Locale>
            {
                Core.Capability.Localization.Locale.zhCN,
                Core.Capability.Localization.Locale.enUS,
                Core.Capability.Localization.Locale.jaJP,
                Core.Capability.Localization.Locale.koKR,
                Core.Capability.Localization.Locale.zhTW,
            };
            var localeLoader = new AddressableStringTableLoader();
            _localeManager = new LocaleManager(localeLoader, defaultLocales, Core.Capability.Localization.Locale.zhCN);
            World.LocaleProvider = _localeManager;

            // 创建 PhysX 物理世界（默认 3D 模式）
            World.PhysicsWorld = new PhysXWorld(PhysicsDimension.D3);

            // 创建摄像机管理器，自动查找场景中的 CinemachineBrain
            var brain = FindObjectOfType<CinemachineBrain>();
            if (brain != null)
            {
                var cameraManager = new CameraManager(brain);
                World.CameraManager = cameraManager;
            }

            // 初始化帧同步（可通过 World.FrameSyncManager 属性替换为自定义实现）
            InitializeFrameSync();

            OnRegisterGameModules(World);
            World.Initialize();
        }

        /// <summary>
        /// 初始化帧同步系统。创建 <see cref="LockstepManager"/> 和 <see cref="LockstepNetworkDriver"/>，
        /// 注入到 <see cref="World"/>。
        /// 子类可重写此方法以自定义帧同步配置（如修改帧率、缓冲大小或替换实现）。
        /// </summary>
        protected virtual void InitializeFrameSync()
        {
            m_lockstepManager = new LockstepManager(); // 默认帧率 15，缓冲 3
            m_fishNetMessageBus = new FishNetMessageBus();
            m_lockstepDriver = new LockstepNetworkDriver(m_lockstepManager, m_fishNetMessageBus);
            m_lockstepDriver.Initialize();
            World.FrameSyncManager = m_lockstepManager;
        }

        protected virtual void OnRegisterGameModules(GameWorld world) { }

        /// <summary>
        /// 销毁时释放相关资源。
        /// </summary>
        protected virtual void OnDestroy()
        {
            m_lockstepDriver?.Shutdown();
            m_fishNetMessageBus?.Dispose();
            (World?.InputManager as IDisposable)?.Dispose();
            (World?.UIManager as IDisposable)?.Dispose();
            (World?.PhysicsWorld as IDisposable)?.Dispose();
            (World?.CameraManager as IDisposable)?.Dispose();
            _localeManager?.Dispose();
        }

        protected virtual void Update()
        {
            World.Tick();
        }

        protected virtual void LateUpdate()
        {
            World.LateTick();
        }
    }
}
