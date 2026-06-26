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


        [SerializeField] private ObjectPoolManager m_manager;
        [SerializeField] private IReadOnlyDictionary<string, PoolBase> m_pools;

        public ObjectPoolManager Manager => m_manager;
        public IReadOnlyDictionary<string, PoolBase> Pools => m_pools;
    }
}
#endif
