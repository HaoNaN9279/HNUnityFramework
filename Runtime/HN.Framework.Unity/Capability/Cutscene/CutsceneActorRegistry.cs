using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HN.Framework.Unity.Capability.Cutscene
{
    /// <summary>
    /// <see cref="ICutsceneActorRegistry"/> 的默认实现（实例注册表）。
    /// CutsceneActor 在 OnEnable 时主动注册，OnDisable 时注销。
    /// 提供 O(1) 字典查找替代 GameObject.Find / FindObjectsByType 的 O(n) 全场景扫描。
    /// 场景卸载时自动清理该场景相关注册项。
    /// </summary>
    /// <remarks>
    /// 实例由 <see cref="CutsceneManager"/> 创建并持有，通过构造函数注入给
    /// <see cref="CutsceneBindingResolver"/>。销毁时调用 <see cref="Dispose"/>
    /// 解除 SceneManager 事件订阅，避免域重载/多次创建时的残留与泄漏。
    /// </remarks>
    public sealed class CutsceneActorRegistry : ICutsceneActorRegistry, IDisposable
    {
        private readonly Dictionary<string, GameObject> _roleMap = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _tagMap = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _entityMap = new Dictionary<string, GameObject>();
        private bool _disposed;

        public CutsceneActorRegistry()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <inheritdoc/>
        public void Register(CutsceneActor actor)
        {
            if (actor == null || actor.gameObject == null)
                return;

            if (!string.IsNullOrEmpty(actor.ActorRole))
            {
                if (_roleMap.ContainsKey(actor.ActorRole))
                    UnityEngine.Debug.LogWarning("[CutsceneActorRegistry] Duplicate ActorRole '" + actor.ActorRole + "' registered. Overwriting previous binding.");
                _roleMap[actor.ActorRole] = actor.gameObject;
            }

            if (!string.IsNullOrEmpty(actor.ActorTag))
            {
                if (_tagMap.ContainsKey(actor.ActorTag))
                    UnityEngine.Debug.LogWarning("[CutsceneActorRegistry] Duplicate ActorTag '" + actor.ActorTag + "' registered. Overwriting previous binding.");
                _tagMap[actor.ActorTag] = actor.gameObject;
            }

            if (!string.IsNullOrEmpty(actor.EntityId))
            {
                if (_entityMap.ContainsKey(actor.EntityId))
                    UnityEngine.Debug.LogWarning("[CutsceneActorRegistry] Duplicate EntityId '" + actor.EntityId + "' registered. Overwriting previous binding.");
                _entityMap[actor.EntityId] = actor.gameObject;
            }
        }

        /// <inheritdoc/>
        public void Unregister(CutsceneActor actor)
        {
            if (actor == null)
                return;

            if (!string.IsNullOrEmpty(actor.ActorRole)
                && _roleMap.TryGetValue(actor.ActorRole, out var roleGo)
                && roleGo == actor.gameObject)
            {
                _roleMap.Remove(actor.ActorRole);
            }

            if (!string.IsNullOrEmpty(actor.ActorTag)
                && _tagMap.TryGetValue(actor.ActorTag, out var tagGo)
                && tagGo == actor.gameObject)
            {
                _tagMap.Remove(actor.ActorTag);
            }

            if (!string.IsNullOrEmpty(actor.EntityId)
                && _entityMap.TryGetValue(actor.EntityId, out var entityGo)
                && entityGo == actor.gameObject)
            {
                _entityMap.Remove(actor.EntityId);
            }
        }

        /// <inheritdoc/>
        public bool TryResolveByRole(string role, out GameObject go)
        {
            return _roleMap.TryGetValue(role, out go);
        }

        /// <inheritdoc/>
        public bool TryResolveByTag(string tag, out GameObject go)
        {
            return _tagMap.TryGetValue(tag, out go);
        }

        /// <inheritdoc/>
        public bool TryResolveByEntity(string entityId, out GameObject go)
        {
            return _entityMap.TryGetValue(entityId, out go);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _roleMap.Clear();
            _tagMap.Clear();
            _entityMap.Clear();
        }

        /// <summary>
        /// 解除 SceneManager 事件订阅并清空注册项。
        /// 幂等；调用后不应再使用该实例。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Clear();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            RemoveSceneEntries(_roleMap, scene);
            RemoveSceneEntries(_tagMap, scene);
            RemoveSceneEntries(_entityMap, scene);
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
