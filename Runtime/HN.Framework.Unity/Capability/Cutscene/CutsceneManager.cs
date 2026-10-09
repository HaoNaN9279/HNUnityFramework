using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Capability.Camera;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Cutscene
{
    public class CutsceneManager : ICutsceneManager, IDisposable
    {
        /// <summary>
        /// 缓存的过场全局配置。由模块初始化时设置。
        /// </summary>
        internal static CutsceneSettings? s_CachedSettings;

        /// <summary>
        /// 演员注册表实例。由本管理器创建/持有（或由外部注入），
        /// 并透传给每个 <see cref="CutscenePlayer"/> → <see cref="CutsceneBindingResolver"/>。
        /// </summary>
        public ICutsceneActorRegistry ActorRegistry { get; }

        private readonly bool _ownsRegistry;

        /// <summary>
        /// 构造过场管理器。
        /// </summary>
        /// <param name="actorRegistry">
        /// 演员注册表；为 null 时内部创建默认的 <see cref="CutsceneActorRegistry"/>（由本管理器负责释放）。
        /// 传入自定义实现以替换查找来源或便于测试隔离。
        /// </param>
        public CutsceneManager(ICutsceneActorRegistry? actorRegistry = null)
        {
            _ownsRegistry = actorRegistry == null;
            ActorRegistry = actorRegistry ?? new CutsceneActorRegistry();
        }

        public IInputBlocker InputBlocker { get; set; }
        public ICameraManager CameraManager { get; set; }

        /// <summary>
        /// 全局配置。优先读取 <see cref="CutsceneSettings"/> SO，回退到 struct 默认值。
        /// </summary>
#pragma warning disable CS0618 // 保留对旧 struct 的兼容
        public CutsceneGlobalSettings GlobalSettings
        {
            get
            {
                if (s_CachedSettings != null)
                {
                    return new CutsceneGlobalSettings
                    {
                        GlobalSkipEnabled = s_CachedSettings.GlobalSkipEnabled,
                        GlobalSpeedMultiplier = s_CachedSettings.GlobalSpeedMultiplier
                    };
                }
                return CutsceneGlobalSettings.Default;
            }
            set { }
        }
#pragma warning restore CS0618
        public bool IsAnyPlaying => _activePlayers.Count > 0;
        public int ActivePlayerCount => _activePlayers.Count;
        public int QueuedCount => _queue.Count;

        private readonly List<ICutscenePlayer> _activePlayers = new List<ICutscenePlayer>();
        private readonly Queue<(string key, CutscenePriority priority, CutsceneSkipMode skipMode, CutsceneBindingMap? binding)> _queue
            = new Queue<(string, CutscenePriority, CutsceneSkipMode, CutsceneBindingMap?)>();

        public ICutscenePlayer Play(string cutsceneKey, CutscenePriority priority = CutscenePriority.Normal,
            CutsceneSkipMode skipMode = CutsceneSkipMode.None, CutsceneBindingMap? bindingMap = null)
        {
            if (priority == CutscenePriority.Critical)
            {
                StopAll();
            }
            else if (priority == CutscenePriority.Important)
            {
                foreach (var player in _activePlayers.ToArray())
                {
                    if (player.State == CutsceneState.Playing)
                        player.Pause();
                }
            }
            else if (priority <= CutscenePriority.Normal && IsAnyPlaying)
            {
                Enqueue(cutsceneKey, priority, skipMode, bindingMap);
                return null;
            }

            return CreateAndPlay(cutsceneKey, bindingMap);
        }

        public void Enqueue(string cutsceneKey, CutscenePriority priority = CutscenePriority.Normal,
            CutsceneSkipMode skipMode = CutsceneSkipMode.None, CutsceneBindingMap? bindingMap = null)
        {
            if (priority >= CutscenePriority.Important)
            {
                Play(cutsceneKey, priority, skipMode, bindingMap);
                return;
            }

            _queue.Enqueue((cutsceneKey, priority, skipMode, bindingMap));
        }

        public void StopAll()
        {
            foreach (var player in _activePlayers.ToArray())
            {
                if (player is CutscenePlayer cp)
                    cp.Cleanup();
            }
            _activePlayers.Clear();
            PopInputBlocker();
        }

        public void PauseAll()
        {
            foreach (var player in _activePlayers)
            {
                if (player.State == CutsceneState.Playing)
                    player.Pause();
            }
        }

        public void ResumeAll()
        {
            foreach (var player in _activePlayers)
            {
                if (player.State == CutsceneState.Paused)
                    player.Resume();
            }
        }

        public void SkipCurrent()
        {
            if (_activePlayers.Count > 0)
            {
                var current = _activePlayers[0];
                current.Skip();
                OnPlayerFinished(current);
            }
        }

        public void ClearQueue()
        {
            _queue.Clear();
        }

        public void Tick()
        {
            if (!IsAnyPlaying && _queue.Count > 0)
            {
                var next = _queue.Dequeue();
                CreateAndPlay(next.key, next.binding);
            }
        }

        private ICutscenePlayer CreateAndPlay(string cutsceneKey, CutsceneBindingMap? bindingMap)
        {
            var player = new CutscenePlayer(cutsceneKey, bindingMap, cutsceneKey, ActorRegistry);
            player.OnFinished += OnPlayerFinished;
            player.OnStateChanged += OnPlayerStateChanged;

            PushInputBlocker();
            SwitchToCutsceneCamera();

            player.Play();
            _activePlayers.Add(player);
            return player;
        }

        private void OnPlayerFinished(ICutscenePlayer player)
        {
            _activePlayers.Remove(player);
            if (player is CutscenePlayer cp)
                cp.Cleanup();

            if (_activePlayers.Count == 0)
            {
                PopInputBlocker();
                RestoreCamera();
            }
        }

        private void OnPlayerStateChanged(ICutscenePlayer player, CutsceneState state)
        {
        }

        private int _inputBlockToken = 0;

        private void PushInputBlocker()
        {
            if (InputBlocker != null)
            {
                _inputBlockToken++;
                InputBlocker.Push(this, _inputBlockToken * 100);
            }
        }

        private void PopInputBlocker()
        {
            if (InputBlocker != null)
            {
                InputBlocker.Pop(this);
            }
        }

        private string _previousCamera;

        private void SwitchToCutsceneCamera()
        {
            if (CameraManager != null)
            {
                _previousCamera = CameraManager.GetActiveCamera();
                CameraManager.SetActiveCamera("CutsceneCamera");
            }
        }

        private void RestoreCamera()
        {
            if (CameraManager != null && !string.IsNullOrEmpty(_previousCamera))
            {
                CameraManager.SetActiveCamera(_previousCamera);
                _previousCamera = null;
            }
        }

        /// <summary>
        /// 释放由本管理器持有的资源。
        /// 仅当注册表由本管理器创建（构造函数未外部注入）时才释放它，
        /// 避免误释放外部注入的实现。
        /// </summary>
        public void Dispose()
        {
            if (_ownsRegistry && ActorRegistry is IDisposable disposable)
                disposable.Dispose();
        }
    }
}