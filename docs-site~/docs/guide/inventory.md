---
sidebar_position: 22
---

# 物品/背包/装备框架（Inventory）

## 概述

物品/背包/装备框架（L9）提供了一套模块化的物品生命周期管理方案，包含三个核心子系统：

| 模块 | 说明 | 独立使用 |
|------|------|:-------:|
| **Item** | 物品实例（ItemInstance）+ 物品定义（ItemDef） | ✅ 可独立 |
| **Container** | 容器系统（IContainer + Container） | 依赖 Item |
| **Equipment** | 装备系统 + L7 Buff 生命周期管理 | 依赖 Container + Item |

## 命名空间

**Core 层**：`HN.Framework.Core.Level.Logic.Inventory`
**Unity 层**：`HN.Framework.Unity.Level.View.Inventory`

## 依赖

- **L3 Entity** — 通过 `EntityId`（uint）关联物品拥有者
- **L7 Combat**（可选）— 装备属性+Buff 效果通过 L7 BuffSystem 管理
- **C1 MemoryPack** — ItemInstance 的自定义格式化器序列化
- **C4 AssetManager** — 物品资源加载（图标/模型等通过 Addressables）

## Item — 物品系统

### 物品定义

物品定义（`ItemDef`）是配置表行定义，通过 L4 Sheet + Luban 加载：

```csharp
// 从配置表获取物品定义
ItemDef swordDef = sheetManager.GetTable<int, ItemDef>("tb_item").Get(1001);

// 物品定义包含：
// - Id: 1001
// - Name: "铁剑"
// - Category: ItemCategory.Weapon
// - MaxStack: 1（不可堆叠）
// - AllowedSlots: EquipmentSlot.Weapon（可装备在武器槽）
// - Buffs: 物品自带的 Buff 效果列表
```

### 物品实例

物品实例（`ItemInstance`）是运行时对象，支持对象池复用和序列化：

```csharp
// 创建物品实例
var item = new ItemInstance();
item.Initialize(itemDefId: 1001, stackCount: 1);

// 可通过 CanStackWith 判断堆叠
bool canStack = item.CanStackWith(otherItem);

// 关联实体
item.OwnerEntityId = playerEntityId;

// 关联掉落物实体
item.WorldEntityId = dropEntityId;

// 添加随机词条
item.ExtraData = new() { { "atk_bonus", (Fixed64)5m } };
```

### 物品 Buff 绑定

`ItemBuffSpec` 将物品与 L7 Combat Buff 关联：

```csharp
// 物品定义中声明的 Buff
// 装备时自动转换为 L7 BuffSpec 并 ApplyBuff
var buffSpec = itemDef.BuffSpec.BuildBuffSpec<uint>(ownerEntityId);
```

## Container — 容器系统

容器是管理物品堆叠和槽位的通用组件，可用作背包、仓库、装备栏等。

### 创建容器

```csharp
// 创建容量为 20 的背包
var backpack = new Container(ContainerType.Backpack, capacity: 20);

// 创建容量为 7 的装备栏
var equipmentBar = new Container(ContainerType.Equipment, capacity: 7);
```

### 容器操作

```csharp
var item = new ItemInstance();
item.Initialize(1001, 5);

// 添加物品（自动寻找空槽位或堆叠）
backpack.TryAddItem(item);

// 移除物品
backpack.RemoveItem(slotIndex: 0, count: 1);

// 交换槽位
backpack.SwapSlots(0, 1);

// 清空容器
backpack.Clear();

// 查询
int count = backpack.GetItemCount(1001);    // 总持有量
bool has = backpack.HasItem(1001, 5);        // 是否有 5 个
```

### 容器事件

容器提供增量变更事件通知：

```csharp
backpack.OnItemAdded += (ItemAddedEvent e) => {
    // 物品添加到槽位 e.SlotIndex
};

backpack.OnItemRemoved += (ItemRemovedEvent e) => { };
backpack.OnStackChanged += (ItemStackChangedEvent e) => { };
backpack.OnItemMoved += (ItemMovedEvent e) => { };
backpack.OnCleared += (ContainerClearedEvent e) => { };
```

## Equipment — 装备系统

装备系统管理实体身上的装备槽位，自动处理装备/卸下时 L7 Buff 的生命周期。

### 创建装备系统

