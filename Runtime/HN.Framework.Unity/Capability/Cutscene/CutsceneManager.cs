using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Core.Capability.Input;
using HN.Framework.Core.Capability.Camera;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Cutscene
{
    public class CutsceneManager : ICutsceneManager
    {
        public IInputBlocker InputBlocker { get; set; }
        public ICameraManager CameraManager { get; set; }

        public CutsceneGlobalSettings GlobalSettings { get; set; } = CutsceneGlobalSettings.Default;
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
            var player = new CutscenePlayer(cutsceneKey, bindingMap, cutsceneKey);
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
    }
}