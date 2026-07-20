using System.Linq;
using HN.Framework.Editor.BuildPipeline;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.BuildPipeline
{
    [TestFixture]
    public class PresetRulesTests
    {
        // ─── NamingConventionRule ────────────────────────────────────

        [Test]
        public void NamingConventionRule_ValidName_Passes()
        {
            var rule = NamingConventionRule.CreateDefault();
            var results = rule.Validate("Assets/MyTexture.png");
            Assert.IsTrue(results.All(r => r.Passed));
        }

        [Test]
        public void NamingConventionRule_NullFileName_Passes()
        {
            // getcwd - 当前工作目录路径，应该存在
            var rule = NamingConventionRule.CreateDefault();
            var results = rule.Validate("Packages/manifest.json");
            Assert.IsTrue(results.All(r => r.Passed));
        }

        [Test]
        public void NamingConventionRule_CustomPattern_ValidFilename_Passes()
        {
            var rule = new NamingConventionRule(@"^[A-Za-z][A-Za-z0-9]*$", "PascalCase");
            var results = rule.Validate("Assets/Example/Prefab.prefab");
            Assert.IsTrue(results.All(r => r.Passed));
        }

        // ─── MissingScriptRule ───────────────────────────────────────

        [Test]
        public void MissingScriptRule_NonPrefab_Passes()
        {
            var rule = new MissingScriptRule();
            var results = rule.Validate("Assets/SomeTextAsset.txt");
            Assert.IsTrue(results.All(r => r.Passed));
        }

        [Test]
        public void MissingScriptRule_NonExistentPrefab_Passes()
        {
            var rule = new MissingScriptRule();
            var results = rule.Validate("Assets/Nonexistent.prefab");
            Assert.IsTrue(results.All(r => r.Passed));
        }

        // ─── ReferenceIntegrityRule ──────────────────────────────────

        [Test]
        public void ReferenceIntegrityRule_NonExistentAsset_Fails()
        {
            var rule = new ReferenceIntegrityRule();
            var results = rule.Validate("Assets/NonexistentAsset.xyz");
            Assert.IsTrue(results.Any(r => !r.Passed));
        }

        [Test]
        public void ReferenceIntegrityRule_NonExistentAsset_HasCorrectRuleName()
        {
            var rule = new ReferenceIntegrityRule();
            var results = rule.Validate("Assets/NonexistentAsset.xyz");
            Assert.AreEqual("Reference Integrity Check", results[0].RuleName);
        }

        [Test]
        public void ReferenceIntegrityRule_NonExistentAsset_HasErrorSeverity()
        {
            var rule = new ReferenceIntegrityRule();
            var results = rule.Validate("Assets/NonexistentAsset.xyz");
            Assert.IsTrue(results.Any(r => r.Severity >= RuleSeverity.Error));
        }

        // ─── AddressablesRule ────────────────────────────────────────

        [Test]
        public void AddressablesRule_WithoutPackage_Passes()
        {
            var rule = new AddressablesRule();
            var results = rule.Validate("Assets/SomeAsset.png");
            Assert.IsTrue(results.All(r => r.Passed));
        }

        // ─── RuleResult ──────────────────────────────────────────────

        [Test]
        public void RuleResult_Pass_CreatesCorrectResult()
        {
            var result = RuleResult.Pass("TestRule", "Assets/test.png");
            Assert.AreEqual("TestRule", result.RuleName);
            Assert.AreEqual("Assets/test.png", result.AssetPath);
            Assert.AreEqual(RuleSeverity.Info, result.Severity);
            Assert.IsTrue(result.Passed);
        }

        [Test]
        public void RuleResult_Fail_CreatesCorrectResult()
        {
            var result = RuleResult.Fail(
                "TestRule", "Assets/test.png", RuleSeverity.Error, "Test failure");
            Assert.AreEqual("TestRule", result.RuleName);
            Assert.AreEqual("Assets/test.png", result.AssetPath);
            Assert.AreEqual(RuleSeverity.Error, result.Severity);
            Assert.AreEqual("Test failure", result.Message);
            Assert.IsFalse(result.Passed);
        }

        [Test]
        public void RuleResult_Equals_SameValues_ReturnsTrue()
        {
            var a = RuleResult.Pass("R1", "path");
            var b = RuleResult.Pass("R1", "path");
            Assert.AreEqual(a, b);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void RuleResult_Equals_DifferentValues_ReturnsFalse()
        {
            var a = RuleResult.Pass("R1", "path1");
            var b = RuleResult.Pass("R2", "path2");
            Assert.AreNotEqual(a, b);
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void RuleSeverity_Values_AreCorrect()
        {
            Assert.AreEqual(0, (int)RuleSeverity.Info);
            Assert.AreEqual(1, (int)RuleSeverity.Warning);
            Assert.AreEqual(2, (int)RuleSeverity.Error);
            Assert.AreEqual(3, (int)RuleSeverity.Fatal);
        }
    }
}
