using System;

namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画管理器接口，负责过场的全局管理、优先级仲裁和生命周期协调。
    /// 支持四种优先级：Critical > Important > Normal > Background。
    /// 播放时自动协调输入阻塞（C14 Input IInputBlocker）、摄像机接管（C17 Camera ICameraManager）、UI 控制（C13 UI）。
    /// </summary>
    public interface ICutsceneManager
    {
        /// <summary>全局设置</summary>
        CutsceneGlobalSettings GlobalSettings { get; set; }

        /// <summary>当前是否有过场正在播放</summary>
        bool IsAnyPlaying { get; }
        
        /// <summary>当前活跃的播放器数量</summary>
        int ActivePlayerCount { get; }
        
        /// <summary>排队队列中的过场数量</summary>
        int QueuedCount { get; }

        /// <summary>
        /// 播放过场动画
        /// </summary>
        /// <param name="cutsceneKey">过场资源标识（Addressables key）</param>
        /// <param name="priority">过场优先级</param>
        /// <param name="skipMode">跳过模式</param>
        /// <param name="bindingMap">角色绑定映射，null 则使用默认绑定</param>
        /// <returns>播放会话句柄</returns>
        ICutscenePlayer Play(
            string cutsceneKey, 
            CutscenePriority priority = CutscenePriority.Normal,
            CutsceneSkipMode skipMode = CutsceneSkipMode.None,
            CutsceneBindingMap? bindingMap = null);

        /// <summary>
        /// 将过场加入播放队列（仅 Normal 和 Background 优先级）
        /// </summary>
        void Enqueue(
            string cutsceneKey,
            CutscenePriority priority = CutscenePriority.Normal,
            CutsceneSkipMode skipMode = CutsceneSkipMode.None,
            CutsceneBindingMap? bindingMap = null);

        /// <summary>停止所有正在播放的过场</summary>
        void StopAll();

        /// <summary>暂停所有过场</summary>
        void PauseAll();

        /// <summary>恢复所有暂停的过场</summary>
        void ResumeAll();

        /// <summary>跳过当前播放的过场</summary>
        void SkipCurrent();

        /// <summary>清空排队队列</summary>
        void ClearQueue();
    }
}
