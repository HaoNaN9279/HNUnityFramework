using System.Collections.Generic;
using HN.Framework.Core.Capability.Cutscene;
using UnityEngine;
using UnityEngine.Playables;

namespace HN.Framework.Unity.Capability.Cutscene
{
    public class CutsceneBindingResolver : IExposedPropertyTable
    {
        private readonly CutsceneBindingMap _bindingMap;
        private readonly Dictionary<PropertyName, Object> _resolvedCache = new Dictionary<PropertyName, Object>();

        public CutsceneBindingResolver(CutsceneBindingMap bindingMap)
        {
            _bindingMap = bindingMap;
        }

        public void ClearCache()
        {
            _resolvedCache.Clear();
        }

        public void SetReferenceValue(PropertyName id, Object value)
        {
            _resolvedCache[id] = value;
        }

        public Object GetReferenceValue(PropertyName id, out bool idValid)
        {
            if (_resolvedCache.TryGetValue(id, out var cached))
            {
                idValid = true;
                return cached;
            }

            var roleName = ParsePropertyName(id.ToString());
            if (string.IsNullOrEmpty(roleName) || !_bindingMap.TryGetBinding(roleName, out var targetId))
            {
                idValid = false;
                return null;
            }

            Object resolved = ResolveBinding(roleName, targetId);
            if (resolved != null)
            {
                _resolvedCache[id] = resolved;
                idValid = true;
                return resolved;
            }

            idValid = false;
            return null;
        }

        public IEnumerable<PropertyName> GetReferenceNames()
        {
            var names = new List<PropertyName>();
            foreach (var kvp in _bindingMap.Bindings)
            {
                names.Add(new PropertyName(kvp.Key));
            }
            return names;
        }

        private Object ResolveBinding(string roleName, string targetId)
        {
            switch (_bindingMap.ResolveMode)
            {
                case BindingResolveMode.ScenePath:
                {
                    // 优先通过注册表按角色名 O(1) 查找
                    if (CutsceneActorRegistry.TryResolveByRole(roleName, out var go))
                        return go;
                    // 尝试按 targetId 查找（targetId 可能恰好与 ActorRole 同名）
                    if (CutsceneActorRegistry.TryResolveByRole(targetId, out go))
                        return go;
                    // Fallback：保留旧方式以兼容未挂 CutsceneActor 组件的对象
                    return GameObject.Find(targetId);
                }
                case BindingResolveMode.Tag:
                {
                    // 优先通过注册表的 Tag 索引 O(1) 查找
                    if (CutsceneActorRegistry.TryResolveByTag(targetId, out var go))
                        return go;
                    // Fallback：引擎内部的 Tag 索引也是 O(1)
                    return GameObject.FindWithTag(targetId);
                }
                case BindingResolveMode.EntityId:
                    if (CutsceneActorRegistry.TryResolveByEntity(targetId, out var entityGo))
                        return entityGo;
                    // Fallback
                    return FindActorByEntityId(targetId);
                case BindingResolveMode.ActorComponent:
                    if (CutsceneActorRegistry.TryResolveByTag(targetId, out var actorGo))
                        return actorGo;
                    // Fallback
                    return FindActorByTag(targetId);
                default:
                    return null;
            }
        }

        private static Object FindActorByEntityId(string entityId)
        {
            var actors = Object.FindObjectsByType<CutsceneActor>(FindObjectsSortMode.None);
            foreach (var actor in actors)
            {
                if (actor.EntityId == entityId)
                    return actor.gameObject;
            }
            return null;
        }

        private static Object FindActorByTag(string actorTag)
        {
            var actors = Object.FindObjectsByType<CutsceneActor>(FindObjectsSortMode.None);
            foreach (var actor in actors)
            {
                if (actor.ActorTag == actorTag)
                    return actor.gameObject;
            }
            return null;
        }
    public void ClearReferenceValue(PropertyName id)
        {
            _resolvedCache.Remove(id);
        }
    private static string ParsePropertyName(string propertyString)
        {
            if (string.IsNullOrEmpty(propertyString)) return null;
            var colonIndex = propertyString.LastIndexOf(':');
            return colonIndex > 0 ? propertyString.Substring(0, colonIndex) : propertyString;
        }
    }
}