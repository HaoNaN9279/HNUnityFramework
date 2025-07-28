using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HN.Framework.Editor
{
    /// <summary>
    /// 对象池信息查看器面板
    /// </summary>
    [CustomEditor(typeof(ObjectPoolViewer))]
    public class ObjectPoolViewerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            ObjectPoolViewer viewer = (ObjectPoolViewer)target;

            m_objectPoolFoldout = EditorGUILayout.Foldout(m_objectPoolFoldout, new GUIContent("Object Pools"));
            if (m_objectPoolFoldout)
            {
                foreach (var item in viewer.m_pools)
                {
                    if (IsSubclassOfRawGeneric(typeof(ObjectPool<>), item.Value.GetType()))
                    {
                        EditorGUILayout.BeginHorizontal();
                        var pool = item.Value;
                        EditorGUILayout.LabelField($"{pool.GetType().Name}[{pool.GetObjectType().Name}]");
                        EditorGUILayout.LabelField($"{pool.InitialCount}:{pool.MinLimitCount}|{pool.MinCount}--{pool.CurrentCount}--{pool.MaxCount}|{pool.MaxLimitCount}");
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            m_gameObjectPoolFoldout = EditorGUILayout.Foldout(m_gameObjectPoolFoldout, new GUIContent("GameObject Pools"));
            if (m_gameObjectPoolFoldout)
            {
                foreach (var item in viewer.m_pools)
                {
                    if (item.Value is GameObjectPool)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var pool = item.Value;
                        EditorGUILayout.LabelField($"{pool.GetType().Name}[{pool.GetObjectType().Name}]");
                        EditorGUILayout.LabelField($"{pool.InitialCount}:{pool.MinLimitCount}|{pool.MinCount}--{pool.CurrentCount}--{pool.MaxCount}|{pool.MaxLimitCount}");
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            if (Application.isPlaying)
            {
                Repaint();
            }
        }


        /// <summary>
        /// 判断类型derivedType是否为指定泛型类genericBase的子类
        /// </summary>
        /// <param name="genericBase"></param>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        private bool IsSubclassOfRawGeneric(Type genericBase, Type derivedType)
        {
            while (derivedType != null && derivedType != typeof(object))
            {
                if (derivedType.IsGenericType && derivedType.GetGenericTypeDefinition() == genericBase)
                {
                    return true;
                }

                derivedType = derivedType.BaseType;
            }
            return false;
        }

        private bool m_objectPoolFoldout = true;
        private bool m_gameObjectPoolFoldout = true;
    }
}
