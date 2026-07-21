using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Driver.Common.Pool.ReferencePool
{
    public sealed partial class ReferencePool
    {
        /// <summary>
        /// 引用池的引用集合类
        /// </summary>
        private sealed class ReferenceCollection
        {
            #region 对外函数
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="referenceType"></param>
            public ReferenceCollection(Type referenceType)
            {
                m_references = new Queue<IReference>();
                this.m_referenceType = referenceType;
            }

            /// <summary>
            /// 从引用集合中请求一个对象
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != m_referenceType)
                {
                    throw new InvalidOperationException("Type is invalid.");
                }

                lock (m_references)
                {
                    if (m_references.Count > 0)
                    {
                        return (T)m_references.Dequeue();
                    }
                }

                return new T();
            }

            /// <summary>
            /// 从引用集合中请求一个对象
            /// </summary>
            /// <returns></returns>
            public IReference Acquire()
            {
                lock (m_references)
                {
                    if (m_references.Count > 0)
                    {
                        return m_references.Dequeue();
                    }
                }

                return (IReference)Activator.CreateInstance(m_referenceType);
            }

            /// <summary>
            /// 回收对象到引用集合
            /// </summary>
            /// <param name="reference"></param>
            public void Release(IReference reference)
            {
                reference.Clear();
                lock (m_references)
                {
                    if (m_references.Contains(reference))
                    {
                        throw new InvalidOperationException("The reference has been released.");
                    }

                    m_references.Enqueue(reference);
                }
            }

            /// <summary>
            /// 添加指定数量的对象到引用集合
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="count"></param>
            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != m_referenceType)
                {
                    throw new InvalidOperationException("Type is invalid.");
                }

                lock (m_references)
                {
                    while (count-- > 0)
                    {
                        m_references.Enqueue(new T());
                    }
                }
            }

            /// <summary>
            /// 添加指定数量的对象到引用集合
            /// </summary>
            /// <param name="count"></param>
            public void Add(int count)
            {
                lock (m_references)
                {
                    while (count-- > 0)
                    {
                        m_references.Enqueue((IReference)Activator.CreateInstance(m_referenceType));
                    }
                }
            }

            /// <summary>
            /// 从引用集合中移除指定数量的对象
            /// </summary>
            /// <param name="count"></param>
            public void Remove(int count)
            {
                lock (m_references)
                {
                    if (count > m_references.Count)
                    {
                        count = m_references.Count;
                    }

                    while (count-- > 0)
                    {
                        m_references.Dequeue();
                    }
                }
            }

            /// <summary>
            /// 清空引用集合
            /// </summary>
            public void RemoveAll()
            {
                lock (m_references)
                {
                    m_references.Clear();
                }
            }
            #endregion

            #region 对外属性
            /// <summary>
            /// 引用集合的类型
            /// </summary>
            public Type ReferenceType
            {
                get
                {
                    return m_referenceType;
                }
            }

            /// <summary>
            /// 引用集合中的对象数量
            /// </summary>
            public int UnusedReferenceCount
            {
                get
                {
                    return m_references.Count;
                }
            }

            private readonly Queue<IReference> m_references;
            private readonly Type m_referenceType;
            #endregion

        }
    }
}
