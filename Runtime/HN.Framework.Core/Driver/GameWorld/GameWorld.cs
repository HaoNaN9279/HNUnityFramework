#nullable enable
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Debug;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Capability.Localization;
using HN.Framework.Core.Capability.Physics;
using HN.Framework.Core.Capability.Camera;
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Core.Capability.UI;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Core.Level.Logic;
using HN.Framework.Core.Level.Logic.AI;
using HN.Framework.Core.Level.Logic.Entity;
using HN.Framework.Core.Level;

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

        /// <summary>
        /// AI 系统，管理所有 AIAgent 的注册与统一 Tick 驱动。
        /// AIAgentComponent 在 Awake 时通过此属性注册 Agent，GameWorld 负责每帧驱动。
        /// </summary>
        public AISystem AISystem { get; }

        /// <summary>
        /// GameplayTag 管理器，管理层级标签的注册、冻结和查询。
        /// 由 GameWorldDriver 在初始化时加载配置并调用 Freeze()。
        /// </summary>
        public GameplayTagManager GameplayTagManager { get; }

        public Capability.Debug.DebugHub DebugHub { get; }

        // 平台适配接口（由 GameWorldDriver 注入）
        private ILogProvider _logProvider = null!;
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
        public INetworkManager? NetworkManager { get; set; }
        public IStorageProvider? StorageProvider { get; set; }

        /// <summary>
        /// 帧同步管理器。启用后在 Tick 循环的最优先位置驱动逻辑帧 Tick。
        /// 由 GameWorldDriver 在初始化时注入，或由外部代码设置为自定义实现。
        /// </summary>
        public IFrameSyncManager? FrameSyncManager { get; set; }

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

        /// <summary>过场动画管理器（C12，由 GameWorldDriver 注入）</summary>
        public ICutsceneManager? CutsceneManager { get; set; }

        /// <summary>
        /// 任务管理器（L8，由项目代码注入）。
        /// 注入的实例应实现 <see cref="ITickable"/> 以支持超时检查等定时逻辑。
        /// 若注入实例实现了 ITickable，GameWorld 会自动驱动其 Tick/LateTick。
        /// 典型注入方式：在 <c>GameWorldDriver.OnRegisterGameModules()</c> 或 <c>GameEntry</c> 中设置，
        /// 例如 <c>world.QuestManager = new QuestManager&lt;int&gt;(...)</c>。
        /// </summary>
        public ITickable? QuestManager { get; set; }

        public GameWorld()
        {
            PoolManager = new ObjectPoolManager();
            ProcedureManager = new ProcedureManager();
            ControllerManager = new ControllerManager();
            EventBus = new EventBus();
            DebugHub = new Capability.Debug.DebugHub();
            EntityManager = new EntityManager(EventBus);
            GameplayTagManager = new GameplayTagManager();
            AISystem = new AISystem();
        }

        public void Initialize()
        {
            ProcedureManager.Initialize();
            ControllerManager.Initialize();
        }

        public void Tick()
        {
            // 帧同步在 Tick 首位驱动，确保逻辑帧先于其他所有模块更新
            (FrameSyncManager as ITickable)?.Tick();
            PoolManager.Tick();
            ProcedureManager.Tick();
            ControllerManager.Tick();
            AISystem.Tick();
            AssetManager?.Tick();
            // InputManager 是事件驱动的，Tick 仅当它实现 ITickable 时才生效
            (InputManager as ITickable)?.Tick();
            (UIManager as ITickable)?.Tick();
            (PhysicsWorld as ITickable)?.Tick();
            (CutsceneManager as ITickable)?.Tick();
            QuestManager?.Tick();
        }

        public void LateTick()
        {
            (FrameSyncManager as ITickable)?.LateTick();
            PoolManager.LateTick();
            ProcedureManager.LateTick();
            ControllerManager.LateTick();
            AISystem.LateTick();
            AssetManager?.LateTick();
            (InputManager as ITickable)?.LateTick();
            (UIManager as ITickable)?.LateTick();
            (PhysicsWorld as ITickable)?.LateTick();
            (CutsceneManager as ITickable)?.LateTick();
            QuestManager?.LateTick();
        }
    }
}
