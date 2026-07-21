using System.Collections.Generic;
using HN.Framework.Editor.BuildPipeline;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.BuildPipeline
{
    [TestFixture]
    public class PipelineTests
    {
        [Test]
        public void BuildContext_Create_HasDefaults()
        {
            var ctx = new BuildContext();
            Assert.IsNotNull(ctx.CustomData);
            Assert.IsNotNull(ctx.StepLogs);
            Assert.IsFalse(ctx.IsCancelled);
        }

        [Test]
        public void BuildPipelineOrchestrator_EmptySteps_ReturnsTrue()
        {
            var orchestrator = new BuildPipelineOrchestrator();
            bool result = orchestrator.Execute();
            Assert.IsTrue(result);
        }

        [Test]
        public void BuildPipelineOrchestrator_AddStep_IncreasesCount()
        {
            var orchestrator = new BuildPipelineOrchestrator();
            var step = new PreBuildValidationStep();
            orchestrator.AddStep(step);
            Assert.AreEqual(1, orchestrator.Steps.Count);
        }

        [Test]
        public void BuildPipelineOrchestrator_NullStep_Throws()
        {
            var orchestrator = new BuildPipelineOrchestrator();
            Assert.Throws<System.ArgumentNullException>(() => orchestrator.AddStep(null));
        }

        [Test]
        public void VersionConfig_GetVersionString_DefaultIsCorrect()
        {
            var config = ScriptableObject.CreateInstance<VersionConfig>();
            string version = config.GetVersionString();
            Assert.AreEqual("1.0.0", version);
        }

        [Test]
        public void VersionConfig_IncrementBuildNumber_ChangesVersion()
        {
            var config = ScriptableObject.CreateInstance<VersionConfig>();
            config.IncrementBuildNumber();
            Assert.AreEqual(1, config.BuildNumber);
        }

        [Test]
        public void VersionConfig_IncrementMajor_ResetsLower()
        {
            var config = ScriptableObject.CreateInstance<VersionConfig>();
            config.Minor = 5;
            config.Patch = 3;
            config.IncrementMajor();
            Assert.AreEqual(2, config.Major);
            Assert.AreEqual(0, config.Minor);
            Assert.AreEqual(0, config.Patch);
        }

        [Test]
        public void BuildManifest_HasDefaultValues()
        {
            var manifest = new BuildManifest();
            Assert.IsNull(manifest.Version);
            Assert.AreEqual(0, manifest.BuildNumber);
        }

        [Test]
        public void BuildReportGenerator_GenerateJson_ReturnsValidString()
        {
            var manifest = new BuildManifest
            {
                Version = "1.0.0",
                BuildTime = "2026-01-01",
                Platform = "StandaloneWindows64",
                BuildNumber = 1,
            };
            string json = BuildReportGenerator.GenerateJson(manifest, new List<RuleResult>());
            Assert.IsTrue(json.Contains("1.0.0"));
            Assert.IsTrue(json.Contains("StandaloneWindows64"));
        }

        [Test]
        public void BuildReportGenerator_GenerateMarkdown_ReturnsFormattedString()
        {
            var manifest = new BuildManifest
            {
                Version = "1.0.0",
                BuildTime = "2026-01-01",
                Platform = "Android",
            };
            string md = BuildReportGenerator.GenerateMarkdown(manifest, new List<RuleResult>());
            Assert.IsTrue(md.Contains("# Build Report"));
            Assert.IsTrue(md.Contains("1.0.0"));
        }

        [Test]
        public void PreBuildValidationStep_HasCorrectName()
        {
            var step = new PreBuildValidationStep();
            Assert.AreEqual("Pre-Build Validation", step.StepName);
        }

        [Test]
        public void AddressablesBuildStep_HasCorrectName()
        {
            var step = new AddressablesBuildStep();
            Assert.AreEqual("Addressables Build", step.StepName);
        }

        [Test]
        public void PlayerBuildStep_HasCorrectName()
        {
            var step = new PlayerBuildStep();
            Assert.AreEqual("Player Build", step.StepName);
        }

        [Test]
        public void PostBuildStep_HasCorrectName()
        {
            var step = new PostBuildStep();
            Assert.AreEqual("Post-Build Processing", step.StepName);
        }
    }
}
