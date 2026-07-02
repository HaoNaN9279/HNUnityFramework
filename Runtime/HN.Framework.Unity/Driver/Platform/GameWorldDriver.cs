using HN.Framework.Core.Driver;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Unity.Driver.Platform;
using HN.Framework.Unity.Capability.Asset;
using HN.Framework.Unity.Driver.Platform.Log;
using UnityEngine;

namespace HN.Framework.Unity.Driver.Platform
{
    public class GameWorldDriver : MonoBehaviour
    {
        public GameWorld World { get; private set; }

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

            OnRegisterGameModules(World);
            World.Initialize();
        }

        protected virtual void OnRegisterGameModules(GameWorld world) { }

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
