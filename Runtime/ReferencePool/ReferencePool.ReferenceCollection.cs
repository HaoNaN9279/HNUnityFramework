using System;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
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
                references = new Queue<IReference>();
                this.referenceType = referenceType;
            }

            /// <summary>
            /// 从引用集合中请求一个对象
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != referenceType)
                {
                    Debug.LogError("Type is invalid.");
                }

                lock (references)
                {
                    if (references.Count > 0)
                    {
                        return (T)references.Dequeue();
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
                lock (references)
                {
                    if (references.Count > 0)
                    {
                        return references.Dequeue();
                    }
                }

                return (IReference)Activator.CreateInstance(referenceType);
            }

            /// <summary>
            /// 回收对象到引用集合
            /// </summary>
            /// <param name="reference"></param>
            public void Release(IReference reference)
            {
                reference.Clear();
                lock (references)
                {
                    if (references.Contains(reference))
                    {
                        Debug.LogError("The reference has been released.");
                    }

                    references.Enqueue(reference);
                }
            }

            /// <summary>
            /// 添加指定数量的对象到引用集合
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="count"></param>
            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != referenceType)
                {
                    Debug.LogError("Type is invalid.");
                }

                lock (references)
                {
                    while (count-- > 0)
                    {
                        references.Enqueue(new T());
                    }
                }
            }

            /// <summary>
            /// 添加指定数量的对象到引用集合
            /// </summary>
            /// <param name="count"></param>
            public void Add(int count)
            {
                lock (references)
                {
                    while (count-- > 0)
                    {
                        references.Enqueue((IReference)Activator.CreateInstance(referenceType));
                    }
                }
            }

            /// <summary>
            /// 从引用集合中移除指定数量的对象
            /// </summary>
            /// <param name="count"></param>
            public void Remove(int count)
            {
                lock (references)
                {
                    if (count > references.Count)
                    {
                        count = references.Count;
                    }

                    while (count-- > 0)
                    {
                        references.Dequeue();
                    }
                }
            }

            /// <summary>
            /// 清空引用集合
            /// </summary>
            public void RemoveAll()
            {
                lock (references)
                {
                    references.Clear();
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
                    return referenceType;
                }
            }

            /// <summary>
            /// 引用集合中的对象数量
            /// </summary>
            public int UnusedReferenceCount
            {
                get
                {
                    return references.Count;
                }
            }

            private readonly Queue<IReference> references;
            private readonly Type referenceType;
            #endregion

        }
    }
}
