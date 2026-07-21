using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HN.Framework.Unity.Capability.Cutscene
{
    /// <summary>
    /// CutsceneActor 自注册表。
    /// CutsceneActor 在 OnEnable 时主动注册，OnDisable 时注销。
    /// 提供 O(1) 字典查找替代 GameObject.Find / FindObjectsByType 的 O(n) 全场景扫描。
    /// 场景卸载时自动清理该场景相关注册项。
    /// </summary>
    public static class CutsceneActorRegistry
    {
        private static readonly Dictionary<string, GameObject> s_roleMap = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> s_tagMap = new Dictionary<string, GameObject>();
        private static readonly Dictionary<string, GameObject> s_entityMap = new Dictionary<string, GameObject>();

        static CutsceneActorRegistry()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>
        /// 注册一个 CutsceneActor 到全局注册表。
        /// 分别在 Role、Tag、EntityId 三个维度建立索引。
        /// 同名冲突时后者覆盖前者并输出警告。
        /// </summary>
        public static void Register(CutsceneActor actor)
        {
            if (actor == null || actor.gameObject == null)
                return;

            if (!string.IsNullOrEmpty(actor.ActorRole))
            {
                if (s_roleMap.ContainsKey(actor.ActorRole))
                    UnityEngine.Debug.LogWarning("[CutsceneActorRegistry] Duplicate ActorRole '" + actor.ActorRole + "' registered. Overwriting previous binding.");
                s_roleMap[actor.ActorRole] = actor.gameObject;
            }

            if (!string.IsNullOrEmpty(actor.ActorTag))
            {
                if (s_tagMap.ContainsKey(actor.ActorTag))
                    UnityEngine.Debug.LogWarning("[CutsceneActorRegistry] Duplicate ActorTag '" + actor.ActorTag + "' registered. Overwriting previous binding.");
                s_tagMap[actor.ActorTag] = actor.gameObject;
            }

            if (!string.IsNullOrEmpty(actor.EntityId))
            {
                if (s_entityMap.ContainsKey(actor.EntityId))
                    UnityEngine.Debug.LogWarning("[CutsceneActorRegistry] Duplicate EntityId '" + actor.EntityId + "' registered. Overwriting previous binding.");
                s_entityMap[actor.EntityId] = actor.gameObject;
            }
        }

        /// <summary>
        /// 从全局注册表中注销一个 CutsceneActor。
        /// 仅当字典中当前值等于该 actor 的 GameObject 时才移除（防止覆盖后被误删）。
        /// </summary>
        public static void Unregister(CutsceneActor actor)
        {
            if (actor == null)
                return;

            if (!string.IsNullOrEmpty(actor.ActorRole)
                && s_roleMap.TryGetValue(actor.ActorRole, out var roleGo)
                && roleGo == actor.gameObject)
            {
                s_roleMap.Remove(actor.ActorRole);
            }

            if (!string.IsNullOrEmpty(actor.ActorTag)
                && s_tagMap.TryGetValue(actor.ActorTag, out var tagGo)
                && tagGo == actor.gameObject)
            {
                s_tagMap.Remove(actor.ActorTag);
            }

            if (!string.IsNullOrEmpty(actor.EntityId)
                && s_entityMap.TryGetValue(actor.EntityId, out var entityGo)
                && entityGo == actor.gameObject)
            {
                s_entityMap.Remove(actor.EntityId);
            }
        }

        /// <summary>通过 ActorRole 查找 GameObject。O(1)。</summary>
        public static bool TryResolveByRole(string role, out GameObject go)
        {
            return s_roleMap.TryGetValue(role, out go);
        }

        /// <summary>通过 ActorTag 查找 GameObject。O(1)。</summary>
        public static bool TryResolveByTag(string tag, out GameObject go)
        {
            return s_tagMap.TryGetValue(tag, out go);
        }

        /// <summary>通过 EntityId 查找 GameObject。O(1)。</summary>
        public static bool TryResolveByEntity(string entityId, out GameObject go)
        {
            return s_entityMap.TryGetValue(entityId, out go);
        }

        /// <summary>清空所有注册项（用于测试环境重置）。</summary>
        public static void Clear()
        {
            s_roleMap.Clear();
            s_tagMap.Clear();
            s_entityMap.Clear();
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            RemoveSceneEntries(s_roleMap, scene);
            RemoveSceneEntries(s_tagMap, scene);
            RemoveSceneEntries(s_entityMap, scene);
        }

        private static void RemoveSceneEntries(Dictionary<string, GameObject> map, Scene scene)
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in map)
            {
                if (kvp.Value == null || kvp.Value.scene == scene)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
                map.Remove(key);
        }
    }
}
