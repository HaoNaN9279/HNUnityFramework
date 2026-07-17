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

            Object resolved = ResolveBinding(targetId);
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

        private Object ResolveBinding(string targetId)
        {
            switch (_bindingMap.ResolveMode)
            {
                case BindingResolveMode.ScenePath:
                    return GameObject.Find(targetId);
                case BindingResolveMode.Tag:
                    return GameObject.FindWithTag(targetId);
                case BindingResolveMode.EntityId:
                    return ResolveEntityBinding(targetId);
                case BindingResolveMode.ActorComponent:
                    return ResolveActorComponent(targetId);
                default:
                    return null;
            }
        }

        private Object ResolveEntityBinding(string entityId)
        {
            var actors = Object.FindObjectsByType<CutsceneActor>(FindObjectsSortMode.None);
            foreach (var actor in actors)
            {
                if (actor.EntityId == entityId)
                    return actor.gameObject;
            }
            return null;
        }

        private Object ResolveActorComponent(string actorTag)
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