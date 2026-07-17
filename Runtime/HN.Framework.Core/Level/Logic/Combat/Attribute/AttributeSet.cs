#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 属性集合接口。管理一组属性（如 HP、ATK、DEF）及其 Modifier。
    /// <typeparamref name="TId"/> 为来源标识类型（如 EntityId），解耦实体。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public interface IAttributeSet<TId> where TId : IEquatable<TId>
    {
        /// <summary>获取属性的最终值（经 Modifier 计算后）。</summary>
        Fixed64 GetFinalValue(AttributeType type);

        /// <summary>获取属性的基础值（未经 Modifier 调整）。</summary>
        Fixed64 GetBaseValue(AttributeType type);

        /// <summary>设置属性的基础值，触发重新计算。</summary>
        void SetBaseValue(AttributeType type, Fixed64 value);

        /// <summary>添加 Modifier，返回句柄用于移除。</summary>
        ModifierHandle AddModifier(AttributeType type, Modifier<TId> modifier);

        /// <summary>通过句柄移除 Modifier，触发重新计算。</summary>
        bool RemoveModifier(ModifierHandle handle);

        /// <summary>属性值变更事件（type, oldValue, newValue）。</summary>
        event Action<AttributeType, Fixed64, Fixed64>? OnValueChanged;

        /// <summary>获取所有已注册的属性类型。</summary>
        IEnumerable<AttributeType> GetAllAttributeTypes();
    }

    /// <summary>
    /// 属性集合实现。使用脏标记策略：仅在 BaseValue 或 Modifier 变更时重新计算 FinalValue。
    /// 计算顺序：Override → Add(按Priority) → Multiply(按Priority) → MinCap → MaxCap。
    /// </summary>
    /// <typeparam name="TId">来源标识类型。</typeparam>
    public sealed class AttributeSet<TId> : IAttributeSet<TId> where TId : IEquatable<TId>
    {
        // 基础值（来自配置表）
        private readonly Dictionary<AttributeType, Fixed64> _baseValues = new();

        // 各属性上的活跃 Modifier（按 Type 分组存储）
        private readonly Dictionary<AttributeType, List<ModifierEntry>> _modifiers = new();

        // 缓存最终值
        private readonly Dictionary<AttributeType, Fixed64> _finalValues = new();

        // 脏标记 —— 需要重新计算的属性
        private readonly HashSet<AttributeType> _dirtyAttributes = new();

        // 全局句柄 → (AttributeType, ModifierEntry) 映射，用于 O(1) 移除
        private readonly Dictionary<ModifierHandle, (AttributeType type, int entryIndex)> _handleMap = new();

        // 句柄 ID 自增计数器（从 1 开始，0 保留给 Invalid）
        private int _nextHandleId = 1;

        // 待回收的已被移除的 ModifierEntry 列表，延迟清理
        private readonly List<(AttributeType type, int entryIndex)> _pendingCleanup = new();

        /// <inheritdoc />
        public event Action<AttributeType, Fixed64, Fixed64>? OnValueChanged;

        /// <summary>
        /// 获取属性的最终值。如属性未注册则返回 <see cref="Fixed64.Zero"/>。
        /// </summary>
        public Fixed64 GetFinalValue(AttributeType type)
        {
            if (!_baseValues.ContainsKey(type))
                return Fixed64.Zero;

            if (_dirtyAttributes.Contains(type))
                Recalculate(type);

            return _finalValues.TryGetValue(type, out var val) ? val : Fixed64.Zero;
        }

        /// <summary>
        /// 获取属性的基础值。如属性未注册则返回 <see cref="Fixed64.Zero"/>。
        /// </summary>
        public Fixed64 GetBaseValue(AttributeType type)
        {
            return _baseValues.TryGetValue(type, out var val) ? val : Fixed64.Zero;
        }

        /// <summary>
        /// 设置属性的基础值，触发重新计算。如属性尚未注册则自动注册。
        /// </summary>
        public void SetBaseValue(AttributeType type, Fixed64 value)
        {
            if (!type.IsValid)
                throw new ArgumentException("不能为无效的 AttributeType 设置值。", nameof(type));

            var oldValue = _baseValues.TryGetValue(type, out var existing) ? existing : Fixed64.Zero;

            _baseValues[type] = value;
            MarkDirty(type);

            if (oldValue != value)
            {
                var newFinal = GetFinalValue(type);
                OnValueChanged?.Invoke(type, oldValue, newFinal);
            }
        }

        /// <summary>
        /// 添加 Modifier 到指定属性。返回句柄可用于后续移除。
        /// 重复来源的 Modifier 可叠加（Multi 模式），由调用方处理层叠逻辑。
        /// </summary>
        public ModifierHandle AddModifier(AttributeType type, Modifier<TId> modifier)
        {
            if (!type.IsValid)
                throw new ArgumentException("不能为无效的 AttributeType 添加 Modifier。", nameof(type));

            var handle = new ModifierHandle(_nextHandleId++);

            if (!_modifiers.TryGetValue(type, out var list))
            {
                list = new List<ModifierEntry>();
                _modifiers[type] = list;
            }

            var entry = new ModifierEntry(handle, modifier);
            list.Add(entry);
            _handleMap[handle] = (type, list.Count - 1);

            MarkDirty(type);

            // 通知值变化
            var newValue = Recalculate(type);
            OnValueChanged?.Invoke(type, Fixed64.Zero, newValue);

            return handle;
        }

        /// <summary>
        /// 通过句柄移除 Modifier。返回 true 表示移除成功，false 表示句柄无效或已移除。
        /// </summary>
        public bool RemoveModifier(ModifierHandle handle)
        {
            if (!_handleMap.TryGetValue(handle, out var mapping))
                return false;

            var (type, entryIndex) = mapping;

            if (!_modifiers.TryGetValue(type, out var list))
                return false;

            if (entryIndex < 0 || entryIndex >= list.Count)
                return false;

            if (list[entryIndex].Handle != handle)
                return false;

            // 标记为已移除（标记删除，避免移位影响其他句柄）
            var removed = list[entryIndex];
            list[entryIndex] = new ModifierEntry(ModifierHandle.Invalid, removed.Modifier);
            _handleMap.Remove(handle);

            MarkDirty(type);

            var newValue = Recalculate(type);
            OnValueChanged?.Invoke(type, Fixed64.Zero, newValue);

            // 延迟清理空条目
            CleanupEmptyEntries(type, list);

            return true;
        }

        /// <inheritdoc />
        public IEnumerable<AttributeType> GetAllAttributeTypes()
        {
            return _baseValues.Keys;
        }

        /// <summary>
        /// 从配置表批量初始化基础值。
        /// </summary>
        /// <param name="definitions">属性定义列表。</param>
        public void InitializeFromDefinitions(IList<AttributeDefinition> definitions)
        {
            if (definitions == null)
                throw new ArgumentNullException(nameof(definitions));

            foreach (var def in definitions)
            {
                var type = new AttributeType(def.Id);
                _baseValues[type] = def.InitialValue;
            }

            // 全部重置后清除脏标记并计算初始 final
            _dirtyAttributes.Clear();
            foreach (var kvp in _baseValues)
            {
                _finalValues[kvp.Key] = kvp.Value;
            }
        }

        // ==================== 内部控制 ====================

        private void MarkDirty(AttributeType type)
        {
            _dirtyAttributes.Add(type);
        }

        /// <summary>
        /// 重新计算指定属性的 FinalValue。
        /// 计算顺序：Override → Add(按Priority升序) → Multiply(按Priority升序) → MinCap → MaxCap。
        /// </summary>
        private Fixed64 Recalculate(AttributeType type)
        {
            _dirtyAttributes.Remove(type);

            if (!_baseValues.TryGetValue(type, out var baseVal))
            {
                _finalValues[type] = Fixed64.Zero;
                return Fixed64.Zero;
            }

            var result = baseVal;

            if (!_modifiers.TryGetValue(type, out var list) || list.Count == 0)
            {
                _finalValues[type] = result;
                return result;
            }

            // 收集有效的 Modifier
            var activeMods = new List<ModifierEntry>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Handle.IsValid)
                    activeMods.Add(list[i]);
            }

            if (activeMods.Count == 0)
            {
                _finalValues[type] = result;
                return result;
            }

            // 阶段 1：Override — 按 Priority 降序排序，取最高优先级的 Override
            bool hasOverride = false;
            Fixed64 overrideValue = Fixed64.Zero;
            int overridePriority = int.MinValue;

            for (int i = 0; i < activeMods.Count; i++)
            {
                if (activeMods[i].Modifier.Op == ModifierOp.Override)
                {
                    if (!hasOverride || activeMods[i].Modifier.Priority > overridePriority)
                    {
                        hasOverride = true;
                        overrideValue = activeMods[i].Modifier.Value;
                        overridePriority = activeMods[i].Modifier.Priority;
                    }
                }
            }

            if (hasOverride)
                result = overrideValue;

            // 阶段 2：Add — 按 Priority 升序
            // 先排序
            var addMods = new List<ModifierEntry>(activeMods.Count);
            for (int i = 0; i < activeMods.Count; i++)
            {
                if (activeMods[i].Modifier.Op == ModifierOp.Add)
                    addMods.Add(activeMods[i]);
            }
            addMods.Sort((a, b) => a.Modifier.Priority.CompareTo(b.Modifier.Priority));
            for (int i = 0; i < addMods.Count; i++)
            {
                result += addMods[i].Modifier.Value;
            }

            // 阶段 3：Multiply — 按 Priority 升序
            var mulMods = new List<ModifierEntry>(activeMods.Count);
            for (int i = 0; i < activeMods.Count; i++)
            {
                if (activeMods[i].Modifier.Op == ModifierOp.Multiply)
                    mulMods.Add(activeMods[i]);
            }
            mulMods.Sort((a, b) => a.Modifier.Priority.CompareTo(b.Modifier.Priority));
            for (int i = 0; i < mulMods.Count; i++)
            {
                result *= mulMods[i].Modifier.Value;
            }

            // 阶段 4：MinCap
            for (int i = 0; i < activeMods.Count; i++)
            {
                if (activeMods[i].Modifier.Op == ModifierOp.MinCap)
                {
                    result = FixedMath.Max(result, activeMods[i].Modifier.Value);
                }
            }

            // 阶段 5：MaxCap
            for (int i = 0; i < activeMods.Count; i++)
            {
                if (activeMods[i].Modifier.Op == ModifierOp.MaxCap)
                {
                    result = FixedMath.Min(result, activeMods[i].Modifier.Value);
                }
            }

            _finalValues[type] = result;
            return result;
        }

        /// <summary>清理列表中被标记删除的条目。</summary>
        private void CleanupEmptyEntries(AttributeType type, List<ModifierEntry> list)
        {
            _pendingCleanup.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].Handle.IsValid)
                    _pendingCleanup.Add((type, i));
            }

            if (_pendingCleanup.Count == 0)
                return;

            // 从后往前移除以避免索引位移
            _pendingCleanup.Sort((a, b) => b.entryIndex.CompareTo(a.entryIndex));
            foreach (var (_, idx) in _pendingCleanup)
            {
                list.RemoveAt(idx);
            }

            // 重建句柄映射
            RebuildHandleMap(type, list);
        }

        private void RebuildHandleMap(AttributeType type, List<ModifierEntry> list)
        {
            // 移除该类型所有旧映射
            var toRemove = new List<ModifierHandle>();
            foreach (var kvp in _handleMap)
            {
                if (kvp.Value.type.Equals(type))
                    toRemove.Add(kvp.Key);
            }
            for (int i = 0; i < toRemove.Count; i++)
                _handleMap.Remove(toRemove[i]);

            // 重建
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Handle.IsValid)
                    _handleMap[list[i].Handle] = (type, i);
            }
        }

        /// <summary>内部 Modifier 条目，关联句柄与数据。</summary>
        private readonly struct ModifierEntry
        {
            public readonly ModifierHandle Handle;
            public readonly Modifier<TId> Modifier;

            public ModifierEntry(ModifierHandle handle, Modifier<TId> modifier)
            {
                Handle = handle;
                Modifier = modifier;
            }
        }
    }
}
