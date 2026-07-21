using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.Cutscene;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HN.Framework.Unity.Capability.Cutscene
{
    public class CutscenePlayer : ICutscenePlayer
    {
        public string CutsceneKey { get; }
        public CutsceneState State { get; private set; } = CutsceneState.Idle;
        public double CurrentTime => _director != null ? _director.time : 0;
        public double Duration => _director != null && _director.playableAsset != null ? _director.playableAsset.duration : 0;

        public event Action<ICutscenePlayer> OnFinished;
        public event Action<ICutscenePlayer, CutsceneState> OnStateChanged;

        private PlayableDirector _director;
        private GameObject _directorGO;
        private readonly CutsceneBindingResolver _resolver;
        private readonly string _addressablesKey;
        private AsyncOperationHandle<PlayableAsset> _loadHandle;
        private bool _isLoaded;

        public CutscenePlayer(string cutsceneKey, CutsceneBindingMap? bindingMap, string addressablesKey)
        {
            CutsceneKey = cutsceneKey;
            _addressablesKey = addressablesKey;
            _resolver = new CutsceneBindingResolver(bindingMap ?? new CutsceneBindingMap());
        }

        public void Play()
        {
            if (State == CutsceneState.Playing) return;

            if (!_isLoaded)
            {
                LoadAsset();
                return;
            }

            if (_director != null)
            {
                ApplyBindings();
                _director.time = 0;
                _director.Play();
                SetState(CutsceneState.Playing);
            }
        }



        public void Pause()
        {
            if (_director != null && State == CutsceneState.Playing)
            {
                _director.Pause();
                SetState(CutsceneState.Paused);
            }
        }

        public void Resume()
        {
            if (_director != null && State == CutsceneState.Paused)
            {
                ApplyBindings();
                _director.Play();
                SetState(CutsceneState.Playing);
            }
        }

        public void Stop()
        {
            if (_director != null)
            {
                _director.Stop();
                SetState(CutsceneState.Idle);
            }
        }

        public void Skip()
        {
            if (_director != null)
            {
                _director.time = Duration;
                _director.Evaluate();
                SetState(CutsceneState.Skipped);
                OnFinished?.Invoke(this);
            }
        }

        public void SeekTo(double time)
        {
            if (_director != null && _director.playableAsset != null)
            {
                time = Math.Max(0, Math.Min(time, _director.playableAsset.duration));
                _director.time = time;
                _director.Evaluate();
            }
        }

        public void Cleanup()
        {
            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);

            if (_directorGO != null)
            {
                UnityEngine.Object.Destroy(_directorGO);
                _directorGO = null;
                _director = null;
            }

            _isLoaded = false;
            SetState(CutsceneState.Idle);
        }

        private void LoadAsset()
        {
            if (string.IsNullOrEmpty(_addressablesKey)) return;

            SetState(CutsceneState.Stopping);
            _loadHandle = Addressables.LoadAssetAsync<PlayableAsset>(_addressablesKey);
            _loadHandle.Completed += OnAssetLoaded;
        }

        private void OnAssetLoaded(AsyncOperationHandle<PlayableAsset> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                _directorGO = new GameObject("[Cutscene] " + CutsceneKey);
                _director = _directorGO.AddComponent<PlayableDirector>();
                _director.playableAsset = handle.Result;
                _director.stopped += OnDirectorStopped;
                _isLoaded = true;

                ApplyBindings();
                _director.Play();
                SetState(CutsceneState.Playing);
            }
            else
            {
                UnityEngine.Debug.LogError("[CutscenePlayer] Failed to load Timeline asset: " + _addressablesKey);
                SetState(CutsceneState.Idle);
                OnFinished?.Invoke(this);
            }
        }

        private void ApplyBindings()
        {
            foreach (var name in _resolver.GetReferenceNames())
            {
                var resolved = _resolver.GetReferenceValue(name, out var valid);
                if (valid && resolved != null && _director != null)
                {
                    _director.SetReferenceValue(name, resolved);
                }
            }
        }

        private void OnDirectorStopped(PlayableDirector director)
        {
            if (State == CutsceneState.Playing || State == CutsceneState.Paused)
            {
                SetState(CutsceneState.Finished);
                OnFinished?.Invoke(this);
            }
        }

        private void SetState(CutsceneState newState)
        {
            if (State == newState) return;
            State = newState;
            OnStateChanged?.Invoke(this, newState);
        }
    }
}