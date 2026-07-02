using HN.Framework.Capability;
using HN.Framework.Capability.Pool;
using HN.Framework.Capability.Procedure;
using HN.Framework.Level.Logic;

namespace HN.Framework.Driver
{
    public class GameWorld : ITickable
    {
        // 框架内置模块
        public ObjectPoolManager PoolManager { get; }
        public ProcedureManager ProcedureManager { get; }
        public ControllerManager ControllerManager { get; }

        // 平台适配接口（由 GameWorldDriver 注入）
        public ILogProvider LogProvider { get; set; }
        public IAssetOperator AssetOperator { get; set; }
        public INetworkManager NetworkManager { get; set; }
        public IStorageProvider StorageProvider { get; set; }

        public GameWorld()
        {
            PoolManager = new ObjectPoolManager();
            ProcedureManager = new ProcedureManager();
            ControllerManager = new ControllerManager();
        }

        public void Initialize()
        {
            PoolManager.Tick();
            ProcedureManager.Tick();
            ControllerManager.Tick();
        }

        public void Tick()
        {
            PoolManager.Tick();
            ProcedureManager.Tick();
            ControllerManager.Tick();
        }

        public void LateTick()
        {
            PoolManager.LateTick();
            ProcedureManager.LateTick();
            ControllerManager.LateTick();
        }
    }
}
