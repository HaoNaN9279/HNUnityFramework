using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 缺失脚本检测规则。
    /// 遍历 Prefab 和 Scene 中的 GameObject，检测 MonoBehaviour 引用断裂。
    /// </summary>
    public class MissingScriptRule : IAssetRule
    {
        /// <inheritdoc />
        public string RuleName => "Missing Script Check";

        /// <inheritdoc />
        public string Description => "Detects missing MonoBehaviour scripts in Prefabs and Scenes.";

        /// <inheritdoc />
        public Type TargetImporterType => null;

        /// <inheritdoc />
        public IReadOnlyList<RuleResult> Validate(string assetPath)
        {
            var results = new List<RuleResult>();

            if (!assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                && !assetPath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
                return results;
            }

            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (root == null)
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
                return results;
            }

            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                GameObject go = t.gameObject;
                Component[] components = go.GetComponents<Component>();

                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        results.Add(RuleResult.Fail(
                            RuleName,
                            assetPath,
                            RuleSeverity.Error,
                            $"Missing script at index {i} on: {GetGameObjectPath(go)}"));
                    }
                }
            }

            if (results.Count == 0)
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
            }

            return results;
        }

        private static string GetGameObjectPath(GameObject go)
        {
            string path = go.name;
            Transform current = go.transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}
