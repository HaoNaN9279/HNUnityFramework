using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic.AI.KnowledgePool
{
    /// <summary>
    /// 感知状态聚合 — 汇总视觉、听觉等多通道感知数据。
    /// 每帧由感知系统填充，供决策层消费。
    /// 实现 <see cref="IReference"/> 接口，支持通过 <see cref="ReferencePool"/> 进行对象池复用。
    /// </summary>
    public class PerceptionState : IReference
    {
        private PooledList<PerceptionEntry> m_VisibleTargets;
        private PooledList<PerceptionEntry> m_AudioTargets;
        private PooledList<PerceptionEntry> m_ThreatTargets;

        private static readonly PooledList<PerceptionEntry> s_EmptyList = new PooledList<PerceptionEntry>();

        /// <summary>
        /// 初始化感知状态，从引用池中申请各通道列表。
        /// </summary>
        public void Initialize()
        {
            m_VisibleTargets = ReferencePool.Acquire<PooledList<PerceptionEntry>>();
            m_AudioTargets = ReferencePool.Acquire<PooledList<PerceptionEntry>>();
            m_ThreatTargets = ReferencePool.Acquire<PooledList<PerceptionEntry>>();
        }

        /// <summary>
        /// 添加可见目标。
        /// </summary>
        /// <param name="entityId">目标实体 ID</param>
        /// <param name="confidence">置信度 (0-1)</param>
        /// <param name="distance">距离</param>
        /// <param name="position">目标位置</param>
        public void AddVisibleTarget(int entityId, float confidence, float distance, Vector3Data position)
        {
            m_VisibleTargets?.Add(new PerceptionEntry
            {
                EntityId = entityId,
                Confidence = confidence,
                Distance = distance,
                Position = position,
                DetectedAtTime = 0f
            });
        }

        /// <summary>
        /// 添加声音目标。
        /// </summary>
        /// <param name="entityId">目标实体 ID</param>
        /// <param name="confidence">置信度 (0-1)</param>
        /// <param name="distance">距离</param>
        /// <param name="position">目标位置</param>
        /// <param name="audioType">音频类型</param>
        public void AddAudioTarget(int entityId, float confidence, float distance, Vector3Data position, AudioType audioType)
        {
            m_AudioTargets?.Add(new PerceptionEntry
            {
                EntityId = entityId,
                Confidence = confidence,
                Distance = distance,
                Position = position,
                AudioType = audioType,
                DetectedAtTime = 0f
            });
        }

        /// <summary>
        /// 添加威胁目标。
        /// </summary>
        /// <param name="entityId">目标实体 ID</param>
        /// <param name="threatLevel">威胁等级 (0-1)</param>
        /// <param name="distance">距离</param>
        /// <param name="position">目标位置</param>
        public void AddThreatTarget(int entityId, float threatLevel, float distance, Vector3Data position)
        {
            m_ThreatTargets?.Add(new PerceptionEntry
            {
                EntityId = entityId,
                Confidence = threatLevel,
                Distance = distance,
                Position = position,
                DetectedAtTime = 0f
            });
        }

        /// <summary>
        /// 获取可见目标列表（只读）。
        /// </summary>
        public IReadOnlyList<PerceptionEntry> VisibleTargets => m_VisibleTargets ?? s_EmptyList;

        /// <summary>
        /// 获取声音目标列表（只读）。
        /// </summary>
        public IReadOnlyList<PerceptionEntry> AudioTargets => m_AudioTargets ?? s_EmptyList;

        /// <summary>
        /// 获取威胁目标列表（只读）。
        /// </summary>
        public IReadOnlyList<PerceptionEntry> ThreatTargets => m_ThreatTargets ?? s_EmptyList;

        /// <summary>
        /// 是否有可见目标。
        /// </summary>
        public bool HasVisibleTargets => m_VisibleTargets != null && m_VisibleTargets.Count > 0;

        /// <summary>
        /// 是否有声音目标。
        /// </summary>
        public bool HasAudioTargets => m_AudioTargets != null && m_AudioTargets.Count > 0;

        /// <summary>
        /// 是否有威胁目标。
        /// </summary>
        public bool HasThreatTargets => m_ThreatTargets != null && m_ThreatTargets.Count > 0;

        /// <summary>
        /// 最近的可见目标距离。
        /// 无可见目标时返回 <see cref="float.MaxValue"/>。
        /// </summary>
        public float ClosestVisibleDistance
        {
            get
            {
                if (m_VisibleTargets == null || m_VisibleTargets.Count == 0)
                {
                    return float.MaxValue;
                }

                float min = float.MaxValue;
                for (int i = 0; i < m_VisibleTargets.Count; i++)
                {
                    if (m_VisibleTargets[i].Distance < min)
                    {
                        min = m_VisibleTargets[i].Distance;
                    }
                }

                return min;
            }
        }

        /// <summary>
        /// 最近的威胁目标距离。
        /// 无威胁目标时返回 <see cref="float.MaxValue"/>。
        /// </summary>
        public float ClosestThreatDistance
        {
            get
            {
                if (m_ThreatTargets == null || m_ThreatTargets.Count == 0)
                {
                    return float.MaxValue;
                }

                float min = float.MaxValue;
                for (int i = 0; i < m_ThreatTargets.Count; i++)
                {
                    if (m_ThreatTargets[i].Distance < min)
                    {
                        min = m_ThreatTargets[i].Distance;
                    }
                }

                return min;
            }
        }

        /// <summary>
        /// 清空所有感知数据（每帧开始时调用）。
        /// 不释放列表，仅清空列表内容。
        /// </summary>
        public void ClearFrame()
        {
            m_VisibleTargets?.Clear();
            m_AudioTargets?.Clear();
            m_ThreatTargets?.Clear();
        }

        /// <summary>
        /// 重置对象状态并归还引用池。
        /// </summary>
        public void Clear()
        {
            if (m_VisibleTargets != null)
            {
                ReferencePool.Release(m_VisibleTargets);
                m_VisibleTargets = null;
            }

            if (m_AudioTargets != null)
            {
                ReferencePool.Release(m_AudioTargets);
                m_AudioTargets = null;
            }

            if (m_ThreatTargets != null)
            {
                ReferencePool.Release(m_ThreatTargets);
                m_ThreatTargets = null;
            }
        }
    }

    /// <summary>
    /// 感知条目 — 存储单个感知目标的信息。
    /// 为 class 而非 struct，以配合 <see cref="PooledList{T}"/> 的 class 约束。
    /// </summary>
    public class PerceptionEntry
    {
        /// <summary>
        /// 目标实体 ID
        /// </summary>
        public int EntityId;

        /// <summary>
        /// 置信度 / 威胁等级 (0-1)
        /// </summary>
        public float Confidence;

        /// <summary>
        /// 距离
        /// </summary>
        public float Distance;

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3Data Position;

        /// <summary>
        /// 音频类型（仅音频通道有效）
        /// </summary>
        public AudioType AudioType;

        /// <summary>
        /// 检测时间（秒）
        /// </summary>
        public float DetectedAtTime;
    }

    /// <summary>
    /// 三维坐标数据（纯 C#，无 Unity 依赖）。
    /// 为 class 而非 struct，以配合 <see cref="PooledList{T}"/> 的 class 约束传递。
    /// </summary>
    public class Vector3Data
    {
        /// <summary>
        /// X 分量
        /// </summary>
        public float X;

        /// <summary>
        /// Y 分量
        /// </summary>
        public float Y;

        /// <summary>
        /// Z 分量
        /// </summary>
        public float Z;

        /// <summary>
        /// 创建一个新的三维坐标数据实例。
        /// </summary>
        /// <param name="x">X 分量</param>
        /// <param name="y">Y 分量</param>
        /// <param name="z">Z 分量</param>
        public Vector3Data(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// 计算与另一坐标的欧氏距离。
        /// </summary>
        /// <param name="other">另一坐标</param>
        /// <returns>两点之间的距离</returns>
        public float DistanceTo(Vector3Data other)
        {
            if (other == null)
            {
                return float.MaxValue;
            }

            float dx = X - other.X;
            float dy = Y - other.Y;
            float dz = Z - other.Z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }

    /// <summary>
    /// 音频类型枚举。
    /// </summary>
    public enum AudioType
    {
        /// <summary>
        /// 一般声音
        /// </summary>
        General,

        /// <summary>
        /// 脚步声
        /// </summary>
        Footstep,

        /// <summary>
        /// 枪声
        /// </summary>
        Gunshot,

        /// <summary>
        /// 爆炸声
        /// </summary>
        Explosion,

        /// <summary>
        /// 对话声
        /// </summary>
        Voice
    }
}
