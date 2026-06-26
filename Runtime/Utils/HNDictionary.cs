using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HN
{
    public class HNDictionary
    {

    }


    [Serializable]
    public class HNDictionary<TKey, TValue> : HNDictionary, ISerializationCallbackReceiver, IDictionary<TKey, TValue>
    {
        /// <summary>
        /// 获取字典中所有键的集合。
        /// </summary>
        public ICollection<TKey> Keys => list.Select(tuple => tuple.Key).ToArray();

        /// <summary>
        /// 获取字典中所有值的集合。
        /// </summary>
        public ICollection<TValue> Values => list.Select(tuple => tuple.Value).ToArray();

        /// <summary>
        /// 获取字典中包含的键值对数量。
        /// </summary>
        public int Count => list.Count;

        /// <summary>
        /// 获取一个值，该值指示字典是否为只读。
        /// </summary>
        public bool IsReadOnly => false;


        [SerializeField] 
        private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();

        private Dictionary<TKey, int> KeyPositions => keyPositions.Value;

        private Lazy<Dictionary<TKey, int>> keyPositions;


        /// <summary>
        /// 初始化 <see cref="HNDictionary{TKey, TValue}"/> 类的新实例。
        /// </summary>
        public HNDictionary() 
        {
            keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
        }

        /// <summary>
        /// 序列化前调用。本实现不执行任何操作。
        /// </summary>
        public void OnBeforeSerialize() 
        { 

        }

        /// <summary>
        /// 反序列化后调用，重建键位置索引。
        /// </summary>
        public void OnAfterDeserialize() 
        {
            keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
        }

        /// <summary>
        /// 获取或设置与指定键关联的值。
        /// </summary>
        /// <param name="key">要获取或设置的值的键。</param>
        /// <returns>与指定键关联的值。</returns>
        public TValue this[TKey key] 
        {
            get => list[KeyPositions[key]].Value;
            set {
                var pair = new SerializableKeyValuePair(key, value);
                if (KeyPositions.ContainsKey(key)) {
                    list[KeyPositions[key]] = pair;
                }
                else {
                    KeyPositions[key] = list.Count;
                    list.Add(pair);
                }
            }
        }

        /// <summary>
        /// 向字典中添加指定键和值的元素。
        /// </summary>
        /// <param name="key">要添加的元素的键。</param>
        /// <param name="value">要添加的元素的值。</param>
        /// <exception cref="ArgumentException">字典中已存在相同键的元素。</exception>
        public void Add(TKey key, TValue value) 
        {
            if (KeyPositions.ContainsKey(key))
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            else 
            {
                KeyPositions[key] = list.Count;
                list.Add(new SerializableKeyValuePair(key, value));
            }
        }

        /// <summary>
        /// 确定字典是否包含指定的键。
        /// </summary>
        /// <param name="key">要在字典中定位的键。</param>
        /// <returns>如果字典包含具有指定键的元素，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

        /// <summary>
        /// 从字典中移除具有指定键的元素。
        /// </summary>
        /// <param name="key">要移除的元素的键。</param>
        /// <returns>如果成功移除元素，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Remove(TKey key) 
        {
            if (KeyPositions.TryGetValue(key, out var index)) 
            {
                KeyPositions.Remove(key);

                list.RemoveAt(index);
                for (var i = index; i < list.Count; i++)
                    KeyPositions[list[i].Key] = i;
                
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 获取与指定键关联的值。
        /// </summary>
        /// <param name="key">要获取的值的键。</param>
        /// <param name="value">当此方法返回时，如果找到指定键，则包含与该键关联的值；否则包含 <c>default</c>。</param>
        /// <returns>如果字典包含具有指定键的元素，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool TryGetValue(TKey key, out TValue value) 
        {
            if (KeyPositions.TryGetValue(key, out var index)) 
            {
                value = list[index].Value;
                return true;
            }
            else 
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// 向字典中添加指定的键值对。
        /// </summary>
        /// <param name="kvp">要添加的键值对。</param>
        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        /// <summary>
        /// 从字典中移除所有键值对。
        /// </summary>
        public void Clear() => list.Clear();

        /// <summary>
        /// 确定字典是否包含指定的键值对（仅检查键）。
        /// </summary>
        /// <param name="kvp">要在字典中定位的键值对。</param>
        /// <returns>如果在字典中找到指定键，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

        /// <summary>
        /// 从指定的数组索引开始，将字典中的键值对复制到 <see cref="KeyValuePair{TKey, TValue}"/> 数组中。
        /// </summary>
        /// <param name="array">作为复制目标的一维 <see cref="KeyValuePair{TKey, TValue}"/> 数组。</param>
        /// <param name="arrayIndex">数组中从零开始的复制起始索引。</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) 
        {
            var numKeys = list.Count;
            if (array.Length - arrayIndex < numKeys)
                throw new ArgumentException("arrayIndex");
            for (var i = 0; i < numKeys; i++, arrayIndex++) 
            {
                var entry = list[i];
                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// 从字典中移除指定的键值对（基于键）。
        /// </summary>
        /// <param name="kvp">要移除的键值对。</param>
        /// <returns>如果成功移除元素，则为 <c>true</c>；否则为 <c>false</c>。</returns>
        public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

        /// <summary>
        /// 返回一个循环访问字典的枚举器。
        /// </summary>
        /// <returns>用于循环访问字典的枚举器。</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return list.Select(ToKeyValuePair).GetEnumerator();

            static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp) 
            {
                return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
            }
        }

    
        /// <summary>
        /// 返回一个循环访问字典的枚举器（非泛型）。
        /// </summary>
        /// <returns>用于循环访问字典的枚举器。</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private Dictionary<TKey, int> MakeKeyPositions() 
        {
            var dictionary = new Dictionary<TKey, int>(list.Count);
            for (var i = 0; i < list.Count; i++) {
                dictionary[list[i].Key] = i;
            }
            return dictionary;
        }


        [Serializable]
        private struct SerializableKeyValuePair 
        {
            /// <summary>
            /// 键。
            /// </summary>
            public TKey Key;

            /// <summary>
            /// 值。
            /// </summary>
            public TValue Value;

            /// <summary>
            /// 初始化 <see cref="SerializableKeyValuePair"/> 结构的新实例。
            /// </summary>
            /// <param name="key">键。</param>
            /// <param name="value">值。</param>
            public SerializableKeyValuePair(TKey key, TValue value) 
            {
                Key = key;
                Value = value;
            }
        }
    }
}
