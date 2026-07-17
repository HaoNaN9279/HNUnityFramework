
using HN.Framework.Core.Capability.Cutscene;
using HN.Framework.Unity.Capability.Cutscene;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.Cutscene
{
    [TestFixture]
    public class CutscenePlayerTests
    {
        [Test]
        public void Player_DefaultState_IsIdle()
        {
            var player = new CutscenePlayer("test", null, "test_key");
            Assert.That(player.State, Is.EqualTo(CutsceneState.Idle));
        }

        [Test]
        public void Player_CutsceneKey_MatchesConstructor()
        {
            var player = new CutscenePlayer("intro_cutscene", null, "addr_key");
            Assert.That(player.CutsceneKey, Is.EqualTo("intro_cutscene"));
        }

        [Test]
        public void Player_Stop_FromIdle_DoesNotThrow()
        {
            var player = new CutscenePlayer("test", null, "key");
            Assert.DoesNotThrow(() => player.Stop());
        }

        [Test]
        public void Player_Pause_FromIdle_DoesNotThrow()
        {
            var player = new CutscenePlayer("test", null, "key");
            Assert.DoesNotThrow(() => player.Pause());
        }

        [Test]
        public void Player_SeekTo_DoesNotThrow()
        {
            var player = new CutscenePlayer("test", null, "key");
            Assert.DoesNotThrow(() => player.SeekTo(5.0));
        }

        [Test]
        public void Player_DoubleStop_DoesNotThrow()
        {
            var player = new CutscenePlayer("test", null, "key");
            player.Stop();
            Assert.DoesNotThrow(() => player.Stop());
        }

        [Test]
        public void Player_EmptyBindingMap_DoesNotThrow()
        {
            var map = new CutsceneBindingMap();
            var player = new CutscenePlayer("test", map, "key");
            Assert.DoesNotThrow(() => player.Stop());
        }
    }
}