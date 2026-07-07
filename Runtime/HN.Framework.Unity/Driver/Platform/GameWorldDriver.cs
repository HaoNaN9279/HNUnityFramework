using System;
using HN.Framework.Core.Driver;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Unity.Capability.Input;
using HN.Framework.Unity.Driver.Platform;
using HN.Framework.Unity.Capability.Asset;
using HN.Framework.Unity.Driver.Platform.Log;
using UnityEngine;
using UnityEngine.InputSystem;

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

            // 创建 InputManager，使用空的 InputActionAsset 作为初始配置
            // 运行时可通过 World.InputManager 属性替换为完整的 InputActionAsset 实例
            var emptyInputAsset = InputActionAsset.FromJson("{\"maps\":[],\"controlSchemes\":[]}");
            World.InputManager = new InputManager(emptyInputAsset);

            OnRegisterGameModules(World);
            World.Initialize();
        }

        protected virtual void OnRegisterGameModules(GameWorld world) { }

        /// <summary>
        /// 销毁时释放 InputManager 资源。
        /// </summary>
        protected virtual void OnDestroy()
        {
            (World?.InputManager as IDisposable)?.Dispose();
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
