using System;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> references;
            private readonly Type referenceType;

            public ReferenceCollection(Type referenceType)
            {
                references = new Queue<IReference>();
                this.referenceType = referenceType;
            }

            public Type ReferenceType
            {
                get
                {
                    return referenceType;
                }
            }

            public int UnusedReferenceCount
            {
                get
                {
                    return references.Count;
                }
            }

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

            public void RemoveAll()
            {
                lock (references)
                {
                    references.Clear();
                }
            }
        }
    }
}
