using UnityEngine;
using HN.Framework.Unity.Driver.Platform;

namespace HN.Framework.Unity.Capability.Cutscene
{
    [AddComponentMenu("HN Framework/Cutscene/CutsceneActor")]
    public class CutsceneActor : MonoBehaviour
    {
        [Tooltip("角色名，用于 Timeline Track 绑定解析的键")]
        public string ActorRole;

        [Tooltip("Actor 标签，用于 ActorComponent 模式查找")]
        public string ActorTag;

        [Tooltip("关联的 EntityId（用于 EntityId 模式查找）")]
        public string EntityId;

        /// <summary>
        /// 当前注册到的注册表实例（由场景中的 GameWorldDriver→CutsceneManager 解析）。
        /// 与 OnEnable 解析的实例配对，确保 OnDisable 只从同一个注册表注销。
        /// </summary>
        private ICutsceneActorRegistry _registry;

        private void Reset()
        {
            ActorRole = gameObject.name;
        }

        private void OnEnable()
        {
            _registry = ResolveRegistry();
            _registry?.Register(this);
        }

        private void OnDisable()
        {
            _registry?.Unregister(this);
            _registry = null;
        }

        /// <summary>
        /// 通过场景中的 <see cref="GameWorldDriver"/> 解析其 <see cref="CutsceneManager"/>
        /// 所持有的演员注册表（与 AIAgentComponent 解析 GameWorld.AISystem 的方式一致）。
        /// 未找到时返回 null，此时不注册，绑定解析将回退到引擎全场景扫描。
        /// </summary>
        private static ICutsceneActorRegistry ResolveRegistry()
        {
            var driver = FindFirstObjectByType<GameWorldDriver>();
            var manager = driver != null ? driver.World?.CutsceneManager : null;
            return (manager as CutsceneManager)?.ActorRegistry;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(1f, 1.8f, 0.5f));
            Gizmos.DrawIcon(transform.position + Vector3.up * 2.5f, "Animation.FilterBySelection", true);
        }
    }
}
