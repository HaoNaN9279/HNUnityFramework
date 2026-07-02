using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Driver.Common
{
    /// <summary>
    /// 可被引用池管理的 List 封装
    /// </summary>
    public class PooledList<T> : List<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的 Dictionary 封装
    /// </summary>
    public class PooledDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    /// <summary>
    /// 可被引用池管理的 Queue 封装
    /// </summary>
    public class PooledQueue<T> : Queue<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的 Stack 封装
    /// </summary>
    public class PooledStack<T> : Stack<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的 HashSet 封装
    /// </summary>
    public class PooledHashSet<T> : HashSet<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的 LinkedList 封装
    /// </summary>
    public class PooledLinkedList<T> : LinkedList<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的 SortedList 封装
    /// </summary>
    public class PooledSortedList<TKey, TValue> : SortedList<TKey, TValue>, IReference where TKey : notnull
    {
    }

    /// <summary>
    /// 可被引用池管理的 SortedDictionary 封装
    /// </summary>
    public class PooledSortedDictionary<TKey, TValue> : SortedDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全 ConcurrentQueue 封装
    /// </summary>
    public class PooledConcurrentQueue<T> : System.Collections.Concurrent.ConcurrentQueue<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全 ConcurrentStack 封装
    /// </summary>
    public class PooledConcurrentStack<T> : System.Collections.Concurrent.ConcurrentStack<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全 ConcurrentBag 封装
    /// </summary>
    public class PooledConcurrentBag<T> : System.Collections.Concurrent.ConcurrentBag<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全 ConcurrentDictionary 封装
    /// </summary>
    public class PooledConcurrentDictionary<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全并发 Set 封装
    /// </summary>
    public class PooledConcurrentSet<T> : System.Collections.Concurrent.ConcurrentDictionary<T, byte>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全并发链表封装
    /// </summary>
    public class PooledConcurrentLinkedList<T> : System.Collections.Concurrent.ConcurrentBag<T>, IReference where T : class
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全有序列表封装
    /// </summary>
    public class PooledConcurrentSortedList<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全有序字典封装
    /// </summary>
    public class PooledConcurrentSortedDictionary<TKey, TValue> : System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, IReference where TKey : notnull
    {
    }

    /// <summary>
    /// 可被引用池管理的线程安全 HashSet 封装
    /// </summary>
    public class PooledConcurrentHashSet<T> : System.Collections.Concurrent.ConcurrentDictionary<T, byte>, IReference where T : class
    {
    }
}
