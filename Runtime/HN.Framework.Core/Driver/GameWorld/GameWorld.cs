using HN.Framework.Capability.Core.Network;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Debug;
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
        }

        public void LateTick()
        {
            PoolManager.LateTick();
            ProcedureManager.LateTick();
            ControllerManager.LateTick();
            AssetManager?.LateTick();
        }
    }
}
