using UnityEngine;

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

        private void Reset()
        {
            ActorRole = gameObject.name;
        }

        private void OnEnable()
        {
            CutsceneActorRegistry.Register(this);
        }

        private void OnDisable()
        {
            CutsceneActorRegistry.Unregister(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, new Vector3(1f, 1.8f, 0.5f));
            Gizmos.DrawIcon(transform.position + Vector3.up * 2.5f, "Animation.FilterBySelection", true);
        }
    }
}