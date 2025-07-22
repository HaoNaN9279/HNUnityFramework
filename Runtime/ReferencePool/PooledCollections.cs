using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    public class PooledList<T> : List<T>, IReference where T : class
    {
    }

    public class PooledDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    public class PooledQueue<T> : Queue<T>, IReference where T : class
    {
    }

    public class PooledStack<T> : Stack<T>, IReference where T : class
    {
    }

    public class PooledHashSet<T> : HashSet<T>, IReference where T : class
    {
    }

    public class PooledLinkedList<T> : LinkedList<T>, IReference where T : class
    {
    }

    public class PooledSortedList<TKey, TValue> : SortedList<TKey, TValue>, IReference where TKey : notnull
    {
    }

    public class PooledSortedDictionary<TKey, TValue> : SortedDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    public class PooledConcurrentQueue<T> : System.Collections.Concurrent.ConcurrentQueue<T>, IReference where T : class
    {
    }

    public class PooledConcurrentStack<T> : System.Collections.Concurrent.ConcurrentStack<T>, IReference where T : class
    {
    }

    public class PooledConcurrentBag<T> : System.Collections.Concurrent.ConcurrentBag<T>, IReference where T : class
    {
    }

    public class PooledConcurrentDictionary<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    public class PooledConcurrentSet<T> : System.Collections.Concurrent.ConcurrentDictionary<T, byte>, IReference where T : class
    {
    }

    public class PooledConcurrentLinkedList<T> : System.Collections.Concurrent.ConcurrentBag<T>, IReference where T : class
    {
    }

    public class PooledConcurrentSortedList<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    public class PooledConcurrentSortedDictionary<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    public class PooledConcurrentHashSet<T> : System.Collections.Concurrent.ConcurrentDictionary<T, byte>, IReference where T : class
    {
    }
}
