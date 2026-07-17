---
sidebar_position: 20
---

# 战斗数值体系（Combat）

## 概述

战斗数值体系（L7）是基于 GAS（Gameplay Ability System）模式的模块化数值框架，
包含四个可独立拆用的子模块：

| 模块 | 说明 | 独立使用 |
|------|------|:-------:|
| **Attribute** | 属性集合 + Modifier 修正系统 | ✅ 可独立 |
| **Effect** | 效果管线（Instant/Duration/Infinite） | 依赖 Attribute |
| **Buff** | Buff 层叠与生命周期管理 | 依赖 Effect |
| **Ability** | 技能激活与冷却管理 | 依赖 Buff + GameplayTag(可选) |

## 命名空间

`HN.Framework.Core.Level.Logic.Combat`

所有组件位于 Core 层，无 Unity 依赖。

## 依赖

- **FixedMathSharp** — 所有数值计算使用 `Fixed64` 定点数
- **L0 GameplayTag**（可选）— Ability 的标签条件检查
- **C7 EventBus**（可选）— 后续事件集成
- **不依赖** L1 MVC、L2 HFSM、L3 Entity（通过 `TId` 泛型解耦）

## Attribute — 属性系统

### 定义属性类型

```csharp
// 配置表加载后，通过 AttributeTypeManager 管理
var attrMgr = new AttributeTypeManager();
attrMgr.LoadFromDefinitions(definitions); // 从配置表加载
attrMgr.Freeze(); // 冻结

var hpType = attrMgr.GetType("MaxHP");
var atkType = attrMgr.GetType("ATK");
```

### 使用 AttributeSet

```csharp
var attrs = new AttributeSet<int>();

// 设置基础值
attrs.SetBaseValue(hpType, (Fixed64)1000);
attrs.SetBaseValue(atkType, (Fixed64)150);

// 添加 Modifier
attrs.AddModifier(atkType, new Modifier<int>(
    source: 1,
    op: ModifierOp.Add,
    value: (Fixed64)50,
    priority: 0
));

// 获取最终值
var finalAtk = attrs.GetFinalValue(atkType); // 200

// 监听变更
attrs.OnValueChanged += (type, oldVal, newVal) => {
    Console.WriteLine($"{type} 从 {oldVal} 变为 {newVal}");
};
```

### Modifier 运算类型

| 运算 | 说明 | 计算优先级 |
|------|------|:---------:|
| `Override` | 覆盖为指定值 | 1（最高优先级决定值） |
| `Add` | 加法累加（按 Priority 排序） | 2 |
| `Multiply` | 乘法累乘（按 Priority 排序） | 3 |
| `MinCap` | 最小值下限 | 4 |
| `MaxCap` | 最大值上限 | 5 |

## Effect — 效果系统

### 创建效果

```csharp
var effect = new EffectSpec<int>
{
    EffectId = 1,
    Type = EffectType.Duration,
    Duration = (Fixed64)10, // 持续 10 秒
    AttributeModifiers =
    {
        new AttributeModifier<int>(atkType,
            new Modifier<int>(0, ModifierOp.Add, (Fixed64)30)),
    }
};
```

### 通过管线应用

```csharp
var pipeline = new EffectPipeline<int>();
var handle = pipeline.Apply(effect, source: 1, target: 2, targetAttributes);

// Tick 推进生命周期
pipeline.Tick(deltaTime, targetAttributes);

// 手动移除
pipeline.RemoveEffect(handle, targetAttributes);
```

## Damage — 伤害管线

### 四阶段模型

```
PreMigration → DamageCalculate → PostMigration → ApplyDamage
```

### 使用默认公式

```csharp
var pipeline = new DamagePipeline<int>();
var damage = pipeline.DealDamage(
    attackerId: 1,
    defenderId: 2,
    baseDamage: 100,
    attackerAttributes,
    defenderAttributes
);
```

### 自定义公式

```csharp
public class MyFormula<TId> : IDamageFormula<TId> where TId : IEquatable<TId>
{
    public Fixed64 Calculate(ref DamageContext<TId> context)
    {
        // 自定义伤害计算逻辑
        return context.BaseDamage * (Fixed64)1.5;
    }
}

var pipeline = new DamagePipeline<int>(new MyFormula<int>());
```

### 扩展各阶段

