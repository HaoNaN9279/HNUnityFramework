using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Unity.Capability.Cutscene;

namespace HN.Framework.Editor.Cutscene
{
    public class CutsceneEditorWindow : EditorWindow
    {
        private string _cutsceneKey = "Intro";
        private TimelineAsset _selectedTimeline;
        private Vector2 _scrollPos;
        private Dictionary<string, string> _bindings = new Dictionary<string, string>();
        private BindingResolveMode _resolveMode = BindingResolveMode.ScenePath;
        private bool _isPlaying;
        private double _previewTime;

        [MenuItem(HNUnityFrameworkConstants.FRAMEWORK_NAME + "/Cutscene Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<CutsceneEditorWindow>("Cutscene Editor");
            window.Show();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeaderSection();
            DrawBindingSection();
            DrawPreviewSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeaderSection()
        {
            EditorGUILayout.LabelField("Cutscene Asset", EditorStyles.boldLabel);
            _cutsceneKey = EditorGUILayout.TextField("Cutscene Key", _cutsceneKey);

            _selectedTimeline = (TimelineAsset)EditorGUILayout.ObjectField(
                "Timeline Asset", _selectedTimeline, typeof(TimelineAsset), false);

            _resolveMode = (BindingResolveMode)EditorGUILayout.EnumPopup("Resolve Mode", _resolveMode);

            EditorGUILayout.Space();
        }

        private void DrawBindingSection()
        {
            EditorGUILayout.LabelField("Role Bindings", EditorStyles.boldLabel);

            if (_selectedTimeline != null)
            {
                var outputs = _selectedTimeline.outputs;
                foreach (var output in outputs)
                {
                    var bindingName = output.sourceObject != null ? output.sourceObject.name : output.streamName;
                    if (!_bindings.ContainsKey(bindingName))
                    {
                        _bindings[bindingName] = "";
                    }

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(bindingName, GUILayout.Width(120));
                    _bindings[bindingName] = EditorGUILayout.TextField(_bindings[bindingName]);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a Timeline Asset to see available bindings.", MessageType.Info);
            }

            EditorGUILayout.Space();
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = _selectedTimeline != null;
            if (GUILayout.Button("Play Preview", GUILayout.Height(30)))
            {
                PlayPreview();
            }

            if (GUILayout.Button("Stop", GUILayout.Height(30)))
            {
                StopPreview();
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            if (_selectedTimeline != null)
            {
                _previewTime = EditorGUILayout.Slider("Seek", (float)_previewTime, 0f, (float)_selectedTimeline.duration);
                EditorGUILayout.LabelField("Time: " + _previewTime.ToString("F2") + "s / " + _selectedTimeline.duration.ToString("F2") + "s");
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Binding Map Asset"))
            {
                CreateBindingMapAsset();
            }
        }

        private void PlayPreview()
        {
            if (_selectedTimeline == null) return;

            var bindingMap = new CutsceneBindingMap(_resolveMode, _bindings);
            var go = new GameObject("[CutscenePreview] ");
            var director = go.AddComponent<PlayableDirector>();
            director.playableAsset = _selectedTimeline;

            // Apply bindings
            foreach (var binding in _bindings)
            {
                if (string.IsNullOrEmpty(binding.Value)) continue;
                var resolved = GameObject.Find(binding.Value);
                if (resolved != null)
                {
                    director.SetReferenceValue(new PropertyName(binding.Key), resolved);
                }
            }

            director.Play(_selectedTimeline);
            _isPlaying = true;
            Debug.Log("[CutsceneEditor] Preview started: " + _cutsceneKey);
        }

        private void StopPreview()
        {
            var previewGO = GameObject.Find("[CutscenePreview]");
            if (previewGO != null)
            {
                var director = previewGO.GetComponent<PlayableDirector>();
                if (director != null)
                    director.Stop();
                DestroyImmediate(previewGO);
            }
            _isPlaying = false;
        }

        private void CreateBindingMapAsset()
        {
            var bindingMap = new CutsceneBindingMap(_resolveMode, _bindings);
            var json = JsonUtility.ToJson(bindingMap, true);
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Binding Map",
                _cutsceneKey + "_bindings",
                "json",
                "Save binding map asset"
            );

            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                Debug.Log("[CutsceneEditor] Binding map saved: " + path);
            }
        }
    }
}