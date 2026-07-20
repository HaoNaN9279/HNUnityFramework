using System;
using System.Collections.Generic;
using HN.Framework.Editor.BuildPipeline;
using NUnit.Framework;
using UnityEditor;

namespace HN.Framework.Unity.Tests.BuildPipeline
{
    [TestFixture]
    public class AssetRuleRegistryTests
    {
        private class MockRule : IAssetRule
        {
            public string RuleName { get; set; }
            public string Description { get; set; }
            public Type TargetImporterType { get; set; }

            public IReadOnlyList<RuleResult> Validate(string assetPath)
            {
                return Array.Empty<RuleResult>();
            }
        }

        [SetUp]
        public void SetUp()
        {
            AssetRuleRegistry.Clear();
        }

        [Test]
        public void RegisterRule_AddsRuleToImporterType()
        {
            var rule = new MockRule
            {
                RuleName = "TestRule",
                TargetImporterType = typeof(TextureImporter),
            };

            AssetRuleRegistry.RegisterRule<TextureImporter>(rule);

            Assert.AreEqual(1, AssetRuleRegistry.GetRuleCount(typeof(TextureImporter)));
        }

        [Test]
        public void RegisterRule_MultipleRules_SameImporter_AllAdded()
        {
            var rule1 = new MockRule { RuleName = "Rule1", TargetImporterType = typeof(TextureImporter) };
            var rule2 = new MockRule { RuleName = "Rule2", TargetImporterType = typeof(TextureImporter) };

            AssetRuleRegistry.RegisterRule<TextureImporter>(rule1);
            AssetRuleRegistry.RegisterRule<TextureImporter>(rule2);

            var rules = AssetRuleRegistry.GetRulesForImporter(typeof(TextureImporter));
            Assert.AreEqual(2, rules.Count);
        }

        [Test]
        public void RegisterRule_NullRule_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                AssetRuleRegistry.RegisterRule<TextureImporter>(null));
        }

        [Test]
        public void GetRulesForImporter_NoRules_ReturnsEmpty()
        {
            var rules = AssetRuleRegistry.GetRulesForImporter(typeof(TextureImporter));
            Assert.AreEqual(0, rules.Count);
        }

        [Test]
        public void GetRulesForImporter_IncludesGlobalRules()
        {
            var globalRule = new MockRule { RuleName = "GlobalRule" };
            var specificRule = new MockRule
            {
                RuleName = "SpecificRule",
                TargetImporterType = typeof(ModelImporter),
            };

            AssetRuleRegistry.RegisterGlobalRule(globalRule);
            AssetRuleRegistry.RegisterRule<ModelImporter>(specificRule);

            var rules = AssetRuleRegistry.GetRulesForImporter(typeof(ModelImporter));
            Assert.AreEqual(2, rules.Count);
        }

        [Test]
        public void RegisterGlobalRule_AppliesToAllImporters()
        {
            var globalRule = new MockRule { RuleName = "GlobalRule" };

            AssetRuleRegistry.RegisterGlobalRule(globalRule);

            var textureRules = AssetRuleRegistry.GetRulesForImporter(typeof(TextureImporter));
            var modelRules = AssetRuleRegistry.GetRulesForImporter(typeof(ModelImporter));

            Assert.AreEqual(1, textureRules.Count);
            Assert.AreEqual(1, modelRules.Count);
        }

        [Test]
        public void GetAllRules_ReturnsAllRegisteredRules()
        {
            var globalRule = new MockRule { RuleName = "Global" };
            var textureRule = new MockRule
            {
                RuleName = "TextureRule",
                TargetImporterType = typeof(TextureImporter),
            };

            AssetRuleRegistry.RegisterGlobalRule(globalRule);
            AssetRuleRegistry.RegisterRule<TextureImporter>(textureRule);

            var allRules = AssetRuleRegistry.GetAllRules();
            Assert.AreEqual(2, allRules.Count);
        }

        [Test]
        public void Clear_RemovesAllRules()
        {
            var rule = new MockRule
            {
                RuleName = "TestRule",
                TargetImporterType = typeof(TextureImporter),
            };

            AssetRuleRegistry.RegisterRule<TextureImporter>(rule);

            AssetRuleRegistry.Clear();

            Assert.AreEqual(0, AssetRuleRegistry.GetRuleCount(typeof(TextureImporter)));
            Assert.AreEqual(0, AssetRuleRegistry.GetGlobalRuleCount());
        }

        [Test]
        public void GetRegisteredImporterTypes_ReturnsCorrectTypes()
        {
            AssetRuleRegistry.RegisterRule<TextureImporter>(
                new MockRule
                {
                    RuleName = "T1",
                    TargetImporterType = typeof(TextureImporter),
                });
            AssetRuleRegistry.RegisterRule<ModelImporter>(
                new MockRule
                {
                    RuleName = "M1",
                    TargetImporterType = typeof(ModelImporter),
                });

            var types = AssetRuleRegistry.GetRegisteredImporterTypes();
            Assert.Contains(typeof(TextureImporter), new List<Type>(types));
            Assert.Contains(typeof(ModelImporter), new List<Type>(types));
        }
    }
}
