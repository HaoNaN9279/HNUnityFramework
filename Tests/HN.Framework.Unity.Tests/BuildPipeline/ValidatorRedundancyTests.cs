using System.Linq;
using HN.Framework.Editor.BuildPipeline;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Unity.Tests.BuildPipeline
{
    [TestFixture]
    public class ValidatorRedundancyTests
    {
        // ─── AssetValidator ──────────────────────────────────────────

        [Test]
        public void AssetValidator_Create_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => new AssetValidator());
        }

        [Test]
        public void AssetValidator_AddRule_DoesNotThrow()
        {
            var validator = new AssetValidator();
            var rule = new MissingScriptRule();
            Assert.DoesNotThrow(() => validator.AddRule(rule));
        }

        [Test]
        public void AssetValidator_AddNullRule_Throws()
        {
            var validator = new AssetValidator();
            Assert.Throws<System.ArgumentNullException>(() => validator.AddRule(null));
        }

        [Test]
        public void AssetValidator_SingleKnownAsset_DoesNotThrow()
        {
            var validator = new AssetValidator();
            Assert.DoesNotThrow(() =>
                validator.ValidateSingle("Assets/Editor/BuildPipeline/Rules/Presets/MissingScriptRule.cs"));
        }

        [Test]
        public void AssetValidator_ValidateNonExistent_ReturnsResults()
        {
            var validator = new AssetValidator();
            // 注册一个全局规则以保证返回结果
            AssetRuleRegistry.RegisterGlobalRule(new NamingConventionRule("^.*$", "test"));
            var results = validator.ValidateSingle("Assets/NonexistentAsset.xyz");
            AssetRuleRegistry.Clear();
            Assert.IsNotNull(results);
        }

        // ─── DependencyGraph ─────────────────────────────────────────

        [Test]
        public void DependencyGraph_Create_IsNotBuilt()
        {
            var graph = new DependencyGraph();
            Assert.IsFalse(graph.IsBuilt);
        }

        [Test]
        public void DependencyGraph_Build_Completes()
        {
            var graph = new DependencyGraph();
            Assert.DoesNotThrow(() => graph.Build("Assets", null));
            Assert.IsTrue(graph.IsBuilt);
        }

        [Test]
        public void DependencyGraph_Build_HasEntries()
        {
            var graph = new DependencyGraph();
            graph.Build("Assets", null);
            Assert.IsTrue(graph.AllAssets.Count > 0);
        }

        [Test]
        public void DependencyGraph_GetDependencies_KnownAsset_ReturnsCollection()
        {
            var graph = new DependencyGraph();
            graph.Build("Assets", null);
            var deps = graph.GetDependencies("Assets/Editor/BuildPipeline/Rules/Presets/MissingScriptRule.cs");
            Assert.IsNotNull(deps);
        }

        [Test]
        public void DependencyGraph_GetDependencies_UnknownAsset_ReturnsEmpty()
        {
            var graph = new DependencyGraph();
            graph.Build("Assets", null);
            var deps = graph.GetDependencies("Assets/Nonexistent.xyz");
            Assert.AreEqual(0, deps.Count);
        }

        [Test]
        public void DependencyGraph_GetStats_ReturnsFormattedString()
        {
            var graph = new DependencyGraph();
            graph.Build("Assets", null);
            string stats = graph.GetStats();
            Assert.IsTrue(stats.Contains("Assets:"));
        }

        // ─── RedundancyScanner ───────────────────────────────────────

        [Test]
        public void RedundancyScanner_Create_NullGraph_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => new RedundancyScanner(null));
        }

        [Test]
        public void RedundancyScanner_Scan_BeforeBuild_Throws()
        {
            var graph = new DependencyGraph();
            var scanner = new RedundancyScanner(graph);
            Assert.Throws<System.InvalidOperationException>(() => scanner.Scan(null));
        }

        [Test]
        public void RedundancyScanner_Scan_Completes()
        {
            var graph = new DependencyGraph();
            graph.Build("Assets", null);
            var scanner = new RedundancyScanner(graph);
            Assert.DoesNotThrow(() => scanner.Scan(null));
            Assert.IsTrue(scanner.IsScanned);
        }

        [Test]
        public void RedundancyScanner_GetEstimatedSavings_ReturnsNonNegative()
        {
            var graph = new DependencyGraph();
            graph.Build("Assets", null);
            var scanner = new RedundancyScanner(graph);
            scanner.Scan(null);
            long savings = scanner.GetEstimatedSavings();
            Assert.GreaterOrEqual(savings, 0);
        }


    }
}
