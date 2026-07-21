using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.Capability.Cutscene;

namespace HN.Framework.Editor.Cutscene
{
    [CustomEditor(typeof(CutsceneActor))]
    public class CutsceneBindingInspector : UnityEditor.Editor
    {
        private SerializedProperty _actorRoleProp;
        private SerializedProperty _actorTagProp;
        private SerializedProperty _entityIdProp;

        private void OnEnable()
        {
            _actorRoleProp = serializedObject.FindProperty("ActorRole");
            _actorTagProp = serializedObject.FindProperty("ActorTag");
            _entityIdProp = serializedObject.FindProperty("EntityId");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Cutscene Actor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_actorRoleProp, new GUIContent("Actor Role"));
            EditorGUILayout.PropertyField(_actorTagProp, new GUIContent("Actor Tag"));
            EditorGUILayout.PropertyField(_entityIdProp, new GUIContent("Entity ID"));

            EditorGUILayout.Space();

            var actor = (CutsceneActor)target;
            EditorGUILayout.LabelField("Binding Info", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Scene Path", actor.gameObject.name);
            EditorGUILayout.LabelField("Tag", actor.gameObject.tag);

            if (GUILayout.Button("Copy Scene Path"))
            {
                var path = GetScenePath(actor.gameObject);
                EditorGUIUtility.systemCopyBuffer = path;
                Debug.Log("[Cutscene] Copied scene path: " + path);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static string GetScenePath(GameObject obj)
        {
            string path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }
    }
}