```csharp
pipeline.OnPreMigration = (ref DamageContext<int> ctx) => {
    ctx.PreMigrationDamage *= (Fixed64)1.2; // 增伤 20%
};
pipeline.OnPostMigration = (ref DamageContext<int> ctx) => {
    ctx.FinalDamage /= (Fixed64)2; // 减半
};
pipeline.OnApplyDamage = (ref DamageContext<int> ctx) => {
    // 自定义扣血逻辑
};
```

## Buff — Buff 系统

### 创建 Buff

```csharp
var buffSpec = new BuffSpec<int>
{
    BuffId = 1,
    Duration = (Fixed64)10,
    StackRule = BuffStackRule.Multi,
    MaxStacks = 3,
    ApplyEffects = { /* 应用时触发效果 */ },
    PeriodicEffects =
    {
        new PeriodicEffectSpec<int>
        {
            Interval = (Fixed64)2, // 每 2 秒触发一次
            Effect = periodicEffect,
        }
    },
};
```

### 应用 Buff

```csharp
var buffSystem = new BuffSystem<int>();
var handle = buffSystem.ApplyBuff(buffSpec, source: 1, target: 2,
    targetAttributes, effectPipeline);

// Tick 推进
buffSystem.Tick(deltaTime, targetAttributes, effectPipeline);

// 手动移除
buffSystem.RemoveBuff(handle, targetAttributes, effectPipeline);
```

### 堆叠规则

| 规则 | 行为 |
|------|------|
| `Single` | 不可堆叠，新 Buff 覆盖旧 Buff |
| `Multi` | 可堆叠至 MaxStacks |
| `Refresh` | 刷新持续时间，不增加层数 |
| `Extend` | 延长持续时间 |

## Ability — 技能系统

### 定义技能

```csharp
var ability = new AbilitySpec<int>
{
    AbilityId = 1,
    Cooldown = (Fixed64)5,
    Cost = new Dictionary<AttributeType, Fixed64>
    {
        { mpType, (Fixed64)30 }
    },
    Effects = { /* 技能效果列表 */ },
};
```

### 激活技能

```csharp
var abilitySystem = new AbilitySystem<int>();
abilitySystem.RegisterAbility(ability);

if (abilitySystem.TryActivateAbility(ability, owner: 1, target: 2,
    ownerAttributes, targetAttributes, effectPipeline))
{
    // 技能激活成功
}

// Tick 推进冷却
abilitySystem.Tick(deltaTime);

// 查询冷却
var remaining = abilitySystem.GetCooldownRemaining(1, 1);
```

### GameplayTag 集成（可选）

AbilitySystem 支持 `GameplayTagContainer` 作为技能激活的条件检查。
激活标签（ActivationTags）和阻止标签（BlockingTags）通过 TagQuery 定义。

## 完整工作流示例

```csharp
// 1. 初始化
var attrs = new AttributeSet<int>();
attrs.SetBaseValue(hp, (Fixed64)1000);
attrs.SetBaseValue(atk, (Fixed64)100);

var effectPipe = new EffectPipeline<int>();
var buffSys = new BuffSystem<int>();
var abilitySys = new AbilitySystem<int>();

// 2. 创建伤害技能
var fireball = new AbilitySpec<int>
{
    AbilityId = 1,
    Cooldown = (Fixed64)3,
    Cost = new Dictionary<AttributeType, Fixed64> { { mp, (Fixed64)20 } },
    Effects =
    {
        new EffectSpec<int>
        {
            EffectId = 100,
            Type = EffectType.Instant,
            AttributeModifiers =
            {
                new AttributeModifier<int>(hp,
                    new Modifier<int>(0, ModifierOp.Add, (Fixed64)(-150))),
            }
        }
    },
};

// 3. 激活
abilitySys.TryActivateAbility(fireball, 1, 2, attrs, targetAttrs, effectPipe);
```

## 数据配置（Luban + L4 Sheet）

所有 Combat 数据定义统一走 Luban 配置表，通过 L4 Sheet 运行时加载：

```
Excel(tb_attribute / tb_buff / tb_ability / ...)
    → Luban 生成 C# struct（MemoryPackable）
    → .bin 序列化
    → L4 SheetManager 运行时加载
    → Combat 模块消费
```

更多配置表集成请参考 [Sheet 系统指南](./sheet)。
