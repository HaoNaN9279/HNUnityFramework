using HN.Framework.Capability.Core.Network;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Debug;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Core.Level.Logic;

namespace HN.Framework.Core.Driver
{
    public class GameWorld : ITickable
    {
        // 框架内置模块
        public ObjectPoolManager PoolManager { get; }
        public ProcedureManager ProcedureManager { get; }
        public ControllerManager ControllerManager { get; }
        public EventBus EventBus { get; }

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

        public GameWorld()
        {
            PoolManager = new ObjectPoolManager();
            ProcedureManager = new ProcedureManager();
            ControllerManager = new ControllerManager();
            EventBus = new EventBus();
            DebugHub = new Capability.Debug.DebugHub();
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
        }

        public void LateTick()
        {
            PoolManager.LateTick();
            ProcedureManager.LateTick();
            ControllerManager.LateTick();
            AssetManager?.LateTick();
            (InputManager as ITickable)?.LateTick();
        }
    }
}
