#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HN.Framework
{
    /// <summary>
    /// 对象池信息查看器组件
    /// 只在编辑器下有效
    /// </summary>
    public class ObjectPoolViewer : MonoBehaviour
    {
        public void Initialize(ObjectPoolManager manager)
        {
            m_manager = manager;
        }

        void Update()
        {
            if (m_manager == null)
            {
                return;
            }

            m_pools = m_manager.ObjectPools;

        }


        public ObjectPoolManager m_manager;
        public IReadOnlyDictionary<string, PoolBase> m_pools;
    }
}
#endif
