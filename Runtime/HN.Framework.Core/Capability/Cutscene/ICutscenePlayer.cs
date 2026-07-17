using System;

namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 单次过场动画播放会话句柄
    /// </summary>
    public interface ICutscenePlayer
    {
        /// <summary>过场唯一标识</summary>
        string CutsceneKey { get; }

        /// <summary>当前状态</summary>
        CutsceneState State { get; }

        /// <summary>当前播放时间（秒）</summary>
        double CurrentTime { get; }

        /// <summary>总时长（秒），0 表示未知</summary>
        double Duration { get; }

        /// <summary>播放完成回调</summary>
        event Action<ICutscenePlayer> OnFinished;

        /// <summary>状态变更回调</summary>
        event Action<ICutscenePlayer, CutsceneState> OnStateChanged;

        /// <summary>开始或恢复播放</summary>
        void Play();

        /// <summary>暂停播放</summary>
        void Pause();

        /// <summary>继续播放</summary>
        void Resume();

        /// <summary>停止播放并释放资源</summary>
        void Stop();

        /// <summary>跳过过场</summary>
        void Skip();

        /// <summary>跳转到指定时间点</summary>
        void SeekTo(double time);
    }
}
