using HN.Framework.Capability.Core.Network;
using HN.Framework.Core.Capability;
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

        // 平台适配接口（由 GameWorldDriver 注入）
        public ILogProvider LogProvider { get; set; }
        public IAssetManager? AssetManager { get; set; }
        public INetworkManager NetworkManager { get; set; }
        public IStorageProvider StorageProvider { get; set; }

        public GameWorld()
        {
            PoolManager = new ObjectPoolManager();
            ProcedureManager = new ProcedureManager();
            ControllerManager = new ControllerManager();
            EventBus = new EventBus();
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