```csharp
// 装备栏容器（7 槽位）
var eqContainer = new Container(ContainerType.Equipment, capacity: 7);

// 装备系统（可选注入 L7 BuffSystem）
var equipment = new Equipment(
    eqContainer,
    buffSystem,        // IBuffSystem<uint>? — 可 null
    ownerAttributes    // IAttributeSet<uint>? — 可 null
);
```

### 装备操作

```csharp
// 装备物品到武器槽
ItemInstance sword = ...;
if (equipment.CanEquip(sword, EquipmentSlot.Weapon))
{
    equipment.Equip(EquipmentSlot.Weapon, sword);
    // 自动执行：校验 AllowedSlots → 替换旧装备 → ApplyBuff
}

// 卸下装备
ItemInstance? unequipped = equipment.Unequip(EquipmentSlot.Weapon);
// 自动执行：RemoveBuff

// 交换槽位
equipment.SwapSlots(EquipmentSlot.Weapon, EquipmentSlot.Head);

// 查询
ItemInstance? equipped = equipment.GetEquippedItem(EquipmentSlot.Weapon);
var allEquipped = equipment.GetAllEquipped();
```

### 装备事件

```csharp
equipment.OnEquipped += (EquippedEvent e) => { };
equipment.OnUnequipped += (UnequippedEvent e) => { };
equipment.OnSlotSwap += (SlotSwapEvent e) => { };
equipment.OnFailedToEquip += (FailedToEquipEvent e) => {
    Console.WriteLine($"装备失败: {e.Reason}");
};
```

## Unity 层桥接

### ContainerView

挂载到 Entity 的 GameObject 上，桥接 Core 层容器数据：

```csharp
var view = gameObject.AddComponent<ContainerView>();
view.Initialize(entityId: 42, containerType: ContainerType.Backpack);
view.SetContainer(backpack);  // 由管理器注入

// UI 面板通过 GetContainer() 获取只读数据
IContainer? data = view.GetContainer();
```

### EquipmentView

挂载到 Entity 的 GameObject 上，桥接 Core 层装备数据：

```csharp
var view = gameObject.AddComponent<EquipmentView>();
view.Initialize(entityId: 42);
view.SetEquipment(equipment);

// UI 面板查询装备数据
IEquipment? eq = view.GetEquipment();
ItemInstance? weapon = view.GetEquippedItem(EquipmentSlot.Weapon);
```

## 掉落物表示

场景中的掉落物/可拾取物品复用现有的 L3 Entity + EntityView 系统，不另新建 `ItemWorldObject`：

```
ItemInstance.WorldEntityId → Entity(entityId) → EntityView(GameObject)
```

物品实例通过 `WorldEntityId` 字段关联到 L3 Entity，由 `ViewFactory` 负责创建对应的 `EntityView`。拾取时通过 EntityManager.Despawn 销毁视觉体，ItemInstance.WorldEntityId 置零。

## 序列化

`ItemInstance` 使用自定义 MemoryPack 格式化器 `ItemInstanceFormatter`，在 `FormattersInitializer.RegisterAll()` 中自动注册：

```csharp
// 序列化
byte[] data = MemoryPackSerializer.Serialize(item);

// 反序列化
ItemInstance restored = MemoryPackSerializer.Deserialize<ItemInstance>(data);
```

## 完整示例

```csharp
// 1. 创建玩家背包和装备栏
var backpack = new Container(ContainerType.Backpack, capacity: 20);
var eqContainer = new Container(ContainerType.Equipment, capacity: 7);
var equipment = new Equipment(eqContainer, buffSystem, playerAttrs);

// 2. 给玩家添加物品
var potion = new ItemInstance();
potion.Initialize(2001, 10);  // 10 瓶药水
backpack.TryAddItem(potion);

// 3. 装备武器
var sword = new ItemInstance();
sword.Initialize(1001, 1);
sword.OwnerEntityId = playerEntityId;

if (equipment.Equip(EquipmentSlot.Weapon, sword))
{
    Console.WriteLine("装备成功！自动触发了 Buff 应用。");
}

// 4. 查询装备属性
IEquipment playerEquipment = ...;  // 从 EquipmentView 获取
ItemInstance? weapon = playerEquipment.GetEquippedItem(EquipmentSlot.Weapon);
int wpnId = weapon?.ItemDefId ?? 0;
```
