#nullable enable

using System;
using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// QuestManager 统一实现。整合 QuestSystem、AchievementSystem、ConditionEvaluator、
    /// RewardProcessor 和 QuestChain，提供统一的任务/成就生命周期管理入口。
    /// 实现 <see cref="ITickable"/> 以驱动 QuestSystem 的定时逻辑。
    /// </summary>
    /// <typeparam name="TId">任务/成就拥有者标识类型，必须可判等。</typeparam>
    public sealed class QuestManager<TId> : IQuestManager<TId>, ITickable where TId : IEquatable<TId>
    {
        // ==================== 子系统 ====================

        private readonly QuestSystem<TId> _questSystem;
        private readonly AchievementSystem<TId> _achievementSystem;
        private readonly ConditionEvaluator<TId> _conditionEvaluator;
        private readonly RewardProcessor<TId> _rewardProcessor;

        // ==================== 配置数据 ====================

        private readonly Dictionary<int, QuestDef> _questDefs = new();
        private readonly Dictionary<int, AchievementDef> _achievementDefs = new();
        private readonly Dictionary<int, ConditionGroupDef> _conditionGroups = new();
        private readonly Dictionary<int, IReadOnlyList<RewardDef>> _rewardGroups = new();
        private readonly Dictionary<int, QuestChainDef> _questChainDefs = new();

        // questId → chainId 反向索引
        private readonly Dictionary<int, int> _questToChainMap = new();

        // ==================== 运行时状态 ====================

        // (chainId, ownerId) → 任务链实例
        private readonly Dictionary<(int chainId, TId ownerId), QuestChainInstance> _activeChains = new();

        // owner → (questId → handle)，用于去重和查询
        private readonly Dictionary<TId, Dictionary<int, QuestHandle>> _ownerQuestMap = new();

        // handle → ownerId 反向索引
        private readonly Dictionary<QuestHandle, TId> _handleToOwner = new();

        // owner → 已追踪的成就 ID 集合
        private readonly Dictionary<TId, HashSet<int>> _ownerAchievementMap = new();

        // ==================== 事件 ====================

        private readonly IEventBus _eventBus;
        private readonly IStorageProvider? _storage;

        // Counter 事件类型映射
        private readonly Dictionary<Type, string> _eventTypeToName = new();
        private readonly Dictionary<string, Type> _eventNameToType = new();

        private bool _initialized;

        // ==================== 构造函数 ====================

        /// <summary>
        /// 初始化 QuestManager 实例。
        /// </summary>
        /// <param name="eventBus">事件总线，用于向游戏全局发布任务/成就事件。不可为 null。</param>
        /// <param name="stateChecker">状态检查委托，用于评估 State 类型条件。key 为状态键名，value 为预期值。</param>
        /// <param name="storage">可选的存储提供者，用于保存/加载任务进度。</param>
        /// <exception cref="ArgumentNullException"><paramref name="eventBus"/> 为 null 时抛出。</exception>
        public QuestManager(
            IEventBus eventBus,
            Func<string, string, bool>? stateChecker = null,
            IStorageProvider? storage = null)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _storage = storage;

            _questSystem = new QuestSystem<TId>(GetQuestDef);
            _achievementSystem = new AchievementSystem<TId>(GetAchievementDef);
            _conditionEvaluator = new ConditionEvaluator<TId>(stateChecker);
            _rewardProcessor = new RewardProcessor<TId>();

            // 连接子系统事件到 QuestManager 内部处理
            _conditionEvaluator.OnConditionMet += OnConditionMet;
            _questSystem.OnQuestStateChanged += OnQuestSystemStateChanged;
            _achievementSystem.OnAchievementStateChanged += OnAchievementSystemStateChanged;
        }

        // ==================== IQuestManager 实现 ====================

        /// <inheritdoc />
        public void Initialize(
            IEnumerable<QuestDef> questDefs,
            IEnumerable<AchievementDef> achievementDefs,
            IEnumerable<ConditionGroupDef> conditionGroups,
            IEnumerable<RewardDef> rewards,
            IEnumerable<QuestChainDef> questChains)
        {
            if (_initialized)
            {
                return;
            }

            // 加载配置表
            foreach (var def in questDefs)
            {
                _questDefs[def.Id] = def;
            }

            foreach (var def in achievementDefs)
            {
                _achievementDefs[def.Id] = def;
            }

            var conditionGroupList = new List<ConditionGroupDef>();
            foreach (var cg in conditionGroups)
            {
                _conditionGroups[cg.Id] = cg;
                conditionGroupList.Add(cg);
            }

            foreach (var reward in rewards)
            {
                // RewardDef 按 ID 各自成组，QuestDef.RewardGroupIds 直接引用 RewardDef.Id
                if (!_rewardGroups.ContainsKey(reward.Id))
                {
                    _rewardGroups[reward.Id] = new List<RewardDef> { reward };
                }
            }

            foreach (var chain in questChains)
            {
                _questChainDefs[chain.Id] = chain;

                // 构建 questId → chainId 反向索引
                if (chain.QuestIds != null)
                {
                    foreach (var questId in chain.QuestIds)
                    {
                        _questToChainMap[questId] = chain.Id;
                    }
                }
            }

            // 加载条件配置到 ConditionEvaluator
            // 注意：ConditionEvaluator 需要 ConditionDef 列表，但 Initialize 没有直接传入
            // 从 ConditionGroupDef 可间接引用，但 ConditionDef 在 QuestManager 中不需要直接持有
            // 因为 ConditionEvaluator 的 LoadConfigs 需要完整的 Def 数据
            // 这里传递空列表——ConditionEvaluator 内部通过 RegisterQuestConditions 按需查询
            // 实际上 ConditionEvaluator.LoadConfigs 只用于存储 Def 引用，而在 RegisterQuestConditions 中
            // 需要通过 QuestManager 提供的 Def 查询
            _conditionEvaluator.LoadConfigs(Array.Empty<ConditionDef>(), conditionGroupList);

            _initialized = true;
            _eventBus.Publish(new QuestManagerInitializedEvent());
        }

        // ==================== 任务操作 ====================

        /// <inheritdoc />
        public QuestHandle TryAcceptQuest(int questId, TId ownerId)
        {
            if (!_initialized)
            {
                return QuestHandle.Invalid;
            }

            // 1. 获取 QuestDef
            if (!_questDefs.TryGetValue(questId, out var questDef))
            {
                return QuestHandle.Invalid;
            }

            var def = questDef;

            // 2. 检查前置任务：所有 PrerequisiteQuestIds 必须为 Claimed 状态
            if (def.PrerequisiteQuestIds != null && def.PrerequisiteQuestIds.Count > 0)
            {
                var ownerQuests = _questSystem.GetAllQuests(ownerId);
                foreach (var prereqId in def.PrerequisiteQuestIds)
                {
                    bool foundClaimed = false;
                    for (int i = 0; i < ownerQuests.Count; i++)
                    {
                        if (ownerQuests[i].QuestId == prereqId && ownerQuests[i].State == QuestState.Claimed)
                        {
                            foundClaimed = true;
                            break;
                        }
                    }

                    if (!foundClaimed)
                    {
                        return QuestHandle.Invalid;
                    }
                }
            }

            // 3. 检查 RequiredLevel：委托外部系统，此处不做硬检查
            // （项目方通过事件/前置条件自行处理等级限制）

            // 4. 检查任务链约束
            if (_questToChainMap.TryGetValue(questId, out var chainId))
            {
                if (!_questChainDefs.TryGetValue(chainId, out var chainDef))
                {
                    return QuestHandle.Invalid;
                }

                var chainKey = (chainId, ownerId);

                if (_activeChains.TryGetValue(chainKey, out var existingChain))
                {
                    // 链已激活，检查当前任务是否与要接受的任务一致
                    if (existingChain.GetCurrentQuestId() != questId)
                    {
                        return QuestHandle.Invalid;
                    }
                }
                else
                {
                    // 链尚未激活，创建新实例
                    if (chainDef.QuestIds == null || chainDef.QuestIds.Count == 0)
                    {
                        return QuestHandle.Invalid;
                    }

                    // 只有链中第一个任务可以启动链
                    if (chainDef.QuestIds[0] != questId)
                    {
                        return QuestHandle.Invalid;
                    }

                    var newChain = ReferencePool.Acquire<QuestChainInstance>();
                    newChain.Initialize(chainId, chainDef.QuestIds);
                    _activeChains[chainKey] = newChain;
                }
            }

            // 5. 检查是否已接受（QuestSystem 内部已做去重，此处仅为防御）
            if (_ownerQuestMap.TryGetValue(ownerId, out var questMap) && questMap.ContainsKey(questId))
            {
                return QuestHandle.Invalid;
            }

            // 6. 调用 QuestSystem.AcceptQuest
            var handle = _questSystem.AcceptQuest(questId, ownerId);
            if (!handle.IsValid)
            {
                return QuestHandle.Invalid;
            }

            // 7. 注册条件到 ConditionEvaluator
            if (def.ConditionGroupIds != null && def.ConditionGroupIds.Count > 0)
            {
                _conditionEvaluator.RegisterQuestConditions(handle.Id, def.ConditionGroupIds);
            }

            // 8. 追踪 owner → handle 映射
            if (!_ownerQuestMap.TryGetValue(ownerId, out var ownerMap))
            {
                ownerMap = new Dictionary<int, QuestHandle>();
                _ownerQuestMap[ownerId] = ownerMap;
            }

            ownerMap[questId] = handle;
            _handleToOwner[handle] = ownerId;

            // 9. 发布 QuestAcceptedEvent
            _eventBus.Publish(new QuestAcceptedEvent<TId>(ownerId, questId, handle));

            return handle;
        }

        /// <inheritdoc />
        public bool CompleteQuest(QuestHandle handle)
        {
            if (!_initialized)
            {
                return false;
            }

            return _questSystem.CompleteQuest(handle);
        }

        /// <inheritdoc />
        public RewardDeliveryResult ClaimQuest(QuestHandle handle)
        {
            if (!_initialized)
            {
                return RewardDeliveryResult.FailureResult("QuestManager not initialized.");
            }

            // 1. 获取 QuestInstance 和 OwnerId
            var quest = _questSystem.GetQuest(handle);
            if (quest == null)
            {
                return RewardDeliveryResult.FailureResult("Quest not found.");
            }

            if (!_handleToOwner.TryGetValue(handle, out var ownerId))
            {
                return RewardDeliveryResult.FailureResult("Owner not found for quest handle.");
            }

            // 2. 获取 Def 中的 RewardGroupIds
            if (!_questDefs.TryGetValue(quest.QuestId, out var questDef))
            {
                return RewardDeliveryResult.FailureResult("QuestDef not found.");
            }

            var def = questDef;
            var rewardGroupIds = def.RewardGroupIds ?? (IReadOnlyList<int>)Array.Empty<int>();

            // 3. 发放奖励
            var rewardResult = _rewardProcessor.ProcessRewards(rewardGroupIds, GetRewardsForGroup, ownerId);

            // 4. 调用 QuestSystem.ClaimQuest 标记 Claimed
            var claimResult = _questSystem.ClaimQuest(handle);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            // 5. 处理任务链推进
            if (_questToChainMap.TryGetValue(quest.QuestId, out var chainId))
            {
                var chainKey = (chainId, ownerId);
                if (_activeChains.TryGetValue(chainKey, out var chain))
                {
                    var oldChainState = chain.State;

                    if (chain.HasNextQuest)
                    {
                        chain.AdvanceToNext();

                        // 发布任务链推进事件
                        var chainEvent = new QuestChainStateChangedEvent<TId>(
                            ownerId, chainId, oldChainState, chain.State);
                        _eventBus.Publish(chainEvent);

                        // 自动接受链中下一个任务
                        var nextQuestId = chain.GetCurrentQuestId();
                        TryAcceptQuest(nextQuestId, ownerId);
                    }
                    else
                    {
                        // 链已完成
                        chain.AdvanceToNext();

                        var chainEvent = new QuestChainStateChangedEvent<TId>(
                            ownerId, chainId, oldChainState, QuestChainState.Completed);
                        _eventBus.Publish(chainEvent);
                    }
                }
            }

            // 6. 发布事件
            foreach (var groupId in rewardGroupIds)
            {
                _eventBus.Publish(new RewardClaimedEvent<TId>(ownerId, quest.QuestId, groupId, rewardResult));
            }

            _eventBus.Publish(new QuestClaimedEvent<TId>(ownerId, quest.QuestId, handle));

            return rewardResult;
        }

        /// <inheritdoc />
        public bool FailQuest(QuestHandle handle)
        {
            if (!_initialized)
            {
                return false;
            }

            var quest = _questSystem.GetQuest(handle);
            if (quest == null || quest.State != QuestState.Active)
            {
                return false;
            }

            if (!_handleToOwner.TryGetValue(handle, out var ownerId))
            {
                return false;
            }

            var oldState = quest.State;
            quest.State = QuestState.Failed;

            _eventBus.Publish(new QuestFailedEvent<TId>(ownerId, quest.QuestId, handle, FailReason.External));

            return true;
        }

        /// <inheritdoc />
        public void UpdateQuestProgress(QuestHandle handle, string progressKey, int delta)
        {
            if (!_initialized)
            {
                return;
            }

            var quest = _questSystem.GetQuest(handle);
            if (quest == null || quest.State != QuestState.Active)
            {
                return;
            }

            quest.Progress ??= new Dictionary<string, int>();
            quest.Progress.TryGetValue(progressKey, out var current);
            quest.Progress[progressKey] = current + delta;

            // 发布进度更新事件
            _eventBus.Publish(new QuestProgressUpdatedEvent(handle, progressKey, quest.Progress[progressKey], 0));
        }

        // ==================== 成就操作 ====================

        /// <inheritdoc />
        public bool CompleteAchievement(int achievementId, TId ownerId)
        {
            if (!_initialized)
            {
                return false;
            }

            // 1. 获取 AchievementDef
            if (!_achievementDefs.TryGetValue(achievementId, out var def))
            {
                return false;
            }

            // 2. 检查前置成就
            if (def.PrerequisiteAchievementIds != null && def.PrerequisiteAchievementIds.Count > 0)
            {
                foreach (var prereqId in def.PrerequisiteAchievementIds)
                {
                    var prereq = _achievementSystem.GetAchievement(prereqId, ownerId);
                    if (prereq == null || prereq.State != AchievementState.Claimed)
                    {
                        return false;
                    }
                }
            }

            // 3. 如果是隐藏成就，先揭示
            var existing = _achievementSystem.GetAchievement(achievementId, ownerId);
            if (existing == null)
            {
                if (!_achievementSystem.RevealAchievement(achievementId, ownerId))
                {
                    return false;
                }
            }

            // 4. 完成成就
            var result = _achievementSystem.CompleteAchievement(achievementId, ownerId);
            if (result)
            {
                // 追踪 owner 成就映射
                if (!_ownerAchievementMap.TryGetValue(ownerId, out var achievementSet))
                {
                    achievementSet = new HashSet<int>();
                    _ownerAchievementMap[ownerId] = achievementSet;
                }

                achievementSet.Add(achievementId);

                // 发布事件
                _eventBus.Publish(new AchievementStateChangedEvent<TId>(ownerId, achievementId, AchievementState.Revealed, AchievementState.Completed));
            }

            return result;
        }

        /// <inheritdoc />
        public RewardDeliveryResult ClaimAchievement(int achievementId, TId ownerId)
        {
            if (!_initialized)
            {
                return RewardDeliveryResult.FailureResult("QuestManager not initialized.");
            }

            // 1. 获取 AchievementDef
            if (!_achievementDefs.TryGetValue(achievementId, out var def))
            {
                return RewardDeliveryResult.FailureResult("AchievementDef not found.");
            }

            // 2. 获取奖励组
            var rewardGroupIds = def.RewardGroupIds ?? (IReadOnlyList<int>)Array.Empty<int>();

            // 3. 发放奖励
            var rewardResult = _rewardProcessor.ProcessRewards(rewardGroupIds, GetRewardsForGroup, ownerId);

            // 4. 标记 Claimed
            var claimResult = _achievementSystem.ClaimAchievement(achievementId, ownerId);
            if (!claimResult.Success)
            {
                return claimResult;
            }

            // 5. 发布事件
            foreach (var groupId in rewardGroupIds)
            {
                _eventBus.Publish(new RewardClaimedEvent<TId>(ownerId, achievementId, groupId, rewardResult));
            }

            _eventBus.Publish(new AchievementClaimedEvent<TId>(ownerId, achievementId));

            return rewardResult;
        }

        // ==================== 条件事件注册 ====================

        /// <inheritdoc />
        public void RegisterCounterEventType<T>(string eventTypeName)
        {
            var type = typeof(T);
            _eventTypeToName[type] = eventTypeName;
            _eventNameToType[eventTypeName] = type;
            _eventBus.Subscribe<T>(data => OnGameEventHandler(data));
        }

        /// <inheritdoc />
        public void NotifyGameEvent(string eventTypeName, TId? ownerId = default)
        {
            if (!_initialized)
            {
                return;
            }

            _conditionEvaluator.NotifyEvent(eventTypeName, ownerId);
        }

        // ==================== 查询 ====================

        /// <inheritdoc />
        public QuestInstance? GetQuest(QuestHandle handle)
        {
            return _questSystem.GetQuest(handle);
        }

        /// <inheritdoc />
        public AchievementInstance? GetAchievement(int achievementId, TId ownerId)
        {
            return _achievementSystem.GetAchievement(achievementId, ownerId);
        }

        /// <inheritdoc />
        public QuestChainInstance? GetQuestChain(int chainId, TId ownerId)
        {
            var key = (chainId, ownerId);
            _activeChains.TryGetValue(key, out var chain);
            return chain;
        }

        /// <inheritdoc />
        public int ActiveQuestCount
        {
            get { return _questSystem.ActiveCount; }
        }

        /// <inheritdoc />
        public int CompletedAchievementCount
        {
            get { return _achievementSystem.CompletedCount; }
        }

        // ==================== ITickable 实现 ====================

        /// <summary>
        /// 每帧驱动 QuestSystem 的定时逻辑（超时检查等）。
        /// </summary>
        public void Tick()
        {
            if (!_initialized)
            {
                return;
            }

            var deltaTime = Fixed64.FromDouble(HNLogicTime.DeltaTime);
            _questSystem.Tick(deltaTime);
        }

        /// <summary>
        /// LateTick 空实现。
        /// </summary>
        public void LateTick()
        {
        }

        // ==================== 保存/加载（可选） ====================

        /// <summary>
        /// 保存当前所有任务进度到存储。
        /// </summary>
        public void SaveProgress()
        {
            if (_storage == null)
            {
                return;
            }

            // TODO: 实现序列化逻辑——将活跃任务实例序列化为 JSON 并写入存储
            // _storage.Save("quest_progress", jsonString);
        }

        /// <summary>
        /// 从存储加载并恢复任务进度。
        /// </summary>
        public void LoadProgress()
        {
            if (_storage == null)
            {
                return;
            }

            // TODO: 实现反序列化逻辑——从存储读取并恢复任务状态
            // var jsonString = _storage.Load("quest_progress");
        }

        // ==================== 内部查询方法 ====================

        /// <summary>
        /// 通过 ID 从配置字典查找 QuestDef。
        /// </summary>
        private QuestDef? GetQuestDef(int id)
        {
            _questDefs.TryGetValue(id, out var def);
            return def;
        }

        /// <summary>
        /// 通过 ID 从配置字典查找 AchievementDef。
        /// </summary>
        private AchievementDef? GetAchievementDef(int id)
        {
            _achievementDefs.TryGetValue(id, out var def);
            return def;
        }

        /// <summary>
        /// 通过奖励组 ID 获取奖励列表。
        /// </summary>
        private IReadOnlyList<RewardDef>? GetRewardsForGroup(int groupId)
        {
            _rewardGroups.TryGetValue(groupId, out var list);
            return list;
        }

        /// <summary>
        /// 通过 ID 查找条件组定义。
        /// </summary>
        private ConditionGroupDef? GetConditionGroupDef(int groupId)
        {
            _conditionGroups.TryGetValue(groupId, out var def);
            return def;
        }

        // ==================== 事件处理 ====================

        /// <summary>
        /// 条件满足时的回调。检查该条件所属任务的所有条件组是否全部满足，
        /// 若全部满足则自动完成任务。
        /// </summary>
        /// <param name="conditionId">满足的条件 ID。</param>
        /// <param name="questHandleId">关联的任务句柄内部 ID。</param>
        private void OnConditionMet(int conditionId, int questHandleId)
        {
            if (!_conditionEvaluator.AreAllGroupsMet(questHandleId))
            {
                return;
            }

            var handle = new QuestHandle(questHandleId);
            CompleteQuest(handle);
        }

        /// <summary>
        /// QuestSystem 状态变更 → 通过 EventBus 发布到游戏全局。
        /// </summary>
        private void OnQuestSystemStateChanged(QuestStateChangedEvent<TId> evt)
        {
            _eventBus.Publish(evt);
        }

        /// <summary>
        /// AchievementSystem 状态变更 → 通过 EventBus 发布到游戏全局。
        /// </summary>
        private void OnAchievementSystemStateChanged(AchievementStateChangedEvent<TId> evt)
        {
            _eventBus.Publish(evt);
        }

        /// <summary>
        /// EventBus 通用游戏事件处理。通过事件类型查找对应的 eventTypeName，
        /// 并转发到 ConditionEvaluator 驱动 Counter 条件计数。
        /// </summary>
        private void OnGameEventHandler<T>(T data)
        {
            if (_eventTypeToName.TryGetValue(typeof(T), out var eventTypeName))
            {
                // 尝试从事件数据中提取 ownerId（简单情况使用 default）
                _conditionEvaluator.NotifyEvent(eventTypeName, default);
            }
        }
    }
}
