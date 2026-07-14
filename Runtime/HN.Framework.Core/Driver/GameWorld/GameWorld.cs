using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Debug;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Capability.Camera;
using HN.Framework.Core.Capability.UI;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Core.Level.Logic;
using HN.Framework.Core.Level.Logic.Entity;

namespace HN.Framework.Core.Driver
{
    public class GameWorld : ITickable
    {
        // 框架内置模块
        public ObjectPoolManager PoolManager { get; }
        public ProcedureManager ProcedureManager { get; }
        public ControllerManager ControllerManager { get; }
        public EventBus EventBus { get; }
        public EntityManager EntityManager { get; }

        public Capability.Debug.DebugHub DebugHub { get; }

        // 平台适配接口（由 GameWorldDriver 注入）
        private ILogProvider _logProvider;
        public ILogProvider LogProvider
        {
            get => _logProvider;
            set
            {
                _logProvider = value;
                DebugHub.SetLogProvider(value);
            }
        }
        public IAssetManager? AssetManager { get; set; }
        public INetworkManager NetworkManager { get; set; }
        public IStorageProvider StorageProvider { get; set; }

        /// <summary>
        /// 输入管理器。可通过属性替换以支持自定义实现。
        /// InputManager 是事件驱动的，因此无需 Tick 驱动；仅当实现 ITickable 时才由 GameWorld 调用 Tick/LateTick。
        /// </summary>
        public IInputManager? InputManager { get; set; }

        /// <summary>
        /// UI 管理器。可通过属性替换以支持自定义实现。
        /// 仅当实现 ITickable 时才由 GameWorld 调用 Tick/LateTick。
        /// </summary>
        public IUIManager? UIManager { get; set; }

        /// <summary>
        /// 本地化提供程序，负责多语言字符串查询和语言区域切换。
        /// 由 GameWorldDriver 在初始化时注入，或由外部代码设置为自定义实现。
        /// </summary>
        public ILocaleProvider? LocaleProvider { get; set; }

        /// <summary>
        /// 物理世界接口，负责管理所有物理刚体的生命周期以及执行物理查询。
        /// 由 GameWorldDriver 在初始化时注入，或由外部代码设置为自定义实现。
        /// 仅当实现 ITickable 时才由 GameWorld 调用 Tick/LateTick。
        /// </summary>
        public IPhysicsWorld? PhysicsWorld { get; set; }

        /// <summary>
        /// 摄像机管理器，负责管理和切换虚拟摄像机。
        /// CameraManager 包装 Cinemachine Brain，由 GameWorldDriver 在初始化时注入。
        /// CameraManager 不实现 ITickable，Cinemachine 自管理生命周期。
        /// </summary>
        public ICameraManager? CameraManager { get; set; }

        public GameWorld()
        {
            PoolManager = new ObjectPoolManager();
            ProcedureManager = new ProcedureManager();
            ControllerManager = new ControllerManager();
            EventBus = new EventBus();
            DebugHub = new Capability.Debug.DebugHub();
            EntityManager = new EntityManager(EventBus);
        }

        public void Initialize()
        {
            ProcedureManager.Initialize();
            ControllerManager.Initialize();
        }

        public void Tick()
        {
            PoolManager.Tick();
            ProcedureManager.Tick();
            ControllerManager.Tick();
            AssetManager?.Tick();
            // InputManager 是事件驱动的，Tick 仅当它实现 ITickable 时才生效
            (InputManager as ITickable)?.Tick();
            (UIManager as ITickable)?.Tick();
            (PhysicsWorld as ITickable)?.Tick();
        }

        public void LateTick()
        {
            PoolManager.LateTick();
            ProcedureManager.LateTick();
            ControllerManager.LateTick();
            AssetManager?.LateTick();
            (InputManager as ITickable)?.LateTick();
            (UIManager as ITickable)?.LateTick();
            (PhysicsWorld as ITickable)?.LateTick();
        }
    }
}
