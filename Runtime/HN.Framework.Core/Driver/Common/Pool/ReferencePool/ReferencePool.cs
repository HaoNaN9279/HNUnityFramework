using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Driver.Common.Pool.ReferencePool
{
    /// <summary>
    /// 引用池
    /// 线程安全
    /// </summary>
    public sealed partial class ReferencePool
    {
        #region 对外函数
        /// <summary>
        /// 从引用池中请求一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        /// <summary>
        /// 从引用池中请求一个对象
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        public static IReference Acquire(Type referenceType)
        {
            return GetReferenceCollection(referenceType).Acquire();
        }

        /// <summary>
        /// 回收对象到引用池
        /// </summary>
        /// <param name="reference"></param>
        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                throw new InvalidOperationException("Reference is invalid.");
            }

            Type referenceType = reference.GetType();
            GetReferenceCollection(referenceType).Release(reference);
        }

        /// <summary>
        /// 添加指定数量的对象到引用池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        public static void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 添加指定数量的对象到引用池
        /// </summary>
        /// <param name="referenceType"></param>
        /// <param name="count"></param>
        public static void Add(Type referenceType, int count)
        {
            GetReferenceCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        public static void Remove<T>(int count) where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的对象
        /// </summary>
        /// <param name="referenceType"></param>
        /// <param name="count"></param>
        public static void Remove(Type referenceType, int count)
        {
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 移除所有指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 移除所有指定类型的对象
        /// </summary>
        /// <param name="referenceType"></param>
        public static void RemoveAll(Type referenceType)
        {
            GetReferenceCollection(referenceType).RemoveAll();
        }

        /// <summary>
        /// 清除所有引用池中的对象
        /// </summary>
        public static void ClearAll()
        {
            lock (referenceCollections)
            {
                foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in referenceCollections)
                {
                    referenceCollection.Value.RemoveAll();
                }

                referenceCollections.Clear();
            }
        }
        #endregion


        #region 对内函数
        /// <summary>
        /// 获取指定类型的引用集合
        /// 如果不存在则创建一个新的引用集合
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        private static ReferenceCollection GetReferenceCollection(Type referenceType)
        {
            if (referenceType == null)
            {
                throw new InvalidOperationException("ReferenceType is invalid.");
            }

            ReferenceCollection referenceCollection = null;
            lock (referenceCollections)
            {
                if (!referenceCollections.TryGetValue(referenceType, out referenceCollection))
                {
                    referenceCollection = new ReferenceCollection(referenceType);
                    referenceCollections.Add(referenceType, referenceCollection);
                }
            }

            return referenceCollection;
        }
        #endregion


        #region 对外属性
        /// <summary>
        /// 引用池中的引用集合数
        /// </summary>
        public static int Count
        {
            get
            {
                return referenceCollections.Count;
            }
        }
        #endregion

        #region 对内字段
        /// <summary>
        /// 引用池中的引用集合字典
        /// 键为引用类型，值为引用集合
        /// </summary>
        private static readonly Dictionary<Type, ReferenceCollection> referenceCollections = new Dictionary<Type, ReferenceCollection>();
        #endregion
    }
}
