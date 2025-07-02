using System;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public partial class ReferencePool
    {
        public static int Count
        {
            get
            {
                return referenceCollections.Count;
            }
        }
        

        private static readonly Dictionary<Type, ReferenceCollection> referenceCollections = new Dictionary<Type, ReferenceCollection>();


        public static T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        public static IReference Acquire(Type referenceType)
        {
            return GetReferenceCollection(referenceType).Acquire();
        }

        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                Debug.LogError("Reference is invalid.");
            }

            Type referenceType = reference.GetType();
            GetReferenceCollection(referenceType).Release(reference);
        }

        public static void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        public static void Add(Type referenceType, int count)
        {
            GetReferenceCollection(referenceType).Add(count);
        }

        public static void Remove<T>(int count) where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        public static void Remove(Type referenceType, int count)
        {
            GetReferenceCollection(referenceType).Remove(count);
        }

        public static void RemoveAll<T>() where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        public static void RemoveAll(Type referenceType)
        {
            GetReferenceCollection(referenceType).RemoveAll();
        }

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

        private static ReferenceCollection GetReferenceCollection(Type referenceType)
        {
            if (referenceType == null)
            {
                Debug.LogError("ReferenceType is invalid.");
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
    }
}